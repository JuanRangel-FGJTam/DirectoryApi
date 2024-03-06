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
    [ApiController]
    [Route("api/[controller]")]
    [CAuthorize]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;
        private readonly DirectoryDBContext dbContext;

        public ContactController(ILogger<ContactController> logger, DirectoryDBContext context)
        {
            this._logger = logger;
            this.dbContext = context;
        }


        [HttpGet]
        [Route("{contactID}")]
        public ActionResult<Address> GetContact( [FromRoute] Guid contactID )
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