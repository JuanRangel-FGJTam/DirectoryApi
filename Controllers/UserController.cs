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

namespace AuthApi.Controllers
{

    /// <summary> </summary>
    [Route("user")]
    public class UserController(ILogger<UserController> logger, IUserService service ) : ControllerBase
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly IUserService userService = service;

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
                foreach( var error in ModelState  ){
                    _errorsMessages.Add( new KeyValuePair<string, string>( error.Key, error.Value.Errors.First().ErrorMessage ) );
                }
                
                // Return bad request
                return BadRequest( new {
                    title = "Request validations fail",
                    message = "The request is not valid",
                    errors = _errorsMessages
                }  );
            }

            var response = await userService.Authenticate( model );

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
        [HttpPut("{userID}")]
        [CAuthorize]
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
        
    }
}