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
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Controllers
{
    
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
        ///  Store a new person in the database
        /// </summary>
        /// <remarks>
        /// Sample request with the minal data required:
        /// 
        ///     POST api/people
        ///     {
        ///       "curp": "RAAE190394MTSNLL02",
        ///       "name": "Juan Salvador",
        ///       "firstName": "Rangel",
        ///       "lastName": "Almaguer",
        ///       "email": "juan.rangel@fgjtam.gob.mx",
        ///       "birthdate": "1993-12-17"
        ///     }
        ///     
        /// 
        /// Sample request with all the data available:
        /// 
        ///     POST api/people
        ///     {
        ///        "rfc": "RAAJ931217SX4",
        ///        "curp": "RAAJ931217MTSNLL02",
        ///        "name": "Juan Salvador",
        ///        "firstName": "Rangel",
        ///        "lastName": "Almaguer",
        ///        "email": "juan.rangel@fgjtam.gob.mx",
        ///        "birthdate": "1993-12-17",
        ///        "GenderID": 1,
        ///        "OccupationID": 1,
        ///        "NationalityID": 31,
        ///        "MaritalStatusID": 1,
        ///        "appName": "appname.fgjtam.gob.mx",
        ///        "password": "password123$",
        ///        "confirmPassword": "password123$"
        ///     }
        /// </remarks>
        /// <param name="personRequest"></param>
        /// <response code="201">Succsessfull stored the person</response>
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

            try
            {
                var person = this.personService.StorePerson(personRequest);
                return Created("Person created", person );
                
            }
            catch (ValidationException ve)
            {
                var errorsData = (Dictionary<string, object>) ve.Value!;
                return UnprocessableEntity(new {
                    Title = "One or more field had errors",
                    Errors = errorsData
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new {
                    Title = "Unhandle exception",
                    Message = ex.Message,
                    Errors = Array.Empty<string>()
                });
            }
            
        }

        

        /// <summary>
        /// Udate the person data
        /// </summary>
        /// <remarks>
        /// You can pass only the fields you want to update
        /// </remarks>
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

            if(personRequest.Birthdate != null )
            {
                person.Birthdate = personRequest.Birthdate.Value;
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


        /// <summary>
        /// Seach the person by the email
        /// </summary>
        /// <remarks>
        /// Seek in the database if the email is already stored in the database and if exist return de data of the person.
        /// 
        /// Data returned:
        /// 
        ///     {
        ///       "Id": int,
        ///       "FullName": string,
        ///       "Birthdate": string (yyy-MM-dd),
        ///       "Email": string,
        ///       "Gender": string,
        ///       "Curp": string
        ///     }
        /// </remarks>
        /// <param name="email"></param>
        /// <param name="hiddenName"></param>
        /// <response code="200">Return the person related with the email</response>
        /// <response code="404">There is no person with the email searched</response>
        [HttpGet]
        [Route("search")]
        public IActionResult SearchPerson( [FromQuery] string email, [FromQuery] int hiddenName ){
            var people =  this.personService.Search( email, null,null, PersonService.SearchMode.Equals );

            var person = people.FirstOrDefault();
            if( person == null){
                return NotFound();
            }

            return Ok( new {
                Id = person.Id,
                FullName = hiddenName == 1 ?HiddenText.Hidden(person.FullName) :person.FullName,
                Birthdate = person.Birthdate.ToString("yyyy-MM-dd"),
                Email = person.Email,
                Gender = person.Gender != null ?person.Gender.Name:"",
                Curp = person.Curp
            });
        }


        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
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
        
        /// <summary>
        /// Validate the person email and password
        /// </summary>
        /// <remarks>
        /// 
        /// Sample of succsess returned data:
        /// 
        ///     Httpcode 200
        ///     {
        ///       "Id": string,
        ///       "FullName": string,
        ///       "Email": string
        ///     }
        ///     
        /// 
        /// Sample of fail the request:
        /// 
        ///     Httpcode 401|404:
        ///     {
        ///       "Message": string
        ///     }
        ///     
        /// </remarks>
        /// <param name="authenticateRequest"></param>
        /// <returns></returns>
        /// <response code="200">Return the person data of the email </response>
        /// <response code="400">The email or password are not present in the request</response>
        /// <response code="401">The email or the password are incorrect</response>
        /// <response code="404">The email was not found on the database</response>
        [HttpPost]
        [Route("auth")]
        public ActionResult AuthPerson([FromBody] AuthenticateRequest authenticateRequest){

            if( !ModelState.IsValid){
                return BadRequest(ModelState);
            }

            var _persons = this.personService.Search( authenticateRequest.Email!, null, null );
            if( _persons.IsNullOrEmpty()){
                return NotFound(new {
                    Message = "El correo no se encuentra registrado en la base de datos"
                });
            }

            var person = this.personService.AuthPerson( authenticateRequest.Email!, authenticateRequest.Password!);
            if( person == null){
                return Unauthorized(new {
                    Message = "Usuario y/o contraseña incorrectos"
                });
            }

            return Ok(new{
                Id = person.Id,
                Name = person.FullName,
                Email = person.Email
            });

        }

    }
}