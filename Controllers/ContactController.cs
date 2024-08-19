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
using Microsoft.AspNetCore.Authorization;

namespace AuthApi.Controllers
{
    /// <summary></summary>
    [Authorize]
    [ApiController]
    [Route("api/contact")]
    public class ContactController(ILogger<ContactController> logger, DirectoryDBContext context) : ControllerBase
    {
        private readonly ILogger<ContactController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;

        /// <summary>
        /// Get the contact information
        /// </summary>
        /// <param name="contactID"></param>
        /// <returns></returns>
        /// <response code="200">Return the contact infor</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpGet]
        [Route("{contactID}")]
        public ActionResult<ContactInformation> GetContact( [FromRoute] Guid contactID )
        {
            
            ContactInformation? contact = this.dbContext.ContactInformations
                .Include( a => a.ContactType )
                .Where( a => a.Id == contactID && a.DeletedAt == null )
                .FirstOrDefault();

            if( contact == null)
            {
                return BadRequest( new {
                    message = $"Contact id {contactID} not found"
                });
            }

            return Ok( contact );
        }


        /// <summary>
        /// Store a new contact info
        /// </summary>
        /// <param name="contactRequest"></param>
        /// <returns></returns>
        /// <response code="201">The contact is stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="422">The contact information is already stored</response>
        [HttpPost]
        [Route("")]
        public IActionResult StoreContact( ContactRequest contactRequest )
        {

            
            // Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Validate ID
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( contactRequest.PersonID );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id format not valid"
                });
            }


            // Get relations and validate them
            var errorsRelations = new Dictionary<string, string>();

            var person = dbContext.People.Find( _personID );
            if (person == null)
            {
                errorsRelations.Add("PersonID", $"Person id {contactRequest.PersonID} is not found");
            }

            var contactType = dbContext.ContactTypes.Find(contactRequest.ContactTypeID);
            if (contactType == null)
            {
                errorsRelations.Add("ContactTypeID", $"Contact type id {contactRequest.ContactTypeID} not found");
            }

            if (errorsRelations.Count > 0)
            {
                return BadRequest(new
                {
                    Title = "One or more relations are not found",
                    Errors = errorsRelations
                });
            }

            // Validate and prevent if the value is already stored
            var _contactIsStored = dbContext.ContactInformations
             .Where( item => item.DeletedAt == null)
             .Where( item => item.Value == contactRequest.Value).Any();
            if(_contactIsStored) {  
                return StatusCode( StatusCodes.Status422UnprocessableEntity, new {
                    Title = "El telefono o correo ya se encuentra registrado"
                });
            }

            // Create model
            var contactInformation = new ContactInformation
            {
                Person = person!,
                ContactType = contactType!,
                Value = contactRequest.Value
            };

            // Insert into db 
            dbContext.ContactInformations.Add( contactInformation );
            dbContext.SaveChanges();

            // Return response
            return Created("Contact stored", contactInformation);
        }

        /// <summary>
        /// Delete the contact information
        /// </summary>
        /// <param name="contactID"></param>
        /// <returns></returns>
        /// <response code="200">The contact information is deleted</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpDelete]
        [Route("{contactID}")]
        public IActionResult DeleteContact( [FromRoute] Guid contactID )
        {

            ContactInformation? currentContact = dbContext.ContactInformations.Where( c => c.Id == contactID &&  c.DeletedAt == null).FirstOrDefault();
            if( currentContact == null)
            {
                return BadRequest( new {
                    message = $"Contact id {contactID} not found"
                });
            }
            currentContact.DeletedAt = DateTime.Now;
            dbContext.ContactInformations.Update( currentContact );
            dbContext.SaveChanges();

            return Ok( new {
                message = $"Contact id {contactID} removed successfully"
            });

        }

        /// <summary>
        /// Update the contact information
        /// </summary>
        /// <param name="contactID"></param>
        /// <param name="contactRequest"></param>
        /// <returns></returns>
        /// <response code="200">The contact information is deleted</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpPatch]
        [Route("{contactID}")]
        public IActionResult UpdateContact( [FromRoute] Guid contactID, [FromBody] ContactRequest contactRequest )
        {
            if( !ModelState.IsValid) {
                return BadRequest( ModelState );
            }


            // * Validate contact id
            ContactInformation? currentContact = dbContext.ContactInformations.Find( contactID );
            if( currentContact == null){
                return BadRequest(new {
                    message = $"Contact id {contactID} not found"
                });
            }


            // * Validate relations
            ContactType? contactType = dbContext.ContactTypes.Find( contactRequest.ContactTypeID );
            if( contactType == null){
                return BadRequest(new {
                    Title = "One or more relations are not found",
                    Errors = new string[]{ $"ContactType id {contactRequest.ContactTypeID} not found"}
                });
            }

            
            // * Update address model
            currentContact.ContactType = contactType;
            currentContact.Value = contactRequest.Value!;

            // * Save changes
            this.dbContext.ContactInformations.Update( currentContact );
            this.dbContext.SaveChanges();
            return Ok( new {
                message = $"Contact id {contactID} updated"
            });

        }


    }
}