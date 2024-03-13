using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Helper;
using AuthApi.Models;

namespace AuthApi.Controllers
{
    /// <summary></summary>
    [ApiController]
    [Route("api/address")]
    [CAuthorize]
    public class AddressController(ILogger<AddressController> logger, DirectoryDBContext context) : ControllerBase
    {
        private readonly ILogger<AddressController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;


        /// <summary>
        /// Get the address info
        /// </summary>
        /// <param name="addressID"></param>
        /// <returns></returns>
        /// <response code="200">Get the address</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
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
                .Where( a => a.DeletedAt == null)
                .Include( a => a.Colony)
                .Include( a => a.Municipality)
                .Include( a => a.State)
                .Include( a => a.Country)
                .Where( a => a.Id == _addressID )
                .FirstOrDefault();

            if( address == null)
            {
                return BadRequest( new {
                    message = $"Address id {addressID} not found"
                });
            }

            return Ok( address );
        }

        /// <summary>
        /// Store the address
        /// </summary>
        /// <param name="addressRequest"></param>
        /// <returns></returns>
        /// <response code="201">The Address is stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpPost]
        public IActionResult StoreAddress( [FromBody] AddressRequest addressRequest )
        {

            // * Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest( ModelState );
            }

            // * Get relations and validate them
            var errorsRelations = new Dictionary<string, object>();
            
            var person = dbContext.People.Find( addressRequest.PersonID );
            if( person == null){
                errorsRelations.Add( "PersonID", new string[]{ $"Person id {addressRequest.PersonID} not found "} );
            }

            var country = dbContext.Countries.Find( addressRequest.CountryID ); 
            if( country == null){
                errorsRelations.Add( "CountryID", new string[]{ $"Occupation id {addressRequest.CountryID} not found "} );
            }

            var state = dbContext.States.Find( addressRequest.StateID ); 
            if( state == null){
                errorsRelations.Add( "StateID", new string[]{ $"Occupation id {addressRequest.StateID} not found "} );
            }

            var municipality = dbContext.Municipalities.Find( addressRequest.MunicipalityID ); 
            if( municipality == null){
                errorsRelations.Add( "MunicipalityID", new string[]{ $"Occupation id {addressRequest.MunicipalityID} not found "} );
            }

            // Colony Id is optional
            Colony? colony = null;
            if( addressRequest.ColonyID != null)
            {
                colony = dbContext.Colonies.Find( addressRequest.ColonyID ); 
                if( colony == null){
                    errorsRelations.Add( "ColonyID", new string[]{ $"Occupation id {addressRequest.ColonyID} not found "} );
                }
            }

            if( errorsRelations.Values.Count > 0)
            {
                return BadRequest(new
                {
                    Title = "One or more relations are not found",
                    Errors = errorsRelations
                });
            }

            // * Create address model
            var _address = new Address(){
                Person = person!,
                Country = country!,
                State = state!,
                Municipality = municipality!,
                Colony = colony,
                Street = addressRequest.Street??"",
                Number = addressRequest.Number,
                NumberInside = addressRequest.NumberInside,
                ZipCode = addressRequest.ZipCode
            };
            
            // * Insert into db 
            dbContext.Addresses.Add( _address);
            dbContext.SaveChanges();

            // * Return response
            return Created("Address stored", _address );
        }
        

        /// <summary>
        /// Delete the address
        /// </summary>
        /// <param name="addressID"></param>
        /// <returns></returns>
        /// <response code="200">The Address is deleted</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpDelete]
        [Route("{addressID}")]
        public IActionResult HttpDeleteAddress( [FromRoute] Guid addressID )
        {

            Address? address = dbContext.Addresses.Where( a => a.DeletedAt == null && a.Id == addressID).FirstOrDefault();
            if( address == null)
            {
                return BadRequest( new {
                    message = $"Addres id {addressID} not found"
                });
            }
            address.DeletedAt = DateTime.Now;
            
            dbContext.Addresses.Update(address);
            dbContext.SaveChanges();

            return Ok( new {
                message = $"Addres id {addressID} removed successfully"
            });

        }


        /// <summary>
        /// Udpate the address
        /// </summary>
        /// <param name="addressID"></param>
        /// <param name="addressRequest"></param>
        /// <returns></returns>
        /// <response code="200">The Address is updated</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpPatch]
        [Route("{addressID}")]
        public IActionResult UpdateAddress( [FromRoute] Guid addressID, [FromBody] AddressRequest addressRequest )
        {
            if( !ModelState.IsValid) {
                return BadRequest( ModelState );
            }


            // * Validate addres id
            Address? currentAddress = dbContext.Addresses.Where( a => a.Id == addressID && a.DeletedAt == null ).FirstOrDefault();
            if( currentAddress == null){
                return BadRequest(new {
                    message = $"Addres id {addressID} not found"
                });
            }


            // * Validate relations
            var errors = new Dictionary<string, object>();
            Country? country = dbContext.Countries.Find( addressRequest.CountryID );
            if( country == null){
                errors.Add("CountryID", new string[]{ $"Country id {addressRequest.CountryID} not found "} );
            }
            State? state = dbContext.States.Find( addressRequest.StateID );
            if( state == null){
                errors.Add("StateID", new string[]{ $"State id {addressRequest.StateID} not found "} );
            }
            Municipality? municipality = dbContext.Municipalities.Find( addressRequest.MunicipalityID );
            if( municipality == null){
                errors.Add("MunicipalityID", new string[]{ $"Municipality id {addressRequest.MunicipalityID} not found "} );
            }
            Colony? colony = dbContext.Colonies.Find( addressRequest.ColonyID );
            if( colony == null){
                errors.Add("ColonyID", new string[]{ $"Colony id {addressRequest.ColonyID} not found "} );
            }
            if( errors.Count > 0){
                return BadRequest(new {
                    Title = "One or more relations are not found",
                    Errors = errors
                });
            }

            
            // * Update address model
            currentAddress.Country = country!;
            currentAddress.State = state!;
            currentAddress.Municipality = municipality!;
            currentAddress.Colony = colony!;
            currentAddress.Street = addressRequest.Street!;
            currentAddress.Number = addressRequest.Number!;
            if( addressRequest.NumberInside != null){
                currentAddress.NumberInside = addressRequest.NumberInside;
            }
            currentAddress.ZipCode = addressRequest.ZipCode;

            // * Save changes
            this.dbContext.Addresses.Update( currentAddress );
            this.dbContext.SaveChanges();
            return Ok( new {
                message = $"Address id {addressID} updated"
            });

        }


    }
}