using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Entities;
using AuthApi.Data.Exceptions;
using AuthApi.Validators.AccountRecovery;
using AuthApi.Models.Responses;
using AuthApi.Services;
using Microsoft.AspNetCore.Components.Web;
using System.Net;

namespace AuthApi.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class AccountRecoveryController(ILogger<AccountRecoveryController> logger, RecoveryAccountService ras, MinioService ms, CatalogService cs) : ControllerBase
    {
        private readonly ILogger<AccountRecoveryController> _logger = logger;
        private readonly RecoveryAccountService recoveryAccountService = ras;
        private readonly MinioService minioService = ms;
        private readonly CatalogService catalogService = cs;

        /// <summary>
        /// List the accounts recovery request
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get the list of recovery request</response>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<AccountRecoveryResponse>>> ListAccountRecoveryRequest()
        {
            // * get data
            var data = await this.recoveryAccountService.GetAllRecords();
            return data.ToList();
        }


        /// <summary>
        /// Get the recovery request with temporally url of the files
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get the data</response>
        /// <response code="400">The request is not valid</response>
        [HttpGet("{accountRecoveryUUID}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<AccountRecoveryResponse>> GetWithDocs([FromRoute] string accountRecoveryUUID)
        {
            var parseCorrect = Guid.TryParse(accountRecoveryUUID, out Guid requestUUID);
            if(!parseCorrect){
                return BadRequest( new {
                    Title = "El formato del accountRecoveryUUID es incorrecto",
                    Message = "El formato del accountRecoveryUUID es incorrecto, se espera un UUID"
                });
            }

            // * get data
            return await this.recoveryAccountService.GetRequestWithFiles(requestUUID);
            
        }



        /// <summary>
        /// Register a new account recovery request
        /// </summary>
        /// <returns></returns>
        /// <response code="201">Stored the recovery request</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="409">Some error at attempt to store the request</response>
        /// <response code="422">The request params are not valid</response>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UnprocesableResponse), StatusCodes.Status422UnprocessableEntity)]
        public IActionResult RegisterNewRequest([FromBody] AccountRecoveryRequest request)
        {
            // * validate the request
            var validationResults = new NewAccountRecoberyValidator().Validate(request);
            if(!validationResults.IsValid){
                return UnprocessableEntity( new UnprocesableResponse(validationResults.Errors));
            }

            // * store the request
            try {
                var recoveryRequest = this.recoveryAccountService.RegisterRecoveryAccountRequest(request);
                return StatusCode(201, recoveryRequest);
            }catch(SimpleValidationException sve){
                return UnprocessableEntity( new UnprocesableResponse(sve.ValidationErrors.ToDictionary()));
            }catch(Exception err){
                return Conflict( new {
                    Title = "Error al registrar la peticion",
                    err.Message
                });
            }
        }


        /// <summary>
        /// Upload a file for the account recovery
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/AccountRecovery/{accountRecoveryUUID}/file
        ///     Content-Type: multipart/form-data
        ///     Authorization: Bearer {auth-token}
        ///
        /// **Body Parameters:**
        /// - **documentTypeId**: (int, required)
        /// - **file**: (file, required)
        ///
        /// </remarks>
        /// <returns></returns>
        /// <response code="201">The file was uploaded</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="404">The request id was not found on the system</response>
        [HttpPost("{accountRecoveryUUID}/file")]
        [Consumes(MediaTypeNames.Multipart.FormData)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AccountRecoveryFile), StatusCodes.Status201Created)]
        public async Task<ActionResult<AccountRecoveryFileResponse>> UploadFile([FromRoute] string accountRecoveryUUID, [FromForm] IFormFile file, [FromForm] int documentTypeId)
        {
            var parseCorrect = Guid.TryParse(accountRecoveryUUID, out Guid requestUUID);
            if(!parseCorrect){
                return BadRequest( new {
                    Title = "El formato del accountRecoveryUUID es incorrecto",
                    Message = "El formato del accountRecoveryUUID es incorrecto, se espera un UUID"
                });
            }

            // * attempt to get the recovery request
            var recoveryRequest = this.recoveryAccountService.GetByID(requestUUID);
            if(recoveryRequest == null){
                return NotFound(new {
                    Title = "No se econtro el registro en la base de datos.",
                    Message = "No se econtro el registro en la base de datos."
                });
            }

            // * get the documentType
            DocumentType? documentType1 = null;
            try
            {
                documentType1 = catalogService.GetDocumentTypes().FirstOrDefault(item => item.Id == documentTypeId);
                if(documentType1 == null){
                    var errors = new Dictionary<string,string> { {"documentTypeId", "No se encontro el tipo de documento"}};
                    return UnprocessableEntity(new UnprocesableResponse(errors));
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error at attempting to get the document types catalog: {message}", ex.Message);
                return Conflict( new {
                    Title = "Error al conectarse al servidor",
                    Message = "Error al conectarse al servidor, " + ex.Message,
                });
            }

            // * upload the file
            string filePath = "";
            using(var stream = file.OpenReadStream()){
                filePath = await minioService.UploadFile( file.FileName, stream, $"accountRecoveryFiles/{accountRecoveryUUID}/");
            }
            if(string.IsNullOrEmpty(filePath)){
                throw new Exception("Fail at uplaod the file");
            }

            // * make the file record
            var recoveryAccountFile = new AccountRecoveryFile(){
                FileName = file.FileName,
                FilePath = filePath,
                FileType = file.ContentType,
                FileSize = file.Length,
                DocumentType = documentType1
            };
            this.recoveryAccountService.AttachFile(recoveryRequest, recoveryAccountFile);

            // * return response
            return StatusCode(201, AccountRecoveryFileResponse.FromEntity(recoveryAccountFile));
        }

    }
}