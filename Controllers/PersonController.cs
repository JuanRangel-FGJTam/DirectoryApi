using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Models;
using AuthApi.Helper;

namespace AuthApi.Controllers
{
    [ApiController]
    [CAuthorize]
    [Route("api/[controller]")]
    public class PersonController(ILogger<CatalogController> logger, DirectoryDBContext context) : ControllerBase
    {
        private readonly ILogger<CatalogController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;
        
        [HttpPost]
        public IActionResult RegisterUser( PersonRequest personRequest )
        {

            // * Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest( ModelState );
            }

            // * Get relations and validate
            var errorsRelations = new Dictionary<string, object>();

            var gender = dbContext.Gender.Find( personRequest.GenderID );
            if( gender == null){
                errorsRelations.Add( "GenderID", new string[]{ $"Gender id {personRequest.GenderID} not found "} );
            }
            
            var maritalStatus = dbContext.MaritalStatus.Find( personRequest.MaritalStatusID );
            if( maritalStatus == null){
                errorsRelations.Add( "MaritalStatusID", new string[]{ $"Marital status id {personRequest.MaritalStatusID} not found "} );
            }
            
            var nationality = dbContext.Nationality.Find( personRequest.NationalityID );
            if( nationality == null){
                errorsRelations.Add( "NationalityID", new string[]{ $"Nationality id {personRequest.NationalityID} not found "} );
            }
            
            var occupation = dbContext.Occupation.Find( personRequest.OccupationID ); 
            if( occupation == null){
                errorsRelations.Add( "OccupationID", new string[]{ $"Occupation id {personRequest.OccupationID} not found "} );
            }

            if( errorsRelations.Values.Count > 0)
            {
                return BadRequest(new {
                    Title = "One or more relations are not found",
                    Errors = errorsRelations
                });
            }


            // * Convert PersonRequest into Person
            var _person = new Person(){
                Rfc = personRequest.Rfc,
                Curp = personRequest.Curp,
                Name = personRequest.Name,
                FirstName = personRequest.FirstName,
                LastName = personRequest.LastName,
                Email = personRequest.Email,
                BirthDate = personRequest.BirthDate,
                Gender = gender,
                MaritalStatus = maritalStatus,
                Nationality = nationality,
                Occupation = occupation
            };
            
            // * Insert into db 
            dbContext.People.Add( _person );
            dbContext.SaveChanges();

            // * Return response
            return Created("Person created", _person );
        }

        [HttpGet]
        [Route ("{person_id}")]
        public ActionResult<Person> GetPerson( string person_id )
        {
            var person = dbContext.People
                .Include(p => p.Addresses)
                .Include( p => p.ContactInformations)
                .FirstOrDefault(p => p.Id == Guid.Parse(person_id) );

            if( person == null)
            {
                return NotFound();
            }
            
            return Ok( person );
        }

        [HttpPost]
        [Route("Address")]
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
  
        [HttpPost]
        [Route("Contact")]
        public IActionResult StoreContact( ContactRequest contactRequest )
        {

            // Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get relations and validate them
            var errorsRelations = new Dictionary<string, string>();

            var person = dbContext.People.Find(Guid.Parse( contactRequest.PersonID) );
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

    }
}