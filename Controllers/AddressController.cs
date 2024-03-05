using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [CAuthorize]
    public class AddressController : ControllerBase
    {
        private readonly ILogger<AddressController> _logger;
        private readonly DirectoryDBContext dbContext;

        public AddressController(ILogger<AddressController> logger, DirectoryDBContext context)
        {
            this._logger = logger;
            this.dbContext = context;
        }

        [HttpGet]
        [Route("{addressID}")]
        public ActionResult<Address> GetAddress( [FromRoute] string addressID )
        {
            // Validate ID
            Guid _addressID = Guid.Empty;
            try{
                _addressID = Guid.Parse( addressID );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Address id format not valid"
                });
            }

            Address? address =  this.dbContext.Addresses
                .Include( a => a.Colony)
                .Include( a => a.Municipality)
                .Include( a => a.State)
                .Include( a => a.Country)
                .Where( a => a.ID == _addressID )
                .FirstOrDefault();

            if( address == null)
            {
                return BadRequest( new {
                    message = $"Address id {addressID} not found"
                });
            }

            return Ok( address );
        }
    }
}