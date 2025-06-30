using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Services;
using Minio.DataModel;


namespace AuthApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PeopleDocumentController(ILogger<PeopleDocumentController> logger, IConfiguration configuration, DirectoryDBContext context, MinioService minioService ) : ControllerBase
    {
        #region Injected properties
        private readonly ILogger<PeopleDocumentController> _logger = logger;
        private readonly IConfiguration configuration = configuration;
        private readonly DirectoryDBContext dbContext = context;
        private readonly MinioService minioService = minioService;
        #endregion

        private string FolderName {get;} = "personal_documents";


        /// <summary>
        ///  Almacena el documento de identidad de la persona
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/people/{personId}/documents
        ///     Content-Type: multipart/form-data
        ///     Authorization: Bearer {auth-token}
        ///
        /// **Body Parameters:**
        /// - **name**: (string, optional, max length: 250)
        /// - **documentTypeId**: (int, required)
        /// - **validation**: (string, optional, format: "yyyy-MM-dd")
        /// - **file**: (File, required) Actual file to upload.
        ///
        /// </remarks>
        /// <param name="personId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="201">Succsessfull create the new record</response>
        /// <response code="400">The request is not valid (person id is not GUID, the payload is not 'appliaction/json')</response>
        /// <response code="404">The person is not found</response>
        /// <response code="409">Internal error, the server id down</response>
        /// <response code="422">Some request params are not valid</response>
        [HttpPost]
        [Route("/api/people/{personId}/documents")]
        public async Task<IActionResult> StoreWithFile([FromRoute] string personId, [FromForm] NewPersonDocumentRequest request)
        {
            // * Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IFormFile _file = request.File![0];
            IFormFile? _fileBack = request.FileBack?[0];

            // * Validate size of the file (12mb)
            if(_file.Length > 1024 * 1024 * 12)
            {
                return UnprocessableEntity(new {
                    Title = "Uno o mas campos tienen error.",
                    Errors = new Dictionary<string, object> {
                        {"file", "El archivo es demasiado grande."}
                    }
                });
            }
            
            if(_fileBack != null)
            {
                if(_fileBack.Length > 1024 * 1024 * 12)
                {
                    return UnprocessableEntity( new {
                        Title = "Uno o mas campos tienen error.",
                        Errors = new Dictionary<string, object> {
                            {"fileBack", "El archivo es demasiado grande."}
                        }
                    });
                }
            }

            // * Validate person
            Guid _personID = Guid.Empty;
            try
            {
                _personID = ValidatePerson(personId);
            }
            catch(ArgumentException)
            {
                return BadRequest(new { message = $"Person id has formatted not valid" });
            }
            catch(KeyNotFoundException)
            {
                return NotFound(new { Message = "The person is not found" });
            }

            // TODO: delete (softdelete) the previous file if exist a document with the same document type

            // * find the documentType
            DocumentType? documentType = dbContext.DocumentTypes.FirstOrDefault(item => item.Id == request.DocumentTypeId)
                ?? throw new ArgumentNullException("documentTypeId", "The document type is not found");


            // * validate if the same document type and document number are not stored
            if(CheckIfDocumentNumberExist(_personID, documentType.Id, request.Folio!))
            {
                return UnprocessableEntity( new {
                    Title = "Uno o mas campos tienen error.",
                    Errors = new Dictionary<string, object> {{"identification_number", "Un documento con le mismo folio ya se encuentra registrado."}}
                });
            }

            // * validate if the file is note already stored by comparing the file size and the mime type
            if(CheckIfDocumentExist(_personID, documentType.Id, request.File!.First()))
            {
                return UnprocessableEntity( new {
                    Title = "Uno o mas campos tienen error.",
                    Errors = new Dictionary<string, object> {{"file", "El documento ya se encuentra registrado."}}
                });
            }

            // * store the file and create the record
            PersonFile newPersonFile;
            try
            {
                // * create the new file
                newPersonFile = new PersonFile
                {
                    PersonId = _personID,
                    DocumentTypeId = documentType.Id,
                    FileName = request.Name !=null ?request.Name!.Trim() :null,
                    FilePath = "",
                    MimmeType = _file.ContentType,
                    FileSize = _file.Length,
                    FileSizeBack = _fileBack?.Length,
                    Validation = request.Validation,
                    CreatedAt = DateTime.Now,
                    Folio = request.Folio
                };


                // * store the front file on minio
                using(Stream stream = request.File!.First().OpenReadStream())
                {
                    // * make the file name
                    var fileName = string.Format("{0}/{1}/{2}-{3}.{4}",
                        this.FolderName,
                        _personID.ToString(),
                        documentType.Name.ToLower().Trim().Replace(" ", "_"),
                        DateTime.Now.Ticks,
                        _file.FileName.Split(".").Last()
                    );

                    // * store the file and retrive the path
                    newPersonFile.FilePath = await minioService.UploadFile(fileName, stream, null);
                }

                // * store the back file on minio
                if(_fileBack != null)
                {
                    using(Stream stream = _fileBack.OpenReadStream())
                    {
                        // * make the file name
                        var fileName = string.Format("{0}/{1}/{2}-back-{3}.{4}",
                            this.FolderName,
                            _personID.ToString(),
                            documentType.Name.ToLower().Trim().Replace(" ", "_"),
                            DateTime.Now.Ticks,
                            _file.FileName.Split(".").Last()
                        );

                        // * store the file and retrive the path
                        newPersonFile.FilePathBack = await minioService.UploadFile(fileName, stream, null);
                    }
                }


                // * insert the record on the db
                dbContext.PersonFiles.Add(newPersonFile);
                dbContext.SaveChanges();
            }
            catch(Exception err)
            {
                this._logger.LogError(err, "Fail at store the file");
                return Conflict( new {
                    Message = err.Message
                });
            }

            // * return the data stored
            return StatusCode(201, new {
                PersonID = personId,
                Document = newPersonFile
            });
        }

        /// <summary>
        /// Retorna el listado de documentos personales de la persona.
        /// </summary>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <param name="ascending"></param>
        /// <param name="take"></param>
        /// <param name="offset"></param>
        /// <param name="withTrash">Especifica el traer documentos eliminados</param>
        /// <response code="200">return the data</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        /// <response code="404">The person is not found</response>
        [HttpGet]
        [Route("/api/people/{personId}/documents")]
        public async Task<ActionResult<IEnumerable<PersonDocumentResponse>>> GetPersonDocuments([FromRoute] string personId, [FromQuery] bool ascending = false, [FromQuery] int take = 5, [FromQuery] int offset = 0, [FromQuery] bool withTrash = false)
        {
            // * Validate person
            Guid _personID = Guid.Empty;
            try
            {
                _personID = ValidatePerson(personId);
            }
            catch(ArgumentException)
            {
                return BadRequest(new { message = $"Person id has formatted not valid" });
            }
            catch(KeyNotFoundException)
            {
                return NotFound(new { Message = "The person is not found" });
            }
            // * validate the person
            if(!this.dbContext.People.Where(item => item.Id == _personID).Any())
            {
                return NotFound(new { Message = "The person is not found" });
            }


            // * get files data
            var personFilesQuery = this.dbContext.PersonFiles
                .Where(item => item.PersonId == _personID)
                .OrderBy(item => item.CreatedAt)
                .Include(p => p.DocumentType)
                .AsQueryable();
            if (!withTrash) personFilesQuery = personFilesQuery.Where(item => item.DeletedAt == null);

            var personFiles = personFilesQuery
                .Skip(offset)
                .Take(take)
                .ToList()
                .Select(item => PersonDocumentResponse.FromEnity(item))
                .ToList();


            // * make temporal url for files
            var personFilesResponse = new List<PersonDocumentResponse>();
            foreach(var pf in personFiles)
            {
                var fileUrl = await minioService.MakeTemporalUrl(pf.FilePath!, pf.MimmeType ?? "image/jpeg");
                string? fileUrlBack = null;
                if( pf.FilePathBack != null)
                {
                    fileUrlBack = await minioService.MakeTemporalUrl(pf.FilePathBack!, pf.MimmeType ?? "image/jpeg");
                }
                
                personFilesResponse.Add(
                    new PersonDocumentResponse
                    {
                        Id = pf.Id,
                        PersonId = pf.PersonId,
                        FileName = pf.FileName,
                        FilePath = pf.FilePath,
                        FilePathBack = pf.FilePathBack,
                        MimmeType = pf.MimmeType,
                        FileSize = pf.FileSize,
                        FileSizeBack = pf.FileSizeBack,
                        DocumentTypeId = pf.DocumentTypeId,
                        DocumentTypeName = pf.DocumentTypeName,
                        CreatedAt = pf.CreatedAt,
                        DeletedAt = pf.DeletedAt,
                        Valid = pf.Valid,
                        Folio = pf.Folio,
                        FileUrl = fileUrl,
                        FileUrlBack = fileUrlBack
                    }
                );
            }

            // * return the data
            return personFilesResponse;
        }

        /// <summary>
        /// Delete the document of the person by  matching the document id and the document name
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="200">return the data</response>
        /// <response code="404">Document not found</response>
        [HttpDelete]
        [Route("/api/people/{personId}/documents")]
        public async Task<ActionResult<IEnumerable<PersonDocumentResponse>>> DeletePersonDocument([FromRoute] string personId, [FromBody] PersonDocumentDeleteRequest request)
        {
            // * Validate person
            Guid _personID = Guid.Empty;
            try
            {
                _personID = ValidatePerson(personId);
            }
            catch(ArgumentException)
            {
                return BadRequest(new { message = $"Person id has formatted not valid" });
            }
            catch(KeyNotFoundException)
            {
                return NotFound(new { Message = "The person is not found" });
            }

            // * validate the person
            if(!this.dbContext.People.Where(item => item.Id == _personID).Any())
            {
                return NotFound(new { Message = "The person is not found" });
            }

            try
            {
                // * get file data
                var personFile = this.dbContext.PersonFiles
                    .FirstOrDefault(item=> item.PersonId == _personID && item.DeletedAt == null && item.Id == request.DocumentId && item.FileName == request.DocumentName)
                    ?? throw new KeyNotFoundException("The document is not found");
                
                personFile.DeletedAt = DateTime.Now;

                this.dbContext.PersonFiles.Update(personFile);
                await this.dbContext.SaveChangesAsync();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new {
                    Message = "El documento no se encuentra registrado."
                });
            }
            
            return Ok();
        }

        /// <summary>
        /// Retrive the las document of the person by documentType
        /// </summary>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <param name="documentTypeId"></param>
        /// <response code="200">return the data</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        /// <response code="404">The person or the document is not found</response>
        [HttpGet]
        [Route("/api/people/{personId}/document/{documentTypeId}")]
        public async Task<ActionResult<PersonDocumentResponse>> GetPersonDocumentByDocummentType([FromRoute] string personId, [FromRoute] int documentTypeId)
        {
            // * Validate person
            Guid _personID = Guid.Empty;
            try
            {
                _personID = ValidatePerson(personId);
            }
            catch(ArgumentException)
            {
                return BadRequest(new { message = $"Person id has formatted not valid" });
            }
            catch(KeyNotFoundException)
            {
                return NotFound(new { Message = "The person is not found" });
            }
            // * validate the person
            if(!this.dbContext.People.Where(item => item.Id == _personID).Any())
            {
                return NotFound(new { Message = "The person is not found" });
            }

            try
            {
                // * validate doccument type
                DocumentType? documentType = dbContext.DocumentTypes.FirstOrDefault( item => item.Id == documentTypeId) 
                    ?? throw new ArgumentNullException("documentTypeId", "The document type is not found");

                // * get files data
                var personFile = await this.dbContext.PersonFiles.OrderByDescending(item=>item.CreatedAt).FirstOrDefaultAsync(item=> item.PersonId == _personID && item.DeletedAt == null && item.DocumentTypeId == documentTypeId)
                    ?? throw new KeyNotFoundException("The document is not found");

                var personDocument = PersonDocumentResponse.FromEnity(personFile);
                personDocument.FileUrl = await minioService.MakeTemporalUrl(personDocument.FilePath!, personDocument.MimmeType!);
                if(personDocument.FilePathBack != null)
                {
                    personDocument.FileUrlBack = await minioService.MakeTemporalUrl(personDocument.FilePathBack!, personDocument.MimmeType!);
                }
                return personDocument;
            }

            catch (ArgumentNullException)
            {
                return UnprocessableEntity(new {
                    Title = "Uno o mas campos tienen error.",
                    Errors = new Dictionary<string, object> {{"documentTypeId", "El tipo de documento no es válido."}}
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound( new {
                    Message = "El documento no se encuentra registrado."
                });
            }
        }
        
        /// <summary>
        /// Retornat un documento en especifico
        /// </summary>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <param name="documentId"> identificador del documento a recuperar</param>
        /// <param name="withTrash">incluir documentos eliminados</param>
        /// <response code="200">retornar el documento</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        /// <response code="404">La persona o el documento no se encuentran</response>
        [HttpGet]
        [Route("/api/people/{personId}/documents/{documentId}")]
        public async Task<ActionResult<PersonDocumentResponse>> GetPersonDocuments([FromRoute] string personId, [FromRoute] string documentId, [FromQuery] bool withTrash = false)
        {
            // * Validate person
            Guid _personID = Guid.Empty;
            try
            {
                _personID = ValidatePerson(personId);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = $"Person id has formatted not valid" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "The person is not found" });
            }
            // * validate the person
            if(!this.dbContext.People.Where(item => item.Id == _personID).Any())
            {
                return NotFound(new { Message = "The person is not found" });
            }

            if (!int.TryParse(documentId, out int docId))
            {
                return BadRequest(new { message = $"DocumentId id has formatted not valid" });
            }


            // * get files data
            var personFile = this.dbContext.PersonFiles
            .Where(item => item.PersonId == _personID && item.Id == docId)
            .Include(p => p.DocumentType)
            .FirstOrDefault();

            if (personFile == null)
            {
                return NotFound(new {
                    Title = "No encontrado",
                    Message = $"El documento con id:{documentId} no se encuentra registrado."
                });
            }

            if (!withTrash && personFile.DeletedAt != null)
            {
                return NotFound(new {
                    Title = "No encontrado",
                    Message = $"El documento con id:{documentId} no se encuentra registrado."
                });
            }

            // * make temporal url for files
            var fileUrl = await minioService.MakeTemporalUrl(personFile.FilePath!, personFile.MimmeType ?? "image/jpeg");
            string? fileUrlBack = null;
            if( !string.IsNullOrEmpty(personFile.FilePathBack))
            {
                fileUrlBack = await minioService.MakeTemporalUrl(personFile.FilePathBack!, personFile.MimmeType ?? "image/jpeg");
            }

            // * make temporal url for files
            PersonDocumentResponse fileModel = PersonDocumentResponse.FromEnity(personFile);
            fileModel.FileUrl = fileUrl;
            fileModel.FileUrlBack = fileUrlBack;

            // * return the data
            return fileModel;
        }


        #region Private methods
        /// <summary>
        /// Validate the person id and if exist
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Person id has formatted not valid</exception>
        /// <exception cref="KeyNotFoundException">Person id has formatted not valid</exception>
        private Guid ValidatePerson(string personId)
        {
            Guid _personID = Guid.Empty;
            try
            {
                _personID = Guid.Parse( personId );
            }
            catch(Exception)
            {
                throw new ArgumentException("Person id has formatted not valid");
            }

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == _personID).Any())
            {
                throw new KeyNotFoundException("The person is not found");
            }
            return _personID;
        }

        private bool CheckIfDocumentExist(Guid personId, int documentTypeId, IFormFile file)
        {
            var exist = false;
            var personDocuments = this.dbContext.PersonFiles.Where(item => item.DocumentTypeId == documentTypeId && item.PersonId == personId && item.DeletedAt == null).ToList();
            foreach(var pd in personDocuments)
            {
                if(pd.FileSize == file.Length && pd.MimmeType == file.ContentType)
                {
                    exist = true;
                    break;
                }
            }
            return exist;
        }

        private bool CheckIfDocumentNumberExist(Guid personId, int documentTypeId, string documentName )
        {
            var personDocuments = this.dbContext.PersonFiles
                .Where(item => item.DocumentTypeId == documentTypeId && EF.Functions.Like(item.Folio, documentName) && item.DeletedAt == null)
                .ToList();
            return personDocuments.Any();
        }

        #endregion
    }
}
