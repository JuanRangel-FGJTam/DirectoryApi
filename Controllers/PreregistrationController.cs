using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using AuthApi.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Services;
using AuthApi.Data.Exceptions;
using AuthApi.Models;
using AuthApi.Entities;
using AuthApi.Data;

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
        public async Task<IActionResult> RegisterUser(PreregistrationRequest request)
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
                    Title = "Error no controlado al generar la solicitud.",
                    err.Message
                });
            }
        }

        /// <summary>
        /// Retrive all the preregisters stored
        /// </summary>
        /// <param name="orderBy"> propertie name used for ordering by default 'createdAt' posibles ["id", "mail", "createdAt", "validTo"] </param>
        /// <param name="ascending"></param>
        /// <param name="take"></param>
        /// <param name="offset"></param>
        /// <param name="search"></param>
        [HttpGet]
        public ActionResult<IEnumerable<Preregistration>?> GetAllPreregisters(
            [FromQuery] string orderBy = "createdAt",
            [FromQuery] bool ascending = false,
            [FromQuery] int take = 25,
            [FromQuery] int offset = 0,
            [FromQuery] string? search = null
        )
        {
            var peopleQuery = this.dbContext.Preregistrations.AsQueryable();

            if(string.IsNullOrEmpty(search) == false)
            {
                // * filter by search
                peopleQuery = peopleQuery.Where(item => 
                    EF.Functions.Like(item.Mail, $"%{search}%") ||
                    item.Token == search
                );
            }

            var totalRecords = peopleQuery.Count();

            // * ordering the data
            string ordering = ascending ? $"{orderBy} asc" : $"{orderBy} desc";
            var records = peopleQuery
                .OrderBy(ordering)
                .Skip(offset)
                .Take(take)
                .ToArray();


            // * make pagination
            var paginator = new
            {
                Total = totalRecords,
                Data = records,
                Filters = new
                {
                    Take = take,
                    Offset = offset,
                    OrderBy = orderBy,
                    Ascending = ascending
                }
            };

            return Ok(paginator);
        }


        /// <summary>
        /// Find the preregister record by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult GetPreregisterById([FromRoute] Guid id)
        {

            Console.WriteLine(">> ID :" + id.ToString());
            try
            {
                var data = this.dbContext.Preregistrations.FirstOrDefault( item => item.Id == id ) ?? throw new KeyNotFoundException("The preregister record was not found");
                return Ok(data);
            }
            catch (KeyNotFoundException)
            {

                return NotFound( new {
                    Title = "No se encontró el registro",
                    Message = "No se encontró el registro de prerregistro"
                });
            }
        }


        /// <summary>
        /// Delete the preregister record by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult DeletePreregister([FromRoute] Guid id)
        {
            // * get the records
            var record = this.dbContext.Preregistrations.FirstOrDefault(item => item.Id == id);
            if(record == null)
            {
                return NotFound( new {
                    Title = "No se encontró el registro",
                    Message = "No se encontró el registro de prerregistro"
                });
            }

            this.dbContext.Preregistrations.Remove(record);
            this.dbContext.SaveChanges();
            return Ok();
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
        ///       "appName" : string,
        ///       "ipAddress" : string,
        ///       "userAgent" : string,
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
        /// <response code="409">Internal error at attempt to generate the person.</response>
        /// <response code="410">The code is expired.</response>
        /// <response code="422">Some request params are not valid</response>
        [Route("validate")]
        [HttpPost]
        public IActionResult ValidateRegister([FromBody] ValidateRegisterRequest request){
            try {

                // * Validate token 
                var preregister =  this.preregisterService.GetPreregistrationByToken(request.Token!);
                if(preregister == null)
                {
                    return NotFound( new {
                        Title = "El código de prerregistro no es válido.",
                        Message = $"No se encontró registro que corresponda al código '{request.Token!}'."
                    });
                }

                // * validate the lifetime of the code
                if(preregister.ValidTo < DateTime.Now)
                {
                    return StatusCode(StatusCodes.Status410Gone, new { message = "El código ha caducado y ya no es válido." });
                }

                // * Store the person data 
                var newPerson = this.preregisterService.ValidateRegister(preregister.Id, request);
                if( newPerson == null)
                {
                    return Conflict( new {
                        Title = "No se puede registrar la persona.",
                        Message = "No se puede registrar la persona, el registro no se encuentra."
                    });
                }

                // * Create a session 
                string ipAddress = request.IpAddress ?? HttpContext.Connection.RemoteIpAddress!.ToString();
                string userAgent = request.UserAgent ?? Request.Headers.UserAgent.ToString();
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
                    Title = "Uno o mas campos tienen error.",
                    Errors = errorsData
                });
            }
            catch (System.Exception err) {
                return BadRequest( new {
                    Title = "Error no controlado al validar el registro.",
                    err.Message
                });
            }
        }


        /// <summary>
        /// Send a mail of welcoming if the email is stored in the db (for testing)
        /// </summary>
        /// <returns code="200">Mail sended to the server provider</returns>
        /// <returns code="404">Email not found</returns>
        /// <returns code="409">Error at attempting to send the mail to the server</returns>
        [Authorize]
        [HttpPost("send/mail/welcome/to/{email}")]
        public IActionResult SendWelcomeMail([FromRoute] string email)
        {
            // * attempt to find the person
            Person? person = dbContext.People.Where(item => item.Email == email).FirstOrDefault();
            if(person == null){
                return NotFound(new {
                    Title = "Email not found"
                });
            }

            // * send the email
            try {

                var sendEmailTask = preregisterService.SendWelcomeMail(person);
                sendEmailTask.Wait();

                // return the response
                return Ok( new {
                    Email = email,
                    EmailResponse = sendEmailTask.Result
                });

            }catch(Exception ex){
                return Conflict( new {
                    Title = "Error al enviar el mensage",
                    Message = ex.Message
                });
            }
        }

    }
}