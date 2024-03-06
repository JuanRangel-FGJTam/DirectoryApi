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
                .Include(p => p.Addresses.Where( a => a.DeletedAt == null))
                .Include( p => p.ContactInformations.Where( a => a.DeletedAt == null))
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

    }
}