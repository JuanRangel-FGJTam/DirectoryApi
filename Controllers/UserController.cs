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
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var response = await userService.Authenticate( model );

            if (response == null){
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            return Ok(response);
        }

        /// <summary>
        /// Stoew new user
        /// </summary>
        /// <param name="userObj"></param>
        /// <returns></returns>
        /// <response code="200">New user stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpPost]
        [CAuthorize]
        public async Task<IActionResult> Post([FromBody] User userObj)
        {
            userObj.Id = 0;
            return Ok(await userService.AddAndUpdateUser(userObj));
        }

        /// <summary>
        /// Update an user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userObj"></param>
        /// <returns></returns>
        /// <response code="200">User udpated or stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpPut("{id}")]
        [CAuthorize]
        public async Task<IActionResult> Put(int id, [FromBody] User userObj)
        {
            return Ok(await userService.AddAndUpdateUser(userObj));
        }
        
    }
}