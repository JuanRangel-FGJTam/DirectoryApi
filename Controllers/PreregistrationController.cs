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

namespace AuthApi.Controllers
{

    /// <summary></summary>
    [Authorize]
    [ApiController]
    [Route("api/pre-registration")]
    public class PreregistrationController(DirectoryDBContext context) : ControllerBase
    {
        private readonly DirectoryDBContext dbContext = context;


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

            var _newRecord = new Preregistration(){
                Mail = request.Mail,
                Password = request.Password
            };

            // Insert into db 
            dbContext.Preregistrations.Add( _newRecord );
            dbContext.SaveChanges();

            // Return response
            return Created("", request );
        }

    }
}