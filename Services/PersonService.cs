using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Services
{
    public class PersonService(DirectoryDBContext dbContext, ICryptographyService cryptographyService, ILogger<PersonService> logger)
    {
        
        private readonly DirectoryDBContext dbContext = dbContext;
        private readonly ICryptographyService cryptographyService = cryptographyService;
        private readonly ILogger<PersonService> logger = logger;

        public enum SearchMode {
            Like = 1,
            Equals = 2
        }

        public IQueryable<Person> GetPeople()
        {
            return this.dbContext.People
                .Include(p => p.Gender)
                .Include(p => p.MaritalStatus)
                .Include(p => p.Nationality)
                .Include(p => p.Occupation)
                .OrderBy(p => p.CreatedAt)
                .AsQueryable();
        }

        public IEnumerable<Person> Search( string email, string? curp, string? name, SearchMode searchMode = SearchMode.Equals ){

            var peopleQuery = this.GetPeople();

            if( !string.IsNullOrEmpty( email)){
                peopleQuery = peopleQuery.Where( p =>
                    (searchMode == SearchMode.Equals)
                        ? (p.Email??"").ToLower().Equals(email.ToLower())
                        : p.Email != null && p.Email.Contains(email)
                );
            }

            if( !string.IsNullOrEmpty(curp)){
                peopleQuery = peopleQuery.Where( p =>
                    (searchMode == SearchMode.Equals)
                        ? p.Curp.ToLower().Equals(curp.ToLower())
                        : p.Curp.Contains(curp)
                );
            }

            if( !string.IsNullOrEmpty(name)){
                peopleQuery = peopleQuery.Where( p => 
                    (p.Name??"").Contains(name) ||
                    (p.FirstName??"").Contains(name) ||
                    (p.LastName??"").Contains(name)
                );
            }

            return peopleQuery.ToArray();

        }


        /// <summary>
        /// Stored new person
        /// </summary>
        /// <param name="personRequest"></param>
        /// <param name="personId">Set the person id (optional)</param>
        /// <exception cref="ValidationException"></exception>
        public Person? StorePerson( PersonRequest personRequest, Guid? personId = null, DateTime? validateAt = null)
        {

            // * Create person entity
            var _person = new Person(){
                Rfc = personRequest.Rfc??"",
                Curp = personRequest.Curp!,
                Name = personRequest.Name,
                FirstName = personRequest.FirstName,
                LastName = personRequest.LastName,
                Email = personRequest.Email,
                Birthdate = personRequest.Birthdate!.Value,
                AppOwner = personRequest.AppName
            };

            if( personId != null){
                _person.Id = personId.Value;
            }

            var errorsRelations = new Dictionary<string, object>();

            // Validate unique parameters
            var rfcStored =  dbContext.People.Where( p => p.DeletedAt == null && p.Rfc == personRequest.Rfc ).Count();
            if(rfcStored > 0){
                errorsRelations.Add( "rfc", new string[]{ $"El RFC ya se encuentra en la base de datos"} );
            }

            var curpStored =  dbContext.People.Where( p => p.DeletedAt == null && p.Curp == personRequest.Curp ).Count();
            if(curpStored > 0){
                errorsRelations.Add( "curp", new string[]{ $"El CURP ya se encuentra en la base de datos"} );
            }

            var emailStored =  dbContext.People.Where( p => p.DeletedAt == null && p.Email == personRequest.Email ).Count();
            if(emailStored > 0){
                errorsRelations.Add( "email", new string[]{ $"El correo electronico ya se encuentra en la base de datos"} );
            }

            if( !string.IsNullOrEmpty(personRequest.Password) ){
                if( !personRequest.Password.Equals( personRequest.ConfirmPassword, StringComparison.CurrentCultureIgnoreCase )){
                    errorsRelations.Add( "ConfirmedPassword", "Las contraseñas no coinciden." );
                }else{
                    _person.Password = cryptographyService.HashData( personRequest.Password );
                }
            }
            
            // Validate relations
            if( personRequest.GenderId != null){
                try{
                    _person.Gender = dbContext.Gender.Find(personRequest.GenderId) ?? throw new Exception("Genero no encontrado");
                }catch(Exception err)
                {
                    errorsRelations.Add("GenderID", err.Message);
                }
            }

            if( personRequest.MaritalStatusId != null){
                try{
                    _person.MaritalStatus = dbContext.MaritalStatus.Find(personRequest.MaritalStatusId) ?? throw new Exception("Estado civil no encontrado");
                }catch(Exception err)
                {
                    errorsRelations.Add("MaritalStatusId", err.Message);
                }
            }

            if( personRequest.NationalityId != null){
                try{
                    _person.Nationality = dbContext.Nationality.Find(personRequest.NationalityId) ?? throw new Exception("Nacionalidad no encontrado");
                }catch(Exception err)
                {
                    errorsRelations.Add("NationalityId", err.Message);
                }
            }

            if( personRequest.OccupationId != null){
                try{
                    _person.Occupation = dbContext.Occupation.Find(personRequest.OccupationId) ?? throw new Exception("Ocupacion no encontrado");
                }catch(Exception err)
                {
                    errorsRelations.Add("OccupationId", err.Message);
                }
            }

            if( !errorsRelations.IsNullOrEmpty() ){
                throw new ValidationException("Some errors at store the person", null, errorsRelations );
            }

            if( validateAt != null){
                _person.ValidatedAt = validateAt.Value;
            }

            // * Register person
            this.dbContext.People.Add( _person );
            this.dbContext.SaveChanges();

            return this.dbContext.People.Find(_person.Id);
        }


        /// <summary>
        /// Set a new password for the person
        /// </summary>
        /// <param name="person_id"></param>
        /// <param name="password"></param>
        /// <exception cref="ValidationException"></exception>
        public void SetPassword( Guid person_id, string password){
            var person = dbContext.People.Find(person_id)?? throw new ValidationException("Person not found");
            person.Password = cryptographyService.HashData( password );
            dbContext.People.Update( person);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Set a new email
        /// </summary>
        /// <param name="person_id"></param>
        /// <param name="email"></param>
        /// <exception cref="ValidationException"></exception>
        public void UpdateEmail( Guid person_id, string email){
            var person = dbContext.People.Find(person_id)?? throw new ValidationException("Person not found");
            person.Email = email.Trim();
            dbContext.People.Update( person);
            dbContext.SaveChanges();
        }


        public Person? AuthPerson( string email, string password)
        {
            var hashedPassword = this.cryptographyService.HashData(password);
            return this.dbContext.People.Where( p => p.Email == email && p.Password == hashedPassword).FirstOrDefault();
        }
        
        public IEnumerable<Address>? GetPersonAddress(Guid personId){
            return this.dbContext.Addresses
                .Where( item => item.Person.Id == personId &&  item.DeletedAt == null )
                .ToArray();
        }

        public IEnumerable<ContactInformation>? GetAllContactInformation( Guid personId, int contactTypeId = 0){
            var _query = this.dbContext.ContactInformations
                .Where( item => item.Person.Id == personId && item.DeletedAt == null)
                .AsQueryable();
                
            if(contactTypeId > 0){
                _query = _query.Where( c => c.ContactType.Id == contactTypeId);
            }
            return _query.ToList();
        }

        /// <summary>
        /// Update the password
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <exception cref="ValidationException"> The old password is not valid</exception>
        public void UpdateThePassword( Guid personId, string oldPassword, string newPassword){
            var hashedPassword = this.cryptographyService.HashData(oldPassword);
            var _person = this.dbContext.People.Where( p => p.Id == personId && p.Password == hashedPassword).FirstOrDefault();
            if( _person == null){
                throw new ValidationException("La contraseña no es valida");
            }

            this.SetPassword( _person.Id, newPassword);
        }

    }  

    
}