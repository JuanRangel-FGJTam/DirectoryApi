using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Entities;
using AuthApi.Data.Exceptions;
using AuthApi.Validators.Users;
using AuthApi.Models.Responses;


namespace AuthApi.Controllers
{

    /// <summary> </summary>
    [Route("user")]
    public class UserController(ILogger<UserController> logger, IUserService service, DirectoryDBContext dbContext) : ControllerBase
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly IUserService userService = service;
        private readonly DirectoryDBContext directoryDBContext = dbContext;

        /// <summary>
        /// Validate the user and generate a JWT token for authentication
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <response code="201">Stored the person</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest model)
        {
            // Validate request
            if( !ModelState.IsValid){
                // Process errors
                var _errorsMessages = new List<KeyValuePair<string,string>>();
                foreach( var error in ModelState)
                {
                    _errorsMessages.Add( new KeyValuePair<string, string>( error.Key, error.Value.Errors.First().ErrorMessage ) );
                }
                
                // Return bad request
                return BadRequest( new {
                    title = "Request validations fail",
                    message = "The request is not valid",
                    errors = _errorsMessages
                }  );
            }

            var response = await userService.Authenticate(model);

            if (response == null){
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            return Ok(response);
        }

        /// <summary>
        /// Stoew new user
        /// </summary>
        /// <param name="userRequest"></param>
        /// <returns></returns>
        /// <response code="200">New user stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="422">Validations request fail</response>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserRequest userRequest)
        {

            if( !ModelState.IsValid){
                // Process erroros
                var _errorsMessages = new List<KeyValuePair<string,string>>();
                foreach( var error in ModelState  ){
                    _errorsMessages.Add( new KeyValuePair<string, string>( error.Key, error.Value.Errors.First().ErrorMessage ) );
                }
                
                // Return bad request
                return UnprocessableEntity( new {
                    title = "Request validations fail",
                    message = "The request is not valid",
                    errors = _errorsMessages
                }  );
            }

            try
            {
                var userId = await userService.CreateUser(userRequest);
                return Ok( new{
                    title = "User created successfully",
                    user_id = userId
                });
            }
            catch (SimpleValidationException validationException)
            {
                return UnprocessableEntity( new {
                    title = "Request validations fail",
                    message = validationException.Message,
                    errors = validationException.ValidationErrors
                });
            }
        }

        /// <summary>
        /// Update an user
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userUpdateRequest"></param>
        /// <returns></returns>
        /// <response code="200">User udpated or stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="422">Validations request fail</response>
        [Authorize]
        [HttpPut("{userID}")]
        public async Task<IActionResult> Put( [FromRoute] int userID, [FromBody] UserUpdateRequest userUpdateRequest )
        {
            // Validate request
            if( !ModelState.IsValid){
                // Process errors
                var _errorsMessages = new List<KeyValuePair<string,string>>();
                foreach( var error in ModelState ){
                    if( !error.Value.Errors.IsNullOrEmpty()){
                        _errorsMessages.Add( new KeyValuePair<string, string>( error.Key, error.Value.Errors.First().ErrorMessage ) );
                    }
                }
                
                // Return bad request
                return UnprocessableEntity( new {
                    title = "Request validations fail",
                    message = "The request is not valid",
                    errors = _errorsMessages
                }  );
            }

            try {
                var userUpdated = await userService.UpdateUser( userID, userUpdateRequest );
                return Ok( new{
                    title = "User updated successfully",
                    user_id = userUpdated!.Id
                });
            }
            catch (SimpleValidationException validationException)
            {
                return UnprocessableEntity( new {
                    title = "Request validations fail",
                    message = validationException.Message,
                    errors = validationException.ValidationErrors
                });
            }

        }


        /// <summary>
        /// Seek in the database if the email is already stored in the database and if exist return de his datra.
        /// </summary>
        /// <param name="email"></param>
        /// <response code="200">Return the user related with the email</response>
        /// <response code="404">There is no user with the email searched</response>
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchUser([FromQuery] string email){

            var user = await this.userService.GetByEmail(email);

            if( user == null){
                return NotFound( new {
                    Message = "User not found"
                });
            }

            return Ok( new {
                Id = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email
            });
        }
    

        /// <summary>
        /// return the list of users
        /// </summary>
        /// <param name="orderBy"> propertie name used for ordering by default 'createdAt' posibles [ "firstName", "email"] </param>
        /// <param name="ascending"></param>
        /// <param name="take"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <response code="200">List of users</response>
        /// <response code="200">The auth token is missing or is invalid</response>
        /// <response code="409">Fail at get the data</response>
        [Authorize]
        [HttpGet("")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<User>> GetUsers([FromQuery] int take = 25, [FromQuery] int offset = 0, [FromQuery] string orderBy = "firstName", [FromQuery] bool ascending = false ){
            try
            {
                // * ordering the data
                string ordering = ascending ? $"{orderBy} asc" : $"{orderBy} desc";

                // * get data
                // TODO: Fix infinity loop of relations (UserRoles, UserClaims) at serialice the response on json
                var users = await this.directoryDBContext.Users
                    // .Include( u => u.UserRoles)
                    // .Include( u => u.UserClaims)
                    .OrderBy(ordering)
                    .Skip(offset)
                    .Take(take)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Fail at attempting to get the roles: {message}", ex.Message);
                return Conflict();
            }
        }

        /// <summary>
        /// return the list of roles availables
        /// </summary>
        /// <returns></returns>    
        /// <response code="200">List of the roles</response>
        /// <response code="200">The auth token is missing or is invalid</response>
        /// <response code="409">Fail at get the data</response>
        [Authorize]
        [HttpGet("roles-availables")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetRolesAvailables(){
            try
            {
                var roles = await this.userService.GetRolesAvailables();
                return Ok( roles);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Fail at attempting to get the roles: {message}", ex.Message);
                return Conflict();
            }
        }


        /// <summary>
        /// Add a role to the user
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userRoleRequest"></param>
        /// <returns></returns>
        /// <response code="200">User udpated or stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="404">The user is not found</response>
        /// <response code="422">Parameter are missing or are invalid</response>
        [Authorize]
        [HttpPut("{userID}/role")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UnprocesableResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AddRole( [FromRoute] int userID, [FromBody] UserRoleRequest userRoleRequest)
        {
            
            User? user = await this.userService.GetById(userID);
            if(user == null)
            {
                return NotFound( new {
                    Title = "El usuario no se encuentra registrado",
                    Message = $"El usuario con id {userID} no se encuentra registrado en el sistema",
                });
            }

            // TODO: Validate if the current user has enough permission to add roles to another user.

            // Validate request
            var validationResults = new UserRoleValidator().Validate(userRoleRequest);
            if(!validationResults.IsValid){
                // Return bad request
                return UnprocessableEntity( new UnprocesableResponse(validationResults.Errors));
            }

            var targetRole = (await this.userService.GetRolesAvailables()).FirstOrDefault( item => item.Id == userRoleRequest.RoleId);
            if(targetRole == null){
                return UnprocessableEntity( new UnprocesableResponse( new Dictionary<string,string>{
                    {"RoleId", "El rol no existe o no esta disponible"}
                }));
            }


            // * assign the role to the user
            try
            {
                var results = await userService.AttachRoleToUser(user, targetRole!);
                return Ok( new {
                    Title = "Nuevo rol assignedo al usuario",
                    Messge = $"El rol '{targetRole.Name}' se asigno al usuario '{user.Email}'"
                });
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Fail at attempting to assign the role '{roleId}' to the user '{user}': {message}", userRoleRequest.RoleId, user.Id, ex.Message );
                return Conflict( new {
                    Title = "Request validations fail",
                    Message = $"Error al tratar de asignar el rol '{userRoleRequest.RoleId}' al usuario: {ex.Message}",
                });
            }

        }


        /// <summary>
        /// Remove the role of the user
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userRoleRequest"></param>
        /// <returns></returns>
        /// <response code="200">User udpated or stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="404">The user is not found</response>
        /// <response code="422">Parameter are missing or are invalid</response>
        [Authorize]
        [HttpDelete("{userID}/role")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UnprocesableResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RemoveRole( [FromRoute] int userID, [FromBody] UserRoleRequest userRoleRequest)
        {
            
            User? user = await this.userService.GetById(userID);
            if(user == null)
            {
                return NotFound( new {
                    Title = "El usuario no se encuentra registrado",
                    Message = $"El usuario con id {userID} no se encuentra registrado en el sistema",
                });
            }

            // TODO: Validate if the current user has enough permission to add roles to another user.

            // Validate request
            var validationResults = new UserRoleValidator().Validate(userRoleRequest);
            if(!validationResults.IsValid)
            {
                return UnprocessableEntity( new UnprocesableResponse(validationResults.Errors));
            }

            var targetRole = (await this.userService.GetRolesAvailables()).FirstOrDefault( item => item.Id == userRoleRequest.RoleId);
            if(targetRole == null)
            {
                return UnprocessableEntity( new UnprocesableResponse( new Dictionary<string,string>
                {
                    {"RoleId", "El rol no existe o no esta disponible"}
                }));
            }


            // * remove the role from the user
            try
            {
                var results = await userService.DetachRoleFromUser(user, targetRole!);
                this._logger.LogInformation( "Removed the role {role} of the user {user}", targetRole.ToString(), user.ToString());
                return Ok( new {
                    Title = "Rol eliminado del usuario",
                    Messge = $"El rol '{targetRole}' se removio del usuario '{user}'"
                });
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Fail at attempting to assign the role '{role}' to the user '{user}': {message}", userRoleRequest, user, ex.Message );
                return Conflict( new {
                    Title = "Request validations fail",
                    Message = $"Error al tratar de remover el rol '{userRoleRequest}' al usuario {user}: {ex.Message}",
                });
            }

        }

    }
}