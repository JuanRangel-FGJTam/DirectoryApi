using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Entities;
using AuthApi.Data.Exceptions;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthApi.Services;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AuthApi.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/session")]
    public class SessionController(ILogger<SessionController> logger, DirectoryDBContext directoryDBContext, PersonService personService, SessionService sessionService ) : ControllerBase
    {
        private readonly ILogger<SessionController> _logger = logger;
        private readonly DirectoryDBContext directoryDBContext = directoryDBContext;
        private readonly PersonService personService = personService;
        private readonly SessionService sessionService = sessionService;
        private static readonly string cookieName = "SessionToken";
        private static readonly string headerSessionName = "SessionToken";
        

        /// <summary>
        /// Authenticate the person and init a session
        /// </summary>
        /// <remarks>
        /// Sample response:
        /// 
        ///     {
        ///       Id: int,
        ///       Name: string,
        ///       SessionToken: string
        ///     }
        /// 
        /// </remarks>
        /// <param name="authenticateRequest"></param>
        /// <response code="200">Succsessfull stored the person</response>
        /// <response code="400">The request is not valid or are parameters missing</response>
        /// <response code="401">Credentials are not valid</response>
        /// <response code="409">Unhandle exception at created the session record</response>
        [HttpPost]
        [Route("auth")]
        public IActionResult AuthenticatePerson([FromBody] AuthenticateRequest authenticateRequest)
        {
            
            //  * Validate request
            if( !ModelState.IsValid){
                return BadRequest(ModelState);
            }

            // * Validate credentials
            var person = this.personService.AuthPerson( authenticateRequest.Email!, authenticateRequest.Password!);
            if( person == null){
                return Unauthorized(new {
                    Message = "Usuario y/o contraseña incorrectos"
                });
            }

            // * Create a session record
            try{
                // * Get the ipAddress and userAgent
                string ipAddress = Request.Headers.ContainsKey("X-Forwarded-For")
                ? Request.Headers["X-Forwarded-For"].ToString()
                : HttpContext.Connection.RemoteIpAddress!.ToString();
                string userAgent = Request.Headers["User-Agent"].ToString();

                // * Make the session
                var SessionToken = sessionService.StartPersonSession( person, ipAddress, userAgent);

                // * Set the cookie for the response
                Response.Cookies.Append( cookieName, SessionToken, new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict
                });

                // * Return all data
                return Ok(new{
                    Id = person.Id,
                    Name = person.FullName,
                    SessionToken
                });

            }catch(Exception err){
                this._logger.LogError(err, "" );
                return Conflict(
                    new {
                        Title = "Error al generar la session",
                        Message = "Error no controlado al tratar de iniciar la sesión."
                    }
                );
            }
        }


        /// <summary>
        /// Attempting to retrive the person data by the session token"
        /// </summary>
        /// <remarks>
        ///  NOTE: Will attempting to get the session token first by header request with the name "SessionToken", then from the query param "t" and by last by the cookie named "SessionToken" 
        /// </remarks>
        /// <response code="200">Succsessfull stored the person</response>
        /// <response code="400">The session cookie was not found</response>
        /// <response code="403">The session token is not valid or expired</response>
        /// <response code="409">Unhandle exception</response>
        [PersonSessionKey]
        [HttpGet]
        [Route("me")]
        public ActionResult<PersonResponse?> GetSessionPerson([FromQuery] string? t)
        {
            // * Retrieve the sessionToken by the header, query param or cookie value
            string sessionToken = (string) HttpContext.Items["session_token"]!;

            // * Attempt to get the person data
            try{
                var person = sessionService.GetPersonSession(sessionToken) ?? throw new Exception("La respuesta es nula");
                directoryDBContext.Entry(person).Reference( e => e.Gender).Load();
                directoryDBContext.Entry(person).Reference( e => e.MaritalStatus).Load();
                directoryDBContext.Entry(person).Reference( e => e.Nationality).Load();
                directoryDBContext.Entry(person).Reference( e => e.Occupation).Load();
                directoryDBContext.Entry(person).Collection( e => e.Addresses!).Load();
                directoryDBContext.Entry(person).Collection( e => e.ContactInformations!).Load();

                var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var json = JsonConvert.SerializeObject( PersonResponse.FromEntity(person!), camelSettings );
                return new ContentResult {
                    Content = json,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                

            }catch(SessionNotValid sbv){
                _logger.LogError(sbv, "Session token not valid");
                return StatusCode( 403, new { sbv.Message });
            }catch(Exception err){
                _logger.LogError(err, "Error no controlado al obtener los datos de la sesion");
                return Conflict( new {
                    Message = "Error no controlado al obtener los datos de la sesion"
                });
            } 
        }


        /// <summary>
        /// Retrive the addresses of the person assigned to the session token
        /// </summary>
        /// <remarks>
        ///  NOTE: Will attempting to get the session token first by header request with the name "SessionToken", then from the query param "t" and by last by the cookie named "SessionToken" 
        /// </remarks>
        /// <response code="200">Return the data</response>
        /// <response code="400">The session cookie was not found</response>
        /// <response code="403">The session token is not valid or expired</response>
        /// <response code="409">Unhandle exception</response>
        [PersonSessionKey]
        [HttpGet]
        [Route("me/addresses")]
        public ActionResult<IEnumerable<AddressResponse>?> GetAddressesBySession([FromQuery] string? t)
        {
            // * Retrieve the sessionToken by the header, query param or cookie value
            string sessionToken = (string) HttpContext.Items["session_token"]!;

            // * Attempt to get the person data
            try{

                var person = sessionService.GetPersonSession(sessionToken) ?? throw new Exception("La respuesta es nula");
                directoryDBContext.Entry(person).Collection( e => e.Addresses!).Load();
                return Ok( (person.Addresses??[]).Select( addr => AddressResponse.FromEntity(addr)) );

            }catch(SessionNotValid sbv){
                _logger.LogError(sbv, "Session token not valid");
                return StatusCode( 403, new { sbv.Message });
            }catch(Exception err){
                _logger.LogError(err, "Error no controlado al obtener los datos de la sesion");
                return Conflict( new {
                    Message = "Error no controlado al obtener los datos de la sesion"
                });
            } 
        }


        /// <summary>
        /// Update the password
        /// </summary>
        /// <remarks>
        ///  NOTE: Will attempting to get the session token first by header request with the name "SessionToken", then from the query param "t" and by last by the cookie named "SessionToken" 
        /// </remarks>
        /// <response code="200">Return the data</response>
        /// <response code="400">The session cookie was not found</response>
        /// <response code="403">The session token is not valid or expired</response>
        /// <response code="409">Unhandle exception</response>
        [PersonSessionKey]
        [HttpPatch]
        [Route("me/password")]
        public ActionResult<IEnumerable<AddressResponse>?> UpdateThePassword([FromQuery] string? t, [FromBody] UpdatePasswordRequest updatePasswordRequest)
        {
            
            // * Retrieve the sessionToken by the header, query param or cookie value
            string sessionToken = (string) HttpContext.Items["session_token"]!;
            
            // * Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest( ModelState );
            }

            // * Attempt to get the person data
            try {
                // Get the person
                var person = sessionService.GetPersonSession(sessionToken) ?? throw new Exception("La respuesta es nula");

                this.personService.UpdateThePassword( person.Id, updatePasswordRequest.OldPassword!, updatePasswordRequest.NewPassword!);

                return Ok( new {
                    Title = "Contraseña actualizada",
                    Message = "Contraseña actualizada"
                });

            }catch(SessionNotValid sbv){
                _logger.LogError(sbv, "Session token not valid");
                return StatusCode( 403, new { sbv.Message });
            }catch(ValidationException ex){
                _logger.LogError(ex, "Old password is not valid");
                var errors = new List<object> {
                    new { OldPassword = "La contraseña anterior no coincide" }
                };
                return UnprocessableEntity(new { Errors = errors });
            }catch(Exception err){
                _logger.LogError(err, "Error no controlado al obtener los datos de la sesion");
                return Conflict( new {
                    Message = "Error no controlado al obtener los datos de la sesion"
                });
            } 
        }


        /// <summary>
        /// Retrive the contact information of the person assigned to the session token
        /// </summary>
        /// <remarks>
        ///  NOTE: Will attempting to get the session token first by header request with the name "SessionToken", then from the query param "t" and by last by the cookie named "SessionToken" 
        /// </remarks>
        /// <response code="200">Return the contact information</response>
        /// <response code="400">The session cookie was not found</response>
        /// <response code="403">The session token is not valid or expired</response>
        /// <response code="409">Unhandle exception</response>
        [PersonSessionKey]
        [HttpGet]
        [Route("me/contactInformations")]
        public ActionResult<IEnumerable<ContactResponse>?> GetContactInformationBySession([FromQuery] string? t)
        {
            // * Retrieve the sessionToken by the header, query param or cookie value
            string sessionToken = (string) HttpContext.Items["session_token"]!;

            // * Attempt to get the person data
            try{

                var person = sessionService.GetPersonSession(sessionToken) ?? throw new Exception("La respuesta es nula");
                directoryDBContext.Entry(person).Collection( e => e.ContactInformations!).Load();
                return Ok( (person.ContactInformations??[]).Select( cont => ContactResponse.FromEntity(cont)) );

            }catch(SessionNotValid sbv){
                _logger.LogError(sbv, "Session token not valid");
                return StatusCode( 403, new { sbv.Message });
            }catch(Exception err){
                _logger.LogError(err, "Error no controlado al obtener los datos de la sesion");
                return Conflict( new {
                    Message = "Error no controlado al obtener los datos de la sesion"
                });
            } 
        }

    

        /// <summary>
        /// Get all the sessions data
        /// </summary>
        /// <response code="200">Get all the session</response>
        /// <response code="401">No authenticated</response>
        [HttpGet]
        [Route("")]
        public IActionResult GetAllSessions( int take = 10, int skip = 0)
        {
            // * Get data
            var data = this.directoryDBContext.Sessions
                .OrderByDescending( item => item.BegginAt)
                .Take(50)
                .Skip(0)
                .ToArray();

            // * Get the total sessions
            var total = directoryDBContext.Sessions.Count();
          
            // * Return the data
            return Ok( new {
                take,
                skip,
                total,
                data
            });
        }

    }
}