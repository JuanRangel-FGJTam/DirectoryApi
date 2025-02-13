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


namespace AuthApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProcedureController(ILogger<ProcedureController> logger, IConfiguration configuration, DirectoryDBContext context, MinioService minioService ) : ControllerBase
    {
        #region Injected properties
        private readonly ILogger<ProcedureController> _logger = logger;
        private readonly IConfiguration configuration = configuration;
        private readonly DirectoryDBContext dbContext = context;
        private readonly MinioService minioService = minioService;
        #endregion


        /// <summary>
        /// Almacena los datos de los trámites que realiza el ciudadano a través de las diversas aplicaciones.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/people/{personId}/procedures
        ///     {
        ///         folio: string [required, max:100]
        ///         status: string|null [max:24],
        ///         area: string|null [max:100],
        ///         name: string [required, max:120],
        ///         observations: string|null [max:200],
        ///         denunciaId: string|null [max:100],
        ///         created_at: string|null [format('yyyy-MM-dd HH:mm')],
        ///         officeLocation: string|null,
        ///     }
        ///
        /// </remarks>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <param name="request"></param>
        /// <response code="201">Succsessfull create the new record</response>
        /// <response code="400">The request is not valid (person id is not GUID, the payload is not 'appliaction/json' )</response>
        /// <response code="404">The person is not found</response>
        /// <response code="422">Some request params are not valid</response>
        [HttpPost]
        [Route("/api/people/{personId}/procedures")]
        public IActionResult Store( [FromRoute] string personId, [FromBody] NewProceedingRequest request){
            
            // * Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // * Validate person id
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( personId );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id has formatted not valid"
                });
            }

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == _personID).Any()){
                return NotFound( new {
                    Message = "The person is not found"
                });
            }

            // * validate if the folio is already stored with the same status
            var exist = this.dbContext.Proceeding.Where(item => item.PersonId == _personID && item.Folio == request.Folio).Any();
            if(ValidateDuplicatedRecord(_personID, request, out string message))
            {
                return UnprocessableEntity(new {
                    Title = "Uno o mas campos tienen error",
                    Errors = new { Folio = message }
                });
            }

            // * find the status or create a new one
            ProceedingStatus? proceedingStatus = null;
            if( !string.IsNullOrEmpty(request.Status) ){
                proceedingStatus = dbContext.ProceedingStatus.Where(item => item.Name == request.Status).FirstOrDefault();
                if(proceedingStatus == null){
                    proceedingStatus = new ProceedingStatus {
                        Name = request.Status.Trim()
                    };
                    dbContext.ProceedingStatus.Add(proceedingStatus);
                    dbContext.SaveChanges();
                }
            }

            // * find the area or create a new one
            Area? area = null;
            if( !string.IsNullOrEmpty(request.Area) ){
                area = dbContext.Area.Where(item => item.Name == request.Area).FirstOrDefault();
                if(area == null){
                    area = new Area {
                        Name = request.Area.Trim()
                    };
                    dbContext.Area.Add(area);
                    dbContext.SaveChanges();
                }
            }

            // * parse datetime
            DateTime? datetime = null;
            if( request.CreatedAt != null){
                if( DateTime.TryParseExact(
                    request.CreatedAt,
                    "yyyy-MM-dd HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedDate
                )){
                    datetime = parsedDate;
                }
            }

            // * create the new resorce
            var newProceeding = new Proceeding {
                PersonId = _personID,
                Name = request.Name !=null ?request.Name!.Trim() :null,
                Folio = request.Folio!.Trim(),
                Status = proceedingStatus,
                Area = area,
                DenunciaId = request.DenunciaId!=null ? request.DenunciaId!.Trim() :null,
                Observations = request.Observations!=null ?request.Observations!.Trim() :null,
                CreatedAt = datetime ?? DateTime.Now,
                OfficeLocation = request.OfficeLocation
            };

            // * insert into db
            dbContext.Proceeding.Add( newProceeding );
            dbContext.SaveChanges();

            // Return response
            return Created("Proceeding stored", newProceeding);
        }

        /// <summary>
        /// Almacena los datos de los trámites que realiza el ciudadano a través de las diversas aplicaciones junto con los archivos
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/people/{personId}/procedures-files
        ///     Content-Type: multipart/form-data
        ///     Authorization: Bearer {auth-token}
        ///
        /// **Body Parameters:**
        /// - **folio**: (string, required, max length: 100)
        /// - **status**: (string, required, max length: 24)
        /// - **area**: (string, optional, max length: 100)
        /// - **name**: (string, required, max length: 120)
        /// - **observations**: (string, optional, max length: 200)
        /// - **denunciaId**: (string, optional, max length: 100)
        /// - **created_at**: (string, optional, format: "yyyy-MM-dd HH:mm")
        /// - **officeLocation**: (string, optional)
        /// - **file**: (Filea, required) Multiple files can be uploaded with keys as `file1`, `file2`, `fileN`.
        ///
        /// </remarks>
        /// <param name="personId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/people/{personId}/procedures-files")]
        public async Task<IActionResult> StoreWithFiles([FromRoute] string personId, [FromForm] NewProceedingRequest request){

            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            // * validate the files
            if( request.File == null || request.File?.Count == 0){
                return BadRequest( new {
                    Message = "The file are required, No files were found in the request"
                });
            }

            // * validate person id
            Guid _personID = Guid.Empty;
            try
            {
                _personID = Guid.Parse( personId );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id has formatted not valid"
                });
            }

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == _personID).Any()){
                return NotFound( new {
                    Message = "The person is not found"
                });
            }


            // * validate if the folio is already stored with the same status
            var exist = this.dbContext.Proceeding.Where(item => item.PersonId == _personID && item.Folio == request.Folio).Any();
            if(ValidateDuplicatedRecord(_personID, request, out string message))
            {
                return UnprocessableEntity(new {
                    Title = "Uno o mas campos tienen error",
                    Errors = new { Folio = message }
                });
            }


            dbContext.Database.BeginTransaction();

            // * insert the proceding
            Proceeding newProceeding;
            try {
                // * find the status or create a new one
                ProceedingStatus? proceedingStatus = null;
                if( !string.IsNullOrEmpty(request.Status) ){
                    proceedingStatus = dbContext.ProceedingStatus.Where(item => item.Name == request.Status).FirstOrDefault();
                    if(proceedingStatus == null){
                        proceedingStatus = new ProceedingStatus {
                            Name = request.Status.Trim()
                        };
                        dbContext.ProceedingStatus.Add(proceedingStatus);
                        dbContext.SaveChanges();
                    }
                }

                // * find the area or create a new one
                Area? area = null;
                if( !string.IsNullOrEmpty(request.Area) ){
                    area = dbContext.Area.Where(item => item.Name == request.Area).FirstOrDefault();
                    if(area == null){
                        area = new Area {
                            Name = request.Area.Trim()
                        };
                        dbContext.Area.Add(area);
                        dbContext.SaveChanges();
                    }
                }

                // * parse datetime
                DateTime? datetime = null;
                if( request.CreatedAt != null){
                    if( DateTime.TryParseExact(
                        request.CreatedAt,
                        "yyyy-MM-dd HH:mm",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime parsedDate
                    )){
                        datetime = parsedDate;
                    }
                }

                // * create the new resorce
                newProceeding = new Proceeding {
                    PersonId = _personID,
                    Name = request.Name !=null ?request.Name!.Trim() :null,
                    Folio = request.Folio!.Trim(),
                    Status = proceedingStatus,
                    Area = area,
                    DenunciaId = request.DenunciaId!=null ? request.DenunciaId!.Trim() :null,
                    Observations = request.Observations!=null ?request.Observations!.Trim() :null,
                    CreatedAt = datetime ?? DateTime.Now
                };

                // * insert into db
                dbContext.Proceeding.Add( newProceeding );
                dbContext.SaveChanges();


            }catch(Exception err){
                dbContext.Database.RollbackTransaction();
                return Conflict( new {
                    Message = err.Message
                });
            }


            // * insert the files
            var procedingFiles = new List<ProceedingFile>();
            try {

                // * verify if the buket is created
                await minioService.EnsureBuketIsCreated();
                
                // * upload the files to minio
                foreach(var uploadedFile in request.File!){
                    var _newProcedingFile = new ProceedingFile {
                        FileName = uploadedFile.FileName,
                        FilePath = "",
                        FileType = uploadedFile.ContentType,
                        FileSize = uploadedFile.Length,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        ProceedingId = newProceeding.Id,
                    };

                    // * store the file on minio
                    using var uploadFileStream = uploadedFile.OpenReadStream();
                    _newProcedingFile.FilePath = await minioService.UploadFile( uploadedFile.FileName, uploadFileStream);
                    
                    procedingFiles.Add( _newProcedingFile );
                }

                dbContext.ProceedingFiles.AddRange(procedingFiles);
                dbContext.SaveChanges();

            } catch(Exception err){
                dbContext.Database.RollbackTransaction();
                return Conflict( new {
                    Message = err.Message
                });
            }

            dbContext.Database.CommitTransaction();

            // * return the data stored
            return StatusCode(201, new {
                PersonID = personId,
                Data = new {
                    newProceeding.Id,
                    newProceeding.PersonId,
                    newProceeding.Name,
                    newProceeding.Folio,
                    newProceeding.Status,
                    newProceeding.Area,
                    newProceeding.DenunciaId,
                    newProceeding.Observations,
                    newProceeding.CreatedAt
                },
                Files = procedingFiles.Select( pf => new {
                    pf.Id,
                    OriginalName = pf.FileName,
                    pf.FilePath,
                    pf.FileType,
                    pf.FileSize,
                })
            });

        }


        /// <summary>
        /// Retorna el listado de tramites almacenados de la persona
        /// </summary>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <param name="orderBy"> propertie name used for ordering by default 'createdAt' posibles ["id", "name", "folio", "denunciaId","status","area", "createdAt"] </param>
        /// <param name="ascending"></param>
        /// <param name="take"></param>
        /// <param name="offset"></param>
        /// <response code="200">return the data</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        /// <response code="404">The person is not found</response>
        [HttpGet]
        [Route("/api/people/{personId}/procedures")]
        public async Task<ActionResult<IEnumerable<ProceedingResponse>>> GetPersonProcedings([FromRoute] string personId, [FromQuery] string orderBy = "createdAt", [FromQuery] bool ascending = false, [FromQuery] int take = 5, [FromQuery] int offset = 0){
            
            // * Validate person id
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( personId );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id has formatted not valid"
                });
            }

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == _personID).Any()){
                return NotFound( new {
                    Message = "The person is not found"
                });
            }

            // * get data
            var query = this.dbContext.Proceeding
                .Where(item=> item.PersonId == _personID)
                .Include(p=>p.Files)
                .Include(p => p.Status)
                .Include(p => p.Area)
                .AsQueryable();

            // * ordering the data
            string ordering = ascending ? $"{orderBy} asc" : $"{orderBy} desc";
            query = query.OrderBy(ordering).Skip(offset).Take(take);

            var proccedings = query
                .ToList<Proceeding>()
                .Select(item => ProceedingResponse.FromIdentity(item))
                .ToList<ProceedingResponse>();

            // * make temporal url for files
            foreach( var p in proccedings){
                if(!p.Files.Any()){
                    continue;
                }

                // * override the proceding file with the temporally url
                var fileTasks = p.Files.Select(async file => {
                    if( file.DeletedAt != null || string.IsNullOrEmpty(file.FilePath)){
                        return file;
                    }
                    var fileUrl = await minioService.MakeTemporalUrl(file.FilePath!, file.FileType??"application/pdf");
                    return new ProceedingFileResponse {
                        Id = file.Id,
                        FileName = file.FileName,
                        FilePath = file.FilePath,
                        FileType = file.FileType,
                        FileSize = file.FileSize,
                        CreatedAt = file.CreatedAt,
                        UpdatedAt = file.UpdatedAt,
                        FileUrl = fileUrl,
                        DeletedAt = file.DeletedAt
                    };
                }).ToList();
                
                p.Files = await Task.WhenAll(fileTasks);
            }

            // * return the data
            return proccedings;
        }

        /// <summary>
        ///  Actualiza el estatus y observación del procedimiento almacenado que coincida con la denunciaId y el personId
        /// </summary>
        /// <remarks>
        /// 
        /// Sample request:
        ///
        ///     PATCH /api/people/{personId}/procedures-files/{denunciaId}
        ///     Content-Type: application/json
        ///     Authorization: Bearer {auth-token}
        ///
        ///     POST /api/people/{personId}/procedures
        ///     {
        ///         status: string [required, max:24],
        ///         observations: string|null [max:200]
        ///     }
        ///
        /// </remarks>
        /// <param name="personId">person id</param>
        /// <param name="denunciaId">denuncia id </param>
        /// <param name="updateProceedingRequest"> request payload</param>
        /// <response code="201">Resource updated</response>
        /// <response code="400">The request is not valid (person id is not GUID, the payload is not 'appliaction/json' )</response>
        /// <response code="404">The person or the proceding its not found</response>
        /// <response code="409">Fail to update the proceding</response>
        [HttpPatch]
        [Route("/api/people/{personId}/procedures/{denunciaId}")]
        public IActionResult UpdateProcedingByCode([FromRoute] Guid personId, [FromRoute] string denunciaId, [FromBody] UpdateProceedingRequest updateProceedingRequest ) {

            #region Validate request

            // * retrive the procedding
            var procedding = this.dbContext.Proceeding.Where( p=> p.PersonId == personId && p.DenunciaId !=null && p.DenunciaId == denunciaId ).FirstOrDefault();
            if( procedding == null){
                return NotFound(new {
                    Message = "No se encontró registro que coincida con la persona y la denuncia."
                });
            }

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == personId).Any()){
                return NotFound( new {
                    Message = "The person is not found"
                });
            }

            // * validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            #endregion

            // * attempting to update the resource
            try {
                UpdateProceding(procedding, updateProceedingRequest, updateDenunciaId: false);
                return StatusCode(201, new {Message = $"Procedimiento persona:'{personId}' denuncia:'{denunciaId}' actualizado"} );
            }catch(Exception ex){
                this._logger.LogError(ex, "Fail to attemting to update the proceding id {id}", procedding.Id);
                return Conflict(new {
                    Message = "Error no controlado al actualizar",
                    ErrorMessage = ex.Message
                });
            }

        }

        /// <summary>
        ///  Actualiza el estatus y la observación del procedimiento e inserta uno o más archivos relacionados con el procedimiento.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/people/{personId}/procedures-files/{denunciaId}
        ///     Content-Type: multipart/form-data
        ///     Authorization: Bearer {auth-token}
        ///
        /// **Body Parameters:**
        /// - **status**: (string, required, max length: 24)
        /// - **observations**: (string, optional, max length: 200)
        /// - **officeLocation**: (string, optional, max length: 200)
        /// - **file**: (File, required) Multiple files can be uploaded with keys as `file1`, `file2`, `fileN`.
        ///
        /// </remarks>
        /// <param name="personId">person id</param>
        /// <param name="denunciaId">denuncia id </param>
        /// <param name="updateProceedingRequest"> request payload</param>
        /// <response code="201">Resource updated</response>
        /// <response code="400">The request is not valid (person id is not GUID, the payload is not 'appliaction/json' )</response>
        /// <response code="404">The person or the proceding its not found</response>
        /// <response code="409">Fail to update the proceding</response>
        [HttpPatch]
        [Route("/api/people/{personId}/procedures-files/{denunciaId}")]
        public async Task<IActionResult> UpdateProcedingWithFile([FromRoute] Guid personId, [FromRoute] string denunciaId, [FromForm] UpdateProceedingRequest updateProceedingRequest ) {

            #region validate request
            // * retrive the procedding
            var procedding = this.dbContext.Proceeding.Where( p=> p.PersonId == personId && p.DenunciaId !=null && p.DenunciaId == denunciaId ).FirstOrDefault();
            if( procedding == null){
                return NotFound(new {
                    Message = "No se encontró registro que coincida con la persona y la denuncia."
                });
            }

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == personId).Any()){
                return NotFound( new {
                    Message = "The person is not found"
                });
            }

            // * validate request
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            // * validate the files
            if( updateProceedingRequest.File == null || updateProceedingRequest.File?.Count == 0){
                return BadRequest( new {
                    Message = "The file are required, No files were found in the request"
                });
            }
            #endregion

            dbContext.Database.BeginTransaction();

            // * attempt to remove the old files
            try {
                var files = dbContext.ProceedingFiles
                    .Where( item => item.ProceedingId == procedding.Id && item.FilePath != null)
                    .ToList();
                await minioService.RemoveFiles( files.Select(item => item.FilePath!) );
                dbContext.ProceedingFiles.RemoveRange(files);
                dbContext.SaveChanges();

            } catch(Exception ex){
                dbContext.Database.RollbackTransaction();
                return Conflict( new {
                    Message = "Error no controlado al subir los archivos",
                    ErrorMessage = ex.Message
                });
            }

            // * upload the files
            var procedingFiles = new List<ProceedingFile>();
            try {
                // * verify if the buket is created
                await minioService.EnsureBuketIsCreated();
                
                // * upload the files to minio
                foreach(var uploadedFile in updateProceedingRequest.File!){
                    var _newProcedingFile = new ProceedingFile {
                        FileName = uploadedFile.FileName,
                        FilePath = "",
                        FileType = uploadedFile.ContentType,
                        FileSize = uploadedFile.Length,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        ProceedingId = procedding.Id,
                    };

                    // * store the file on minio
                    using var uploadFileStream = uploadedFile.OpenReadStream();
                    _newProcedingFile.FilePath = await minioService.UploadFile( uploadedFile.FileName, uploadFileStream);
                    
                    procedingFiles.Add( _newProcedingFile );
                }

                dbContext.ProceedingFiles.AddRange(procedingFiles);
                dbContext.SaveChanges();

            } catch(Exception ex){
                dbContext.Database.RollbackTransaction();
                return Conflict( new {
                    Message = "Error no controlado al subir los archivos",
                    ErrorMessage = ex.Message
                });
            }

            // * attempting to update the resource
            try {
                UpdateProceding(procedding, updateProceedingRequest, updateDenunciaId: false);
            }catch(Exception ex){
                this._logger.LogError(ex, "Fail to attemting to update the proceding id {id}", procedding.Id);
                dbContext.Database.RollbackTransaction();
                return Conflict(new {
                    Message = "Error no controlado al actualizar",
                    ErrorMessage = ex.Message
                });
            }

            dbContext.Database.CommitTransaction();
            return StatusCode(201, new {
                Message = $"Procedimiento persona:'{personId}' denuncia:'{denunciaId}' actualizado",
                Data = new {
                    procedding.Id,
                    procedding.PersonId,
                    procedding.Name,
                    procedding.Folio,
                    procedding.Status,
                    procedding.Area,
                    procedding.DenunciaId,
                    procedding.Observations,
                    procedding.CreatedAt
                },
                Files = procedingFiles.Select( pf => new {
                    pf.Id,
                    OriginalName = pf.FileName,
                    pf.FilePath,
                    pf.FileType,
                    pf.FileSize,
                })
            });

        }

        /// <summary>
        ///  Actualiza el estatus y la observación del procedimiento
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/people/{personId}/procedures
        ///     {
        ///         status: string [required, max:24],
        ///         observations: string|null [max:200],
        ///         denunciaId: string|null [max:100],
        ///         officeLocation: string|null [max:200]
        ///     }
        /// </remarks>
        /// <param name="procedingId">procedding id</param>
        /// <param name="updateProceedingRequest"> request payload</param>
        /// <response code="201">Resource udpated</response>
        /// <response code="400">The request is not valid (the payload is not 'appliaction/json', etc. )</response>
        /// <response code="404">The proceding element its not found</response>
        /// <response code="409">Fail to update the proceding</response>
        [HttpPatch]
        [Route("{procedingId:int}")]
        public IActionResult UpdateProcedingById([FromRoute] int procedingId, [FromBody] UpdateProceedingRequest updateProceedingRequest ) {

            // * retrive the procedding
            var procedding = this.dbContext.Proceeding.Where( p=> p.Id == procedingId).FirstOrDefault();
            if( procedding == null){
                return NotFound(new {Message = "No se encontro registro que concuerde con el Id"});
            }

            // * validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // * attempting to update the resource
            try {
                UpdateProceding(procedding, updateProceedingRequest);
                return StatusCode(201, new {Message = $"Procedimiento id '{procedingId}' actualizado"} );
            }catch(Exception ex){
                this._logger.LogError(ex, "Fail to attemting to update the proceding id {id}", procedingId);
                return Conflict(new {
                    Message = "Error no controlado al actualizar",
                    ErrorMessage = ex.Message
                });
            }

        }


        /// <summary>
        /// Retorna el listado de tramites almacenados de la persona agrupados por folio
        /// </summary>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <param name="take"></param>
        /// <param name="offset"></param>
        /// <response code="200">return the data</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        /// <response code="404">The person is not found</response>
        [HttpGet]
        [Route("/api/people/{personId}/procedures-folio")]
        public async Task<ActionResult<IEnumerable<ProceedingGroupResponse>>> GetPersonProcedingsFolio([FromRoute] string personId, [FromQuery] int take = 5, [FromQuery] int offset = 0)
        {
            // * Validate person id
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( personId );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id has formatted not valid"
                });
            }

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == _personID).Any()){
                return NotFound( new {
                    Message = "The person is not found"
                });
            }


            // * get the proceedings grouped
            var responseList = new List<ProceedingGroupResponse>();
            responseList = this.dbContext.Proceeding
                .Where(item=>item.PersonId == _personID)
                .GroupBy(item=> item.Folio)
                .Select( g => new ProceedingGroupResponse
                    {
                        Folio = g.Key!,
                        Name = string.Join(", ", g.GroupBy(p=>p.Name).Select(g => g.Key).ToArray()),
                        FirtRegister = g.OrderBy(item => item.CreatedAt).First().CreatedAt,
                        LastModification = g.OrderByDescending(item => item.CreatedAt).First().CreatedAt
                    }
                )
                .ToList<ProceedingGroupResponse>()
                .OrderByDescending(item => item.LastModification)
                .Skip(offset)
                .Take(take)
                .ToList();


            foreach( var groupedFolio in responseList)
            {
                // * get the procedings of the folio
                var proccedings = this.dbContext.Proceeding
                    .Where(item=> item.PersonId == _personID && item.Folio == groupedFolio.Folio)
                    .OrderBy(item => item.CreatedAt)
                    .Include(p=>p.Files)
                    .Include(p => p.Status)
                    .Include(p => p.Area)
                    .Select( item => ProceedingResponse.FromIdentity(item))
                    .ToList<ProceedingResponse>();

                // * make temporal url for files
                foreach( var p in proccedings)
                {
                    if(!p.Files.Any())
                    {
                        continue;
                    }

                    // * override the proceding file with the temporally url
                    var fileTasks = p.Files.Select(async file => {
                        if( file.DeletedAt != null || string.IsNullOrEmpty(file.FilePath)){
                            return file;
                        }
                        var fileUrl = await minioService.MakeTemporalUrl(file.FilePath!, file.FileType??"application/pdf");
                        return new ProceedingFileResponse {
                            Id = file.Id,
                            FileName = file.FileName,
                            FilePath = file.FilePath,
                            FileType = file.FileType,
                            FileSize = file.FileSize,
                            CreatedAt = file.CreatedAt,
                            UpdatedAt = file.UpdatedAt,
                            FileUrl = fileUrl,
                            DeletedAt = file.DeletedAt
                        };
                    }).ToList();
                    
                    p.Files = await Task.WhenAll(fileTasks);
                }


                groupedFolio.Proceedings = proccedings;

            }

            // * return the data
            return responseList;
        }

        #region Private methods
        private void UpdateProceding(Proceeding proceeding, UpdateProceedingRequest request, bool updateDenunciaId = true){
            
            // * find the status or create a new one
            ProceedingStatus? proceedingStatus = null;
            if( !string.IsNullOrEmpty(request.Status) ){
                proceedingStatus = dbContext.ProceedingStatus.Where(item => item.Name == request.Status).FirstOrDefault();
                if(proceedingStatus == null){
                    proceedingStatus = new ProceedingStatus {
                        Name = request.Status.Trim()
                    };
                    dbContext.ProceedingStatus.Add(proceedingStatus);
                    dbContext.SaveChanges();
                }
            }

            // * update the procceding
            proceeding.Status = proceedingStatus;

            if(!string.IsNullOrEmpty(request.DenunciaId) && updateDenunciaId)
            {
                proceeding.DenunciaId = request.DenunciaId.Trim();
            }

            if(!string.IsNullOrEmpty(request.Observations))
            {
                proceeding.Observations = request.Observations.Trim();
            }

            if(!string.IsNullOrEmpty(request.OfficeLocation))
            {
                proceeding.OfficeLocation = request.OfficeLocation?.Trim();
            }

            dbContext.Proceeding.Update(proceeding);
            dbContext.SaveChanges();
        }

        private bool ValidateDuplicatedRecord(Guid personId, NewProceedingRequest request, out string message )
        {
            message = string.Empty;

            // * validate if the folio is already stored with the same status
            var exist = this.dbContext.Proceeding.Where(item => item.PersonId == personId && item.Folio == request.Folio).Any();
            if(exist)
            {
                var lastRecord = this.dbContext.Proceeding
                    .OrderByDescending( item => item.CreatedAt)
                    .Include( item => item.Status)
                    .First( item => item.PersonId == personId && item.Folio == request.Folio);

                Console.WriteLine($"{lastRecord.Folio}, {lastRecord.Status?.Name} {request.Status}");
                
                if( lastRecord.Status?.Name == request.Status)
                {
                    message = "El folio ya se encuentra registrado en esta persona con el mismo estatus.";
                    return true;
                }
            }

            return false;

            // // * validate if the denuncia id is already stored
            // exist = this.dbContext.Proceeding.Where( item => item.PersonId == _personID && item.DenunciaId == request.DenunciaId && item.DenunciaId != null ).Any();
            // if(exist){
            //     return UnprocessableEntity(new {
            //         Title = "Uno o mas campos tienen error",
            //         Errors = new { Folio = "El ID de la denuncia ya se encuentra registrado en esta persona." }
            //     });
            // }
        }
        #endregion
    }
}