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
        [HttpPost]
        public IActionResult RegisterUser( PreregistrationRequest request )
        {

            // Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest();
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
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<Preregistration>?> GetAllPreregisters()
        {
            return Ok( dbContext.Preregistrations.ToArray() );
        }

        
        [Route("{preregisterID}")]
        [HttpPost]
        public IActionResult ValidateRegister(Guid preregisterID, [FromBody] ValidateRegisterRequest request){
            try {

                // Retrive validation enity
                var preregister = this.dbContext.Preregistrations.Find(preregisterID);
                if( preregister == null){
                    return BadRequest(new {
                        message = "Preregister record was not found on the database"
                    });
                }

                var newPerson = this.preregisterService.ValidateRegister( preregister.Id, request );    
                if( newPerson != null){
                    return Ok( new {
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