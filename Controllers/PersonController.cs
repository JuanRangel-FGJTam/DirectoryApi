using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Entities;
using AuthApi.Models;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController(ILogger<CatalogController> logger, AuthDbContext context) : ControllerBase
    {
        private readonly ILogger<CatalogController> _logger = logger;
        private readonly AuthDbContext dbContext = context;
        
        [HttpPost]
        public IActionResult RegisterUser( PersonRequest personRequest )
        {

            // * Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // * Get relations and validate
            var _errorsRelations = new Dictionary<string, object>();
            Gender? _gender = dbContext.Gender.Find( personRequest.GenderID );
            if( _gender == null){
                _errorsRelations.Add( "GenderID", new string[]{ $"Gender id {personRequest.GenderID} not found "} );
            }
            MaritalStatus? _maritalStatus = dbContext.MaritalStatus.Find( personRequest.MaritalStatusID );
            if( _maritalStatus == null){
                _errorsRelations.Add( "MaritalStatusID", new string[]{ $"Marital status id {personRequest.MaritalStatusID} not found "} );
            }
            Nationality? _nationality = dbContext.Nationality.Find( personRequest.NationalityID );
            if( _nationality == null){
                _errorsRelations.Add( "NationalityID", new string[]{ $"Nationality id {personRequest.NationalityID} not found "} );
            }
            Occupation? _occupation = dbContext.Occupation.Find( personRequest.OccupationID ); 
            if( _maritalStatus == null){
                _errorsRelations.Add( "OccupationID", new string[]{ $"Occupation id {personRequest.OccupationID} not found "} );
            }

            if( _errorsRelations.Values.Count > 0)
            {

                return BadRequest(new {
                    Title = "One o more relations are not found",
                    Errors = _errorsRelations
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
                Gender = _gender,
                MaritalStatus = _maritalStatus,
                Nationality = _nationality,
                Occupation = _occupation
            };
            
            // * Insert into db 
            dbContext.People.Add( _person );
            dbContext.SaveChanges();

            // * Return response
            return Created("Person created", _person );
        }

    }
}