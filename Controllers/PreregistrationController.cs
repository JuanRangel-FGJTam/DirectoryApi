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
    public class PreregistrationController(DirectoryDBContext context, PreregisterService preregisterService, ILogger<PreregistrationController> logger) : ControllerBase
    {
        private readonly DirectoryDBContext dbContext = context;
        private readonly PreregisterService preregisterService = preregisterService;
        private readonly ILogger<PreregistrationController> logger = logger;


        /// <summary>
        /// </summary>
        /// <response code="201">Succsessfull create the pre-register record</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        /// <response code="422">Some request params are not valid</response>
        [HttpPost]
        public IActionResult RegisterUser( PreregistrationRequest request )
        {

            // Validate request
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity( ModelState );
            }
    
            // Create the pre-register record
            try{
                var _preRegisterId = this.preregisterService.CreatePreregister(request);
                return Created("", new {
                    Id = _preRegisterId
                });
            }catch(SimpleValidationException ve){
                return UnprocessableEntity( ve.ValidationErrors );
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
        ///       "curp": "RAAE190394MTSNLL02",
        ///       "name": "Juan Salvador",
        ///       "firstName": "Rangel",
        ///       "lastName": "Almaguer",
        ///       "birthdate": "1993-12-17"
        ///     }
        /// 
        /// </remarks>
        /// <param name="preregisterID"></param>
        /// <param name="request"></param>
        /// <response code="201">Succsessfull stored the person</response>
        /// <response code="400">The request is not valid or are parameters missing</response>
        /// <response code="404">The preregister record was not found</response>
        /// <response code="422">Some request params are not valid</response>
        [Route("{preregisterID}")]
        [HttpPost]
        public IActionResult ValidateRegister(Guid preregisterID, [FromBody] ValidateRegisterRequest request){
            try {

                // Retrive validation enity
                var preregister = this.dbContext.Preregistrations.Find(preregisterID);
                if( preregister == null){
                    return NotFound(new {
                        message = "Preregister record was not found on the database"
                    });
                }

                var newPerson = this.preregisterService.ValidateRegister( preregister.Id, request );    
                if( newPerson != null){
                    return Created("", new {
                        personId = newPerson.Id.ToString(),
                        fullName = newPerson.FullName
                    });
                }

                return BadRequest( new {
                    message = "Cant register the person"
                });
            }
            catch( ValidationException ve){
                var errorsData = (Dictionary<string, object>) ve.Value!;
                return UnprocessableEntity(new {
                    Title = "One or more field had errors",
                    Errors = errorsData
                });
            }
            catch (System.Exception err) {
                return BadRequest( new {
                    message = err.Message
                });
            }

        }

    }
}