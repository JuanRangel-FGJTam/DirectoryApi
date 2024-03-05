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
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService userService;

        public UserController(ILogger<UserController> logger, IUserService service )
        {
            _logger = logger;
            userService = service;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var response = await userService.Authenticate( model );

            if (response == null){
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            return Ok(response);
        }

        [HttpPost]
        [CAuthorize]
        public async Task<IActionResult> Post([FromBody] User userObj)
        {
            userObj.Id = 0;
            return Ok(await userService.AddAndUpdateUser(userObj));
        }

        [HttpPut("{id}")]
        [CAuthorize]
        public async Task<IActionResult> Put(int id, [FromBody] User userObj)
        {
            return Ok(await userService.AddAndUpdateUser(userObj));
        }
        
    }
}