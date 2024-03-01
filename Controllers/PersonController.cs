using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Entities;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController(AuthDbContext context) : ControllerBase
    {
        private readonly AuthDbContext dbContext = context;
        
        [HttpPost]
        public IActionResult RegisterUser( Person request )
        {

            // Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
       
            // Insert into db 
            dbContext.People.Add( request );
            dbContext.SaveChanges();

            // Return response
            return Created("", request );
        }

    }
}