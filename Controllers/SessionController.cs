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

namespace AuthApi.Controllers
{

    [ApiController]
    [Route("api/session")]
    public class SessionController(ILogger<SessionController> logger, DirectoryDBContext directoryDBContext, PersonService personService, SessionService sessionService ) : ControllerBase
    {
        private readonly ILogger<SessionController> _logger = logger;
        private readonly DirectoryDBContext directoryDBContext = directoryDBContext;
        private readonly PersonService personService = personService;
        private readonly SessionService sessionService = sessionService;
        

        [HttpPost]
        [Route("auth")]
        public IActionResult AuthenticateTest([FromBody] AuthenticateRequest authenticateRequest)
        {
            
            //  * Validate request
            if( !ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var _persons = this.personService.Search( authenticateRequest.Email!, null, null );
            if( _persons.IsNullOrEmpty()){
                return NotFound(new {
                    Message = "El correo no se encuentra registrado en la base de datos"
                });
            }
            var person = this.personService.AuthPerson( authenticateRequest.Email!, authenticateRequest.Password!);
            if( person == null){
                return Unauthorized(new {
                    Message = "Usuario y/o contraseña incorrectos"
                });
            }

            // * Create a session record
            try{
                string ipAddress = HttpContext.Connection.RemoteIpAddress!.ToString();
                string userAgent = Request.Headers["User-Agent"].ToString();
                var SessionToken = sessionService.StartPersonSession( person, ipAddress, userAgent);

                // Set the cookie
                Response.Cookies.Append("FGJTamSession", SessionToken, new CookieOptions
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


        [HttpGet]
        [Route("me")]
        public IActionResult GetSessionPerson()
        {
            // Retrieve the cookie value by name
            var cookieValue = Request.Cookies["FGJTamSession"];

            // Check if the cookie value is null
            if (cookieValue == null)
            {
                // If the cookie value is null, return a BadRequest response
                return BadRequest( new {
                    Message = "Sesion cookie no encontrado."
                });
            }

            try{
                var person = this.sessionService.GetPersonSession( cookieValue);
                return Ok(new {
                    person
                });
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

    }
}