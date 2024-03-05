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
    public class PeopleController(ILogger<PeopleController> logger, DirectoryDBContext context) : ControllerBase
    {
        private readonly ILogger<PeopleController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;
        
        [HttpGet]
        public ActionResult<Person> GetAllPeople( [FromQuery] int chunk = 100, [FromQuery] int skip = 0 )
        {

            // * Get data
            var data = this.dbContext.People
                .Include(p => p.Addresses)
                .Include(p => p.ContactInformations)
                .Include(p => p.Gender)
                .Include(p => p.MaritalStatus)
                .Include(p => p.Nationality)
                .Include(p => p.Occupation)
                .OrderBy(p => p.CreatedAt)
                .Skip(skip)
                .Take(chunk)
                .ToArray();

            // * Return response
            return Ok( data );
        }

        [HttpPost]
        public IActionResult StorePerson( PersonRequest personRequest )
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
                BirthDate = personRequest.BirthDate!.Value,
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
            // Validate ID
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( person_id );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id format not valid"
                });
            }

            var person = dbContext.People
                .Include(p => p.Addresses)
                .Include( p => p.ContactInformations)
                .Include(p => p.Gender)
                .Include(p => p.MaritalStatus)
                .Include(p => p.Nationality)
                .Include(p => p.Occupation)
                .FirstOrDefault(p => p.Id == _personID );

            if( person == null)
            {
                return NotFound();
            }
            
            return Ok( person );
        }

        [HttpPatch]
        [Route ("{personID}")]
        public IActionResult UpdatePerson( string personID, [FromBody] PersonRequest personRequest  )
        {
            // Validate ID
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( personID );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id format not valid"
                });
            }

            // validate person id
            Person? person = this.dbContext.People.Find( _personID );
            if( person == null){
                return BadRequest(new {
                    message = $"Person id {personID} not found"
                });
            }
            
            // Update information
            if(personRequest.Name != null )
            {
                person.Name = personRequest.Name;
            }

            if(personRequest.FirstName != null )
            {
                person.FirstName = personRequest.FirstName;
            }

            if(personRequest.LastName != null )
            {
                person.LastName = personRequest.LastName;
            }

            if(personRequest.Rfc != null )
            {
                person.Rfc = personRequest.Rfc;
            }

            if(personRequest.Curp != null )
            {
                person.Curp = personRequest.Curp;
            }

            if(personRequest.Curp != null )
            {
                person.Curp = personRequest.Curp;
            }

            if(personRequest.BirthDate != null )
            {
                person.BirthDate = personRequest.BirthDate.Value;
            }

            if(personRequest.GenderID > 0)
            {
                var _gender = this.dbContext.Gender.Find( personRequest.GenderID );
                person.Gender = _gender;
            }

            if(personRequest.MaritalStatusID > 0 )
            {
                var _maritalStatus = this.dbContext.MaritalStatus.Find( personRequest.MaritalStatusID );
                person.MaritalStatus = _maritalStatus;
            }
            if(personRequest.NationalityID > 0 )
            {
                var _nationality = this.dbContext.Nationality.Find( personRequest.NationalityID );
                person.Nationality = _nationality;
            }

            if(personRequest.OccupationID > 0 )
            {
                var _occupation = this.dbContext.Occupation.Find( personRequest.OccupationID );
                person.Occupation = _occupation;
            }


            // Aply changes
            this.dbContext.Update( person );
            this.dbContext.SaveChanges();

            return Ok( new {
                message = $"Person {personID} updated"
            });
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

    }
}