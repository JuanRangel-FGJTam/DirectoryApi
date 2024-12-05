using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Services;
using AuthApi.Models.Responses;

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
                // * Make the session
                var SessionToken = sessionService.StartPersonSession( person, authenticateRequest.IpAddress, authenticateRequest.UserAgent );

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
                directoryDBContext.Entry(person).Collection( e => e.Addresses!).Query().Where(item => item.DeletedAt == null).Load();
                directoryDBContext.Entry(person).Collection( e => e.ContactInformations!).Query().Where(item => item.DeletedAt == null ).Load();

                var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var json = JsonConvert.SerializeObject( PersonResponse.FromEntity(person!), camelSettings );
                return new ContentResult {
                    Content = json,
                    ContentType = "application/json; charset=utf-8",
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
                directoryDBContext.Entry(person).Collection( e => e.Addresses!).Query().Where(item => item.DeletedAt == null).Load();
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
                directoryDBContext.Entry(person).Collection( e => e.ContactInformations!).Query().Where(item => item.DeletedAt == null).Load();

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
        /// Get all the session ordered indescending by date.
        /// </summary>
        /// <param name="personId"> Person id in UUID format</param>
        /// <param name="take"> Elements to take, if its 0 then return all the data, otherwise return the n elements. </param>
        /// <param name="skip"></param>
        /// <response code="200">Get all the session</response>
        /// <response code="401">No authenticated</response>
        /// <response code="409">Error al retrive the session, verify the logs.</response>
        /// <response code="422">pesonId format is invalid, UUID required</response>
        [HttpGet]
        [Route("")]
        public ActionResult<SessionResponse> GetAllSessions( [FromQuery] string? personId, [FromQuery] int take = 25, [FromQuery] int skip = 0)
        {
            Guid personID = Guid.Empty;
            if( !string.IsNullOrEmpty(personId) ){
                if (!Guid.TryParse(personId, out personID) ){
                    return UnprocessableEntity( new {
                        Title = "One or more field had errors",
                        Errors = new {
                           personId =  "Formato no valido, UUID requerido."
                        }
                    });
                }
            }

            try {
                
                // * Get data
                var dataQuery = this.directoryDBContext.Sessions
                    .OrderByDescending( item => item.BegginAt)
                    .Where( item => item.Person.Id == (string.IsNullOrEmpty(personId) ?item.Person.Id :personID) )
                    .AsQueryable();
                    

                // * get the total sessions
                var total = dataQuery.Count();
            
                // * get data 

                IEnumerable<SessionResponse> data = [];
                if(take > 0){
                    data = dataQuery
                    .Take(take)
                    .Skip(skip)
                    .Select( s => SessionResponse.FromEntity(s))
                    .ToArray();
                }else {
                    data = dataQuery.Select(s => SessionResponse.FromEntity(s)).ToArray();
                }
            
                // * Return the data
                return Ok( new {
                    take,
                    skip,
                    total,
                    data
                });

            }
            catch (System.Exception ex) {
                this._logger.LogError(ex, "Fail to retrive the data");
                return Conflict(new{
                    Message = "Error no controlado al tratar de obtener las sesiones."
                });
            }
        }

    }
}