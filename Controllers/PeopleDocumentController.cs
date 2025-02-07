using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Services;
using Azure.Core;
using Minio;
using Minio.DataModel.Args;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Net;


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

            // TODO: validate if the file is note already stored

            // TODO: delete (softdelete) the previous file if exist a document with the same document type

            // * find the documentType
            DocumentType? documentType = dbContext.DocumentTypes.FirstOrDefault( item => item.Id == request.DocumentTypeId)
                ?? throw new ArgumentNullException("documentTypeId", "The document type is not found");

            
            // * store the file and create the record
            PersonFile newPersonFile;
            try
            {
                var _file = request.File!.First();
                // * create the new file
                newPersonFile = new PersonFile
                {
                    PersonId = _personID,
                    DocumentTypeId = documentType.Id,
                    FileName = request.Name !=null ?request.Name!.Trim() :null,
                    FilePath = "",
                    MimmeType = _file.ContentType,
                    FileSize = _file.Length,
                    Validation = request.Validation,
                    CreatedAt = DateTime.Now
                };


                // * store the file on minio
                using(Stream stream = request.File!.First().OpenReadStream())
                {
                    // * make the file name
                    var fileName = string.Format("personalDocuments/{0}/{1}-{2}.{3}",
                        _personID.ToString(),
                        documentType.Name.ToLower().Trim().Replace(" ", "_"),
                        DateTime.Now.Ticks,
                        _file.FileName.Split(".").Last()
                    );
                    newPersonFile.FilePath = await minioService.UploadFile( request.File!.First().FileName, stream, "/personaDocs");
                    // * store the file and retrive the path
                    // newPersonFile.FilePath = await minioService.UploadFile(fileName, stream);
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

            this._logger.LogCritical("H");

            return Created();
        }


        /// <summary>
        /// Retorna el listado de documentos personales de la persona.
        /// </summary>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <param name="ascending"></param>
        /// <param name="take"></param>
        /// <param name="offset"></param>
        /// <response code="200">return the data</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        /// <response code="404">The person is not found</response>
        [HttpGet]
        [Route("/api/people/{personId}/documents")]
        public async Task<ActionResult<IEnumerable<PersonDocumentResponse>>> GetPersonDocuments([FromRoute] string personId, [FromQuery] bool ascending = false, [FromQuery] int take = 5, [FromQuery] int offset = 0)
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
            if( !this.dbContext.People.Where(item => item.Id == _personID).Any()){
                return NotFound( new {
                    Message = "The person is not found"
                });
            }


            // * get files data
            var personFiles = this.dbContext.PersonFiles
                .Where(item=> item.PersonId == _personID)
                .OrderBy(item => item.CreatedAt)
                .Include(p=>p.DocumentType)
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
                personFilesResponse.Add(
                    new PersonDocumentResponse
                    {
                        Id = pf.Id,
                        PersonId = pf.PersonId,
                        FileName = pf.FileName,
                        FilePath = pf.FilePath,
                        MimmeType = pf.MimmeType,
                        FileSize = pf.FileSize,
                        DocumentTypeId = pf.DocumentTypeId,
                        DocumentTypeName = pf.DocumentTypeName,
                        CreatedAt = pf.CreatedAt,
                        DeletedAt = pf.DeletedAt,
                        FileUrl = fileUrl
                    }
                );
            }

            // * return the data
            return personFilesResponse;
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

        #endregion
    }
}
