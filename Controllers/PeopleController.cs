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
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using AuthApi.Services;
using System.Runtime.Serialization;

namespace AuthApi.Controllers
{
    
    /// <summary></summary>
    [Authorize]
    [ApiController]
    [Route("api/people")]
    public class PeopleController(ILogger<PeopleController> logger, DirectoryDBContext context, PersonService personService) : ControllerBase
    {
        private readonly ILogger<PeopleController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;
        private readonly PersonService personService = personService;
        
        
        /// <summary>
        /// Return all people stored
        /// </summary>
        /// <remarks> This endpoint returns all people stored.</remarks>
        /// <response code="200">Returns the person created</response>
        [HttpGet]
        public ActionResult<Person> GetAllPeople( [FromQuery] int chunk = 100, [FromQuery] int skip = 0 )
        {
            var peopleQuery = this.personService.GetPeople();

            var dataPeople = peopleQuery.OrderBy(p => p.CreatedAt)
                .Skip(skip)
                .Take(chunk)
                .ToArray();

            // * Return response
            return Ok( dataPeople );
        }


        /// <summary>
        ///  Store a person in the database
        /// </summary>
        /// <param name="personRequest"></param>
        /// <response code="201">Stored the person</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
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

            var gender = dbContext.Gender.Find( personRequest.GenderId );
            if( gender == null){
                errorsRelations.Add( "GenderID", new string[]{ $"Gender id {personRequest.GenderId} not found "} );
            }
            
            var maritalStatus = dbContext.MaritalStatus.Find( personRequest.MaritalStatusId );
            if( maritalStatus == null){
                errorsRelations.Add( "MaritalStatusID", new string[]{ $"Marital status id {personRequest.MaritalStatusId} not found "} );
            }
            
            var nationality = dbContext.Nationality.Find( personRequest.NationalityId );
            if( nationality == null){
                errorsRelations.Add( "NationalityID", new string[]{ $"Nationality id {personRequest.NationalityId} not found "} );
            }
            
            var occupation = dbContext.Occupation.Find( personRequest.OccupationId ); 
            if( occupation == null){
                errorsRelations.Add( "OccupationID", new string[]{ $"Occupation id {personRequest.OccupationId} not found "} );
            }

            var rfcStored =  dbContext.People.Where( p => p.DeletedAt == null && p.Rfc == personRequest.Rfc ).Count();
            if(rfcStored > 0){
                errorsRelations.Add( "rfc", new string[]{ $"The RFC is already stored"} );
            }

            var curpStored =  dbContext.People.Where( p => p.DeletedAt == null && p.Curp == personRequest.Curp ).Count();
            if(curpStored > 0){
                errorsRelations.Add( "curp", new string[]{ $"The CURP is already stored"} );
            }

            var emailStored =  dbContext.People.Where( p => p.DeletedAt == null && p.Email == personRequest.Email ).Count();
            if(emailStored > 0){
                errorsRelations.Add( "email", new string[]{ $"The Email is already stored"} );
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
                Rfc = personRequest.Rfc!,
                Curp = personRequest.Curp!,
                Name = personRequest.Name,
                FirstName = personRequest.FirstName,
                LastName = personRequest.LastName,
                Email = personRequest.Email,
                Birthdate = personRequest.BirthDate!.Value,
                Gender = gender,
                MaritalStatus = maritalStatus,
                Nationality = nationality,
                Occupation = occupation,
                AppOwner = personRequest.AppName
            };
            
            // * Insert into db 
            dbContext.People.Add( _person );
            dbContext.SaveChanges();

            // * Return response
            return Created("Person created", _person );
        }

        

        /// <summary>
        /// Udate the person data
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="personRequest"></param>
        /// <returns></returns>
        /// <response code="201">The person is updated</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
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

            // * Get relations and validate
            var errorsRelations = new Dictionary<string, object>();


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
                var rfcStored =  dbContext.People.Where( p => p.DeletedAt == null && p.Rfc == personRequest.Rfc && p.Id != _personID ).Count();
                if(rfcStored > 0){
                    errorsRelations.Add( "rfc", new string[]{ $"The RFC is already stored"} );
                }
                person.Rfc = personRequest.Rfc;
            }

            if(personRequest.Curp != null )
            {
                var curpStored =  dbContext.People.Where( p => p.DeletedAt == null && p.Curp == personRequest.Curp && p.Id != _personID ).Count();
                if(curpStored > 0){
                    errorsRelations.Add( "curp", new string[]{ $"The CURP is already stored"} );
                }
                person.Curp = personRequest.Curp;
            }

            if( errorsRelations.Values.Count > 0)
            {
                return BadRequest(new {
                    Title = "One or more relations are not found",
                    Errors = errorsRelations
                });
            }

            if(personRequest.BirthDate != null )
            {
                person.Birthdate = personRequest.BirthDate.Value;
            }

            if(personRequest.GenderId > 0)
            {
                var _gender = this.dbContext.Gender.Find( personRequest.GenderId );
                person.Gender = _gender;
            }

            if(personRequest.MaritalStatusId > 0 )
            {
                var _maritalStatus = this.dbContext.MaritalStatus.Find( personRequest.MaritalStatusId );
                person.MaritalStatus = _maritalStatus;
            }
            if(personRequest.NationalityId > 0 )
            {
                var _nationality = this.dbContext.Nationality.Find( personRequest.NationalityId );
                person.Nationality = _nationality;
            }

            if(personRequest.OccupationId > 0 )
            {
                var _occupation = this.dbContext.Occupation.Find( personRequest.OccupationId );
                person.Occupation = _occupation;
            }


            // Aply changes
            this.dbContext.Update( person );
            this.dbContext.SaveChanges();

            return Ok( new {
                message = $"Person {personID} updated"
            });
        }


        [HttpGet]
        [Route("search")]
        public IActionResult SearchPerson( [FromQuery] string email, [FromQuery] int hiddenName ){
            var people =  this.personService.Search( email, null,null, PersonService.SearchMode.Equals );
            return Ok( people.Select( p => new {
                Id = p.Id,
                FullName = hiddenName == 1 ?HiddenText.Hidden(p.FullName) :p.FullName,
                Birthdate = p.Birthdate.ToString("yyyy-MM-dd"),
                Email = p.Email,
                Gender = p.Gender != null ?p.Gender.Name:"",
                Curp = p.Curp
            }));
        }

        [HttpGet]
        [Route("me")]
        public IActionResult GetCurrentDataUser(){
            foreach( var claim in HttpContext.User.Claims ){
                Console.WriteLine( $"(-) >> {claim.Type}: {claim.Value}" );
            }
            return Ok("Not implemented");
        }


        /// <summary>
        ///  Retrive the data of the person 
        /// </summary>
        /// <param name="personID"></param>
        /// <response code="200">Return the person data</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpGet]
        [Route ("{personID}")]
        public ActionResult<Person> GetPerson( string personID )
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

            var person = this.personService.GetPeople()
                .Include(p => p.Addresses.Where( a=> a != null && a.DeletedAt == null ) )
                .Include(p => p.ContactInformations.Where( a => a != null && a.DeletedAt == null))
                .FirstOrDefault(p => p.Id == _personID);

            if ( person == null)
            {
                return NotFound();
            }
            
            return Ok( person );
        }
        
    }
}