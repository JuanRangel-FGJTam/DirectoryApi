#pragma warning disable CS9124
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Services;
using AuthApi.Models.Responses;
using CommunityToolkit.HighPerformance.Helpers;
using System.Security.AccessControl;

namespace AuthApi.Controllers
{
    
    [Authorize]
    [ApiController]
    [Route("api/people")]
    public class PeopleController(ILogger<PeopleController> logger, DirectoryDBContext context, PersonService personService, ResetPasswordState resetPasswordState, ChangeEmailState changeEmailState, ICryptographyService cryptographyService, EmailNotificationsService emailNotificationsService, PersonBanService personBanService) : ControllerBase
    {
        private readonly ILogger<PeopleController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;
        private readonly PersonService personService = personService;
        private readonly ResetPasswordState resetPasswordState = resetPasswordState;
        private readonly ChangeEmailState changeEmailState = changeEmailState;
        private readonly ICryptographyService cryptographyService = cryptographyService;
        private readonly EmailNotificationsService emailNotificationsService = emailNotificationsService;
        private readonly PersonBanService personBanService = personBanService;


        /// <summary>
        /// Return the people stored, by default returns 5 ordered by `createdAt`
        /// </summary>
        /// <param name="orderBy"> propertie name used for ordering by default 'createdAt' posibles ["id", "name", "firstName", "curp", "email", "createdAt"] </param>
        /// <param name="ascending"></param>
        /// <param name="take"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <response code="200">Returns the person</response>
        [HttpGet]
        public ActionResult<IEnumerable<PersonResponse>?> GetAllPeople( [FromQuery] string orderBy = "createdAt", [FromQuery] bool ascending = false, [FromQuery] int take = 5, [FromQuery] int offset = 0 )
        {
            var peopleQuery = this.personService.GetPeople();

            // * ordering the data
            string ordering = ascending ? $"{orderBy} asc" : $"{orderBy} desc";
            var dataPeople = peopleQuery
                .OrderBy(ordering)
                .Skip(offset)
                .Take(take)
                .ToArray();

            // * Return response
            return Ok( dataPeople.Select( p => PersonResponse.FromEntity(p)).ToArray<PersonResponse>() );
        }


        /// <summary>
        ///  Store a new person in the database
        /// </summary>
        /// <remarks>
        /// Sample request with the minal data required:
        /// 
        ///     POST api/people
        ///     {
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
        /// <response code="409">Fail to to update the user password</response>
        /// <response code="422">The validation fails</response>
        [HttpPatch]
        [Route ("{personID}")]
        public async Task<IActionResult> UpdatePerson(string personID, [FromBody] UpdatePersonRequest personRequest)
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

            // * if attempt to update the email check if is allowed
            string _oldEmail = string.Empty;
            if (person.Email != personRequest.Email && !string.IsNullOrEmpty(personRequest.Email))
            {
                if (CheckIfHasActiveProcessing(person!))
                {
                    return UnprocessableEntity(new
                    {
                        Title = "No se puede cambiar el correo",
                        Errors = new
                        {
                            email = new string[] { "No es posible actualizar el correo electrónico del usuario debido a que actualmente tiene un procedimiento activo relacionado con la constancia de antecedentes penales." }
                        }
                    });
                }
            }
            
            // Update information
            if(!string.IsNullOrEmpty(personRequest.Name))
            {
                person.Name = personRequest.Name.Trim();
            }

            if(!string.IsNullOrEmpty(personRequest.FirstName))
            {
                person.FirstName = personRequest.FirstName.Trim();
            }

            if(personRequest.LastName != null)
            {
                if(!string.IsNullOrEmpty(personRequest.LastName))
                {
                    person.LastName = personRequest.LastName.Trim();
                }
                else
                {
                    person.LastName = String.Empty;
                }
            }

            if(personRequest.Rfc != null)
            {
                if(!string.IsNullOrEmpty(personRequest.Rfc))
                {
                    var rfcStored = dbContext.People.Where( p => p.DeletedAt == null && p.Rfc == personRequest.Rfc.Trim() && p.Id != _personID ).Count();
                    if(rfcStored > 0){
                        errorsRelations.Add( "rfc", new string[]{ "El RFC ingresado ya está registrado en nuestro sistema."} );
                    }
                    person.Rfc = personRequest.Rfc.Trim();
                }
                else
                {
                    person.Rfc = String.Empty;
                }
            }

            if(personRequest.Curp != null)
            {
                if(!string.IsNullOrEmpty(personRequest.Curp))
                {
                    var curpStored = dbContext.People.Where( p => p.DeletedAt == null && p.Curp == personRequest.Curp.Trim() && p.Id != _personID ).Count();
                    if(curpStored > 0){
                        errorsRelations.Add( "curp", new string[]{ "El CURP ingresado ya está registrado en nuestro sistema."} );
                    }
                    person.Curp = personRequest.Curp.Trim();
                }
                else
                {
                    person.Curp = String.Empty;
                }
            }

            if(!string.IsNullOrEmpty(personRequest.Email))
            {
                var emailStored = dbContext.People.Where(p => p.DeletedAt == null && p.Email == personRequest.Email.Trim() && p.Id != _personID ).Count();
                if(emailStored > 0){
                    errorsRelations.Add( "email", new string[]{ "El correo ya se encuentra en uso."} );
                }

                // * save the old email for the notification
                _oldEmail = person.Email ?? string.Empty;
                person.Email = personRequest.Email.Trim();
            }

            if( errorsRelations.Values.Count > 0)
            {
                return UnprocessableEntity( new {
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

            // notify of the email changed if requires
            if (!string.IsNullOrEmpty(_oldEmail))
            {
                try
                {
                    this._logger.LogDebug("Notify of email changed to: {email}", _oldEmail);
                    await this.emailNotificationsService.SendEmailChanged(_oldEmail, person.Email!);
                }
                catch (System.Exception err)
                {
                    this._logger.LogError(err, "Failed to notify the previous email address when changing email for person ID '{PersonId}': {ErrorMessage}", person.Id, err.Message);
                }
            }

            // Aply changes
            this.dbContext.Update(person);
            this.dbContext.SaveChanges();

            // Update password
            if( !string.IsNullOrEmpty(personRequest.Password) && personRequest.Password == personRequest.ConfirmPassword ){

                if(string.IsNullOrEmpty( personRequest.OldPassword)){
                    return UnprocessableEntity( new {
                        Title = "One or more field had errors",
                        Errors = new {
                            oldPassword = "El campo es requerido para actualizar la contraseña."
                        }
                    });
                }

                // * verify the old password
                var hashedPassword = this.cryptographyService.HashData(personRequest.OldPassword??"");
                if( person.Password != hashedPassword ){
                    return Conflict(new {
                        Title = "Error al actualizar la contraseña",
                        Message = "La contraseña anterior no coincide con la almacenada en la base de datos."
                    });
                }

                try {
                    this.personService.SetPassword( person.Id, personRequest.Password);
                }
                catch (System.Exception ex) {
                    this._logger.LogError(ex, "Fail to update the user password");
                    return Conflict(new {
                        Title = "Error al actualizar la contraseña",
                        Message = ex.Message
                    });
                }
            }

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
        /// <response code="422">The email to search is not valid</response>
        [HttpGet]
        [Route("search")]
        public IActionResult SearchPerson( [FromQuery] string email, [FromQuery] int hiddenName ){
            IEnumerable<Person> people = [];
            try
            {
                people = this.personService.Search(email, null,null, null, PersonService.SearchMode.Equals );
            }
            catch (ArgumentException ae)
            {
                var errors = new Dictionary<string, string> {{ ae.ParamName??"email", ae.Message}};
                return UnprocessableEntity( new UnprocesableResponse (errors));
            }

            var person = people.FirstOrDefault();
            if( person == null){
                return NotFound( new {
                    Message = "Person not found"
                });
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
        public ActionResult<PersonResponse> GetPerson(string personID)
        {
            // Validate ID
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( personID );
            }catch(Exception){
                return BadRequest( new {
                    Message = $"Person id format not valid"
                });
            }

            var person = this.personService.GetPeople()
                .Include(p => p.Addresses!.Where( a=> a != null && a.DeletedAt == null ) )
                .Include(p => p.ContactInformations!.Where( a => a != null && a.DeletedAt == null))
                .FirstOrDefault(p => p.Id == _personID);

            if ( person == null)
            {
                return NotFound( new {
                    Message = "Person not found"
                });
            }

            var personResponse = PersonResponse.FromEntity(person);

            if (person.BannedAt != null)
            {
                var banRecord = this.personBanService.GetBanHistory(person.Id).FirstOrDefault(item => item.Activo == "BANNED");
                personResponse.BannedReason = banRecord?.Message;
            }

            return Ok(personResponse);
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

            try
            {
                var _persons = this.personService.Search( authenticateRequest.Email!, null, null, null );
                if( _persons.IsNullOrEmpty()){
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                return NotFound(new {
                    Message = "El correo no se encuentra registrado en la base de datos"
                });
            }

            var (success, person, message) = this.personService.AuthPerson( authenticateRequest.Email!, authenticateRequest.Password!);
            if(success)
            {
                return Ok(new{
                    Id = person!.Id,
                    Name = person.FullName,
                    Email = person.Email
                });
            }
            
            return Unauthorized(new {
                Message = message
            });
        }

        /// <summary>
        /// Retrieve the addresses of the person
        /// </summary>
        /// <param name="personID"></param>
        /// <response code="200">Return the addresses stored</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="404">The person has no address</response>
        [HttpGet]
        [Route("{personID}/addresses" )]
        public ActionResult<IEnumerable<AddressResponse>?> GetAllPersonAddresses( string personID )
        {
            // Validate person id
            if( !Guid.TryParse( personID, out Guid _personID)){
                return BadRequest( new {
                    Message = $"Person id format not valid"
                });
            }
            
            // Get the data
            try {
                var addressesDataRaw = this.personService.GetPersonAddress(_personID)
                    ?? throw new KeyNotFoundException("Not address data found for person id " + _personID.ToString());

                    if(!addressesDataRaw.Any()){
                        throw new KeyNotFoundException("Not address data found for person id " + _personID.ToString());
                    }

                return Ok( addressesDataRaw.Select( item => AddressResponse.FromEntity(item) ).ToArray() );
            }
            catch (KeyNotFoundException knfe){
                this._logger.LogError( knfe, "Error at retrieving the addresses of the person {personId}", _personID.ToString() );
                return NotFound( new {
                    Title = "No existe registro de dirección para esta persona.",
                    knfe.Message
                } );
            }
            catch (Exception ex)
            {
                this._logger.LogError( ex, "Error at retrieving the addresses of the person {personId}", _personID.ToString() );
                return BadRequest( new {
                    Title = "Error al obtener las direcciones de la persona",
                    ex.Message
                } );
            }
        }
        
        /// <summary>
        /// Retrieve the last address stored of the person
        /// </summary>
        /// <param name="personID"></param>
        /// <response code="200">Return the addresses stored</response>
        /// <response code="400">The request is not valid</response>
        [HttpGet]
        [Route("{personID}/address" )]
        public ActionResult<AddressResponse?> GetLastPersonAddress( string personID )
        {
            // Validate person id
            if( !Guid.TryParse( personID, out Guid _personID)){
                return BadRequest( new {
                    Message = $"Person id format not valid"
                });
            }
            
            // Get the data
            try {
                var addressesDataRaw = this.personService.GetPersonAddress(_personID)
                    ?? throw new KeyNotFoundException("Not address data found for person id " + _personID.ToString());
                
                if(!addressesDataRaw.Any()){
                    throw new KeyNotFoundException("Not address data found for person id " + _personID.ToString());
                }
                

                var lastAddressRaw = addressesDataRaw.OrderByDescending( a => a.CreatedAt).First();

                return Ok( AddressResponse.FromEntity(lastAddressRaw) );
            }
            catch (KeyNotFoundException knfe){
                this._logger.LogError( knfe, "Error at retrieving the addresses of the person {personId}", _personID.ToString() );
                return NotFound( new {
                    Title = "No existe registro de dirección para esta persona.",
                    knfe.Message
                } );
            }
            catch (Exception ex)
            {
                this._logger.LogError( ex, "Error at retrieving the addresses of the person {personId}", _personID.ToString() );
                return BadRequest( new {
                    Title = "Error al obtener las direcciones de la persona",
                    Message = ex.Message
                } );
            }
        }
        
        /// <summary>
        /// Retrieve the contact information of the person
        /// </summary>
        /// <param name="personID"></param>
        /// <response code="200">Return the contact information of the person</response>
        /// <response code="400">The request is not valid</response>
        [HttpGet]
        [Route("{personID}/contactInformations" )]
        public ActionResult<IEnumerable<ContactResponse>?> GetAllContactInformation(string personID)
        {
            // Validate person id
            if( !Guid.TryParse( personID, out Guid _personId)){
                return BadRequest( new {
                    Message = "Person id format not valid"
                });
            }

            // Get contact information
            try {
                var contactInformationDataRaw = this.personService.GetAllContactInformation( _personId)
                    ?? throw new Exception("No contact information found for person id " + _personId.ToString() );

                return Ok(  contactInformationDataRaw.Select( item => ContactResponse.FromEntity(item)) );
            }catch(Exception ex){
                this._logger.LogError(ex, "Error at retrieving the contact information of person id {personId}", _personId.ToString() );
                return BadRequest( new {
                    Title = $"Error al obtener la informacion de contacto de la persona {_personId.ToString()}",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieve the contact information of the person
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="typeId"></param>
        /// <response code="200">Return the contact information of the person</response>
        /// <response code="400">The request is not valid</response>
        [HttpGet]
        [Route("{personID}/contactInformation" )]
        public ActionResult<ContactResponse?> GetLastContactInformation(string personID, [FromQuery] int? typeId = 0)
        {
            // Validate person id
            if( !Guid.TryParse( personID, out Guid _personId)){
                return BadRequest( new {
                    Message = "Person id format not valid"
                });
            }

            // Get contact information
            try {
                var contactInformationDataRaw = this.personService.GetAllContactInformation( _personId, typeId??0 )
                    ?? throw new Exception("No contact information found for person id " + _personId.ToString() );

                var lastContactInformation = contactInformationDataRaw.OrderByDescending( p => p.CreatedAt).FirstOrDefault();
                
                if( lastContactInformation != null ){
                    return Ok(  ContactResponse.FromEntity( lastContactInformation ) );
                }

                return BadRequest( new {
                    Message = "No contact information found for person id" + _personId.ToString()
                });
                
                
            }catch(Exception ex){
                this._logger.LogError(ex, "Error at retrieving the contact information of person id {personId}", _personId.ToString() );
                return BadRequest( new {
                    Title = $"Error al obtener la informacion de contacto de la persona {_personId.ToString()}",
                    Message = ex.Message
                });
            }
        }



        /// <summary>
        /// Send email for reset the password
        /// </summary>
        /// <response code="200">Email sended successfully</response>
        /// <response code="404">The email was not found on the database</response>
        /// <response code="409">Fail to send the email</response>
        /// <response code="422">Fail on validate the request parameters</response>
        [HttpPost]
        [Route("password/reset" )]
        public async Task<ActionResult<ContactResponse?>> SendEmailResetPassword( [FromBody] ResetPasswordEmailRequest resetPasswordEmailRequest )
        {
            // * Retrive person by the email
            Person? person = null;
            try
            {
                person = this.personService.Search( resetPasswordEmailRequest.Email!, null, null, null ).FirstOrDefault();
                if( person == null){
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                return NotFound( new {
                    Title="Email not found",
                    Message = $"The email {resetPasswordEmailRequest.Email} was not found on the database"
                });
            }

            // * Send email
            try {
                var emailID = await emailNotificationsService.SendResetPasswordEmailv2(person)
                    ?? throw new Exception($"Error at sending the email to {person.Email!}, the response was null");

                _logger.LogInformation("Email sended for reset the password of the user ID:'{person_id}' to the email '{email}' : response:{response}", person.Id.ToString(), person.Email!, emailID );
                return Ok( new {
                    Title = $"Email sended",
                    Message = $"Email for reset the password was sended to '{person.Email!}'"
                });
            }
            catch(Exception err){
                _logger.LogError(err, "Error at attempting to send the email for reset the password of the user ID:'{person_Id}' to the email '{email}'", person.Id.ToString(), person.Email! );
                return Conflict ( new {
                    Title = "Unhandle exception attempting to send the email",
                    err.Message
                });
            }
        }


        /// <summary>
        /// Udate the person by token
        /// </summary>
        /// <param name="resetPasswordRequest"></param>
        /// <returns></returns>
        /// <response code="200">The password was updated</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="422">Codigo no valido o caducado </response>
        [HttpPatch]
        [Route ("password/reset")]
        public IActionResult ResetPassword( [FromBody] ResetPasswordRequest resetPasswordRequest )
        {
            // Validate request
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            
            try{

                // * Validate reset password code
                Guid? personID = this.resetPasswordState.Validate( resetPasswordRequest.Code );
                if( personID == null){
                    return UnprocessableEntity( new {
                        Title = "One or more field had errors",
                        Errors = new {
                            code = "Codigo no es valido o ha caducado"
                        }
                    });
                }
                
                // * Retrive the person data
                Person person = this.dbContext.People.Find(personID) ?? throw new KeyNotFoundException($"Person id {personID} not found");

                // * Update the password
                
                this.personService.SetPassword( person.Id, resetPasswordRequest.Password);

                // * Remove reset password record
                this.resetPasswordState.Remove( person.Id);
                
                // Return response
                return Ok( new {
                    Title = "Password updated",
                    Message = $"The password of the person email '{person.Email!}' was updated"
                });

            }catch(KeyNotFoundException  knfe){
                return BadRequest( new {
                    Title = "Element not found",
                    knfe.Message
                });
            }catch(Exception err){
                this._logger.LogError( err, "Error at attempting to reset the password of the token [{token}]", resetPasswordRequest );
                return BadRequest( new {
                    Title = "Unhandle exception",
                    err.Message
                });
            }

        }


        /// <summary>
        /// Send email with a code for validate the new email
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="updateEmailRequest"></param>
        /// <returns code="200">Email sended</returns>
        /// <returns code="400">Request payload no valid</returns>
        /// <returns code="404">Person not found</returns>
        /// <returns code="409">Error at sending the mail with the code</returns>
        /// <returns code="422">New email not valid or already stored</returns>
        /// <returns></returns>
        [HttpPost]
        [Route("{personID}/updateEmail/sendCode" )]
        public IActionResult SendCode2ChangedEmail(string personID, [FromBody] UpdateEmailRequest updateEmailRequest){
            
            // * Validate request
            if (!ModelState.IsValid) {
                return BadRequest( ModelState );
            }

            // * prevenet email diplicated
            if( dbContext.People.Where( p => p.DeletedAt == null && p != null && p.Email == updateEmailRequest.Email ).Any() ){
                return UnprocessableEntity(new {
                    Title = "One or more field had errors",
                    Errors = new {
                        email = "El correo ya se encuentra almacenado en la base de datos.",
                    }
                });
            }

            // * Get the person
            Guid _personID = Guid.Empty;
            try {
                _personID = Guid.Parse( personID );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id format not valid"
                });
            }
            var person = this.personService.GetPeople() .FirstOrDefault(p => p.Id == _personID);
            if ( person == null) {
                return NotFound(new {
                    Title = "Person not found",
                    Message = "Person not found"
                });
            }

            // * Check if the person has an active processing before attempting to change the email
            if(CheckIfHasActiveProcessing(person))
            {
                return UnprocessableEntity(new {
                    Title = "No se puede cambiar el correo",
                    Errors = new {
                        email = new string[] {"No es posible actualizar el correo electrónico del usuario debido a que actualmente tiene un procedimiento activo relacionado con la constancia de antecedentes penales."}
                    }
                });
            }

            // * attemp to send the validation email code
            try {
                var emailSendingTask = this.emailNotificationsService.SendChangeEmailCode(person, updateEmailRequest.Email);
                emailSendingTask.Wait();

                return Ok( new {
                    Title = "Email de verificacion enviado",
                    Message = $"Code sended to '{updateEmailRequest.Email}'"
                });
            }
            catch(Exception err){
                _logger.LogError(err, "Error at attempting to send the email for reset change the email of the user ID:'{person_Id}' to the email '{email}'", person.Id.ToString(), updateEmailRequest.Email );
                return Conflict( new {
                    Title = "Error al enviar el codigo de verificacion.",
                    err.Message
                });
            }

        }


        /// <summary>
        /// Updates the person's email address after validating the verification code sent previously.
        /// </summary>
        /// <param name="newEmailRequest"></param>
        /// <returns code="200">Email sended</returns>
        /// <returns code="400">Request payload no valid</returns>
        /// <returns code="409">Error at updating the email</returns>
        /// <returns code="422">Validation code and new email nod valid or expired</returns>
        /// <returns></returns>
        [HttpPatch]
        [Route("updateEmail" )]
        public async Task<IActionResult> ChangeEmail( [FromBody] NewEmailRequest newEmailRequest){

            // Validate request
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            
            try {

                // * Validate reset password code
                Guid? personID = this.changeEmailState.Validate( newEmailRequest.ValidationCode, newEmailRequest.Email);
                if( personID == null){
                    return UnprocessableEntity( new {
                        Title = "One or more field had errors",
                        Errors = new {
                            ValidationCode = "El codigo no es valido o ha caducado"
                        }
                    });
                }

                // * Retrive the person data
                Person person = this.dbContext.People.Find(personID) ?? throw new KeyNotFoundException($"Person id {personID} not found");

                // * Check if the person has an active processing before update the email
                if(CheckIfHasActiveProcessing(person))
                {
                    return UnprocessableEntity(new {
                        Title = "No se puede cambiar el correo",
                        Errors = new {
                            email = new string[] {"No es posible actualizar el correo electrónico del usuario debido a que actualmente tiene un procedimiento activo relacionado con la constancia de antecedentes penales."}
                        },
                    });
                }

                // * Send a notification to the previous email
                try
                {
                    var _oldEmail = person.Email ?? throw new ArgumentException("El usuario no tiene un correo electrónico válido registrado.", nameof(person.Email));
                    await this.emailNotificationsService.SendEmailChanged(_oldEmail, newEmailRequest.Email);
                }
                catch (System.Exception err)
                {
                    this._logger.LogError(err, "Failed to notify the previous email address when changing email for person ID '{PersonId}': {ErrorMessage}", person.Id, err.Message);
                }

                // * Update the email
                this.personService.UpdateEmail(person.Id, newEmailRequest.Email);

                // * Remove reset password record
                this.changeEmailState.Remove(person.Id);
                
                // Return response
                return Ok( new {
                    Title = "Correo actualizar",
                    Message = $"El correo se actualizo el correo por '{newEmailRequest.Email!}'."
                });

            }catch(KeyNotFoundException  knfe){
                return BadRequest( new {
                    Title = "Element not found",
                    knfe.Message
                });
            }catch(Exception err){
                this._logger.LogError( err, "Error at attempting to change the email");
                return Conflict( new {
                    Title = "Error no controlado al actualizar el correo",
                    err.Message
                });
            }

        }
        
        /// <summary>
        /// Search people by name, curp and rfc
        /// </summary>
        /// <param name="search"></param>
        /// <returns code="200">List of people seached</returns>
        /// <returns code="422">The search parameter is invalid</returns>/// 
        /// <returns></returns>
        [HttpGet]
        [Route("search-people")]
        public ActionResult<IEnumerable<PersonSearchResponse>> SearchPeople([FromQuery] string? search ){

            // * search for the people
            IEnumerable<Person>? peopleRaw = [];
            try
            {
                peopleRaw = this.personService.Search(search??"");
            }
            catch (ArgumentException ae)
            {
                var errors = new Dictionary<string, string> {{ ae.ParamName??"search", ae.Message}};
                return UnprocessableEntity( new UnprocesableResponse (errors));
            }

            // * process the data
            List<PersonSearchResponse> people = peopleRaw.Select( item => PersonSearchResponse.FromEntity(item)).ToList();
            
            return Ok(people);
        }


        [HttpPost("/api/people/generate-fake-users")]
        public IActionResult GenerateUsers(){
            var createdPeople = new List<string>();

            for(int i=0; i<=666; i++){
                var _name = Faker.Name.First();
                var _random = _name + Faker.Identification.MedicareBeneficiaryIdentifier() + Faker.Identification.MedicareBeneficiaryIdentifier();

                var requ = new PersonRequest() {
                    Rfc =  _random.ToUpper().Substring(0, 12),
                    Curp = _random.ToUpper().Substring(0, 18),
                    Name = _name,
                    FirstName = Faker.Name.Middle(),
                    LastName = Faker.Name.Last(),
                    Email = Faker.Internet.Email(),
                    Birthdate = Faker.Identification.DateOfBirth(),
                    GenderId = context.Gender.ElementAt( (int) Faker.RandomNumber.Next(1,2)).Id,
                    MaritalStatusId = context.MaritalStatus.ElementAt( (int) Faker.RandomNumber.Next(1,2)).Id,
                    NationalityId = 31,
                    OccupationId = context.Occupation.ElementAt( Faker.RandomNumber.Next(1,205)).Id,
                    AppName = "API",
                    Password = "password01",
                    ConfirmPassword = "password01",
                };

                var person = this.personService.StorePerson(requ);

                createdPeople.Add(person!.FullName);
            }

            return Ok( createdPeople );
        }


        private bool CheckIfHasActiveProcessing(Person person)
        {
            // * get the last proceding
            var lastProceding = dbContext.Proceeding.Where(p=> p.PersonId == person.Id)
                .Include(p => p.Status)
                .Where(p=>EF.Functions.Like(p.Name, "%antecedentes penales%"))
                .GroupBy(p => p.DenunciaId)
                .Select(g => g.OrderByDescending(p => p.UpdatedAt).First())
                .ToList()
                .OrderByDescending(p=>p.UpdatedAt)
                .FirstOrDefault();

            if( lastProceding == null )
            {
                return false;
            }
            
            // Check if the status is not one of the completed/canceled ones
            var statusesToExclude = new List<string>() { "ATENDIDA", "CANCELADA" };
            return !statusesToExclude.Contains(lastProceding.Status?.Name, StringComparer.OrdinalIgnoreCase);
        }
    }
}