using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Entities;
using AuthApi.Data;
using AuthApi.Helper;
using Microsoft.AspNetCore.Authorization;
using AuthApi.Services;
using AuthApi.Data.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Controllers
{

    /// <summary></summary>
    [Authorize]
    [ApiController]
    [Route("api/pre-registration")]
    public class PreregistrationController(DirectoryDBContext context, PreregisterService preregisterService, ILogger<PreregistrationController> logger, SessionService sessionService ) : ControllerBase
    {
        private readonly DirectoryDBContext dbContext = context;
        private readonly PreregisterService preregisterService = preregisterService;
        private readonly SessionService sessionService = sessionService;
        private readonly ILogger<PreregistrationController> logger = logger;


        /// <summary>
        /// Pre register the user and send a email to verify his information
        /// </summary>
        /// <response code="201">Succsessfull create the pre-register record</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        /// <response code="422">Some request params are not valid</response>
        [HttpPost]
        public async Task<IActionResult> RegisterUser( PreregistrationRequest request )
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity( ModelState );
            }
    
            // Create the pre-register record
            try{
                var _preRegisterId = await this.preregisterService.CreatePreregister(request);
                return Created("", new {
                    Id = _preRegisterId
                });
            }catch(SimpleValidationException ve){
                return UnprocessableEntity( new {
                    errors = ve.ValidationErrors.ToDictionary()
                } );
            }catch(Exception err){
                logger.LogError(err, "Error at trying to generate a new preregistration record; {message}", err.Message );
                return BadRequest( new {
                    Title = "Error no controlado al generar la solicitud",
                    err.Message
                });
            }
        }

        /// <summary>
        /// Retrive all the preregisters stored
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<Preregistration>?> GetAllPreregisters()
        {
            return Ok( dbContext.Preregistrations.ToArray() );
        }

        /// <summary>
        /// Store the new person using the pre-register record for retriving the email and password
        /// </summary>
        /// <remarks>
        /// Sample request with the minal data required:
        /// 
        ///     POST api/people
        ///     {
        ///       "token": string,
        ///       "curp": "RAAE190394MTSNLL02",
        ///       "name": "Juan Salvador",
        ///       "firstName": "Rangel",
        ///       "lastName": "Almaguer",
        ///       "birthdate": "1993-12-17",
        ///       "appName" : string
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "personId": string,
        ///       "fullName": string
        ///       "sessionToken": string
        ///     }
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="201">Succsessfull stored the person</response>
        /// <response code="400">The request is not valid or are parameters missing</response>
        /// <response code="404">The preregister record was not found by matching the token passed by the request</response>
        /// <response code="422">Some request params are not valid</response>
        [Route("validate")]
        [HttpPost]
        public IActionResult ValidateRegister([FromBody] ValidateRegisterRequest request){
            try {

                // * Validate token 
                var preregister =  this.preregisterService.GetPreregistrationByToken(request.Token!);
                if( preregister == null){
                    return NotFound( new {
                        Title = "The preregister record was not found",
                        Message = $"The preregister record was not found with the token {request.Token!}"
                    });
                }

                // * Store the person data 
                var newPerson = this.preregisterService.ValidateRegister( preregister.Id, request );    
                if( newPerson == null){
                    return BadRequest( new {
                        Title = "Cant register the person",
                        Message = "Errot at create the person, the response was null"
                    });
                }

                // * Create a session 
                string ipAddress = HttpContext.Connection.RemoteIpAddress!.ToString();
                string userAgent = Request.Headers["User-Agent"].ToString();
                var sessionToken = sessionService.StartPersonSession( newPerson, ipAddress, userAgent );

                // * Return the data
                return Created("", new {
                    personId = newPerson.Id.ToString(),
                    fullName = newPerson.FullName,
                    sessionToken
                });

            }
            catch( ValidationException ve){
                var errorsData = (Dictionary<string, object>) ve.Value!;
                return UnprocessableEntity(new {
                    Title = "Uno o mas campos tienen error",
                    Errors = errorsData
                });
            }
            catch (System.Exception err) {
                return BadRequest( new {
                    Title = "Unhandle exception",
                    err.Message
                });
            }

        }

    }
}