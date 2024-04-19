using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Data.Exceptions;
using AuthApi.Entities;
using AuthApi.Models;

namespace AuthApi.Services
{
    public class PreregisterService( DirectoryDBContext dbContext, ICryptographyService cryptographyService, ILogger<PreregisterService> logger, PersonService personService)
    {
        private readonly DirectoryDBContext dbContext = dbContext;
        private readonly ICryptographyService cryptographyService = cryptographyService;
        private readonly ILogger<PreregisterService> logger = logger;
        private readonly PersonService personService = personService;

        
        /// <summary>
        ///  Create a new preregister record
        /// </summary>
        /// <param name="request"></param>
        /// <returns> People pre-registration id </returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SimpleValidationException"></exception>
        public string? CreatePreregister(PreregistrationRequest request){

            // Prevenet email diplicated
            if( dbContext.People.Where( p => p.DeletedAt == null && p != null && p.Email == request.Mail ).Count() > 0 ){
                var errors = new List<KeyValuePair<string, string>> {
                    new("email", "El correo ya se encuentra almacenado en la base de datos.")
                };
                throw new SimpleValidationException("Can't stored the pre-register", errors );
            }


            // Verify if a same preregistration is already stored in the database
            var _preRegister = dbContext.Preregistrations
                .Where(p => p.Mail!.ToLower() == request.Mail.ToLower())
                .OrderByDescending( p => p.CreatedAt )
                .FirstOrDefault();

            
            if( _preRegister != null){
                
                // Update the password if are diferent
                if( _preRegister.Password != request.Password){
                    _preRegister.Password = request.Password;
                    dbContext.Preregistrations.Update( _preRegister);
                    dbContext.SaveChanges();
                }

                // Return the stored 
                return _preRegister.Id.ToString();
            }
            
            // Creat new pre-register record
            var preregistration = new Preregistration(){
                Mail = request.Mail,
                Password = request.Password
            };

            // Insert record into db
            dbContext.Preregistrations.Add( preregistration );
            dbContext.SaveChanges();

            // Return the id generated
            return preregistration.Id.ToString();

        }

        public Person? ValidateRegister( Guid preregisterId, ValidateRegisterRequest request){
            
            // Retrive validation enity
            var preregister = this.dbContext.Preregistrations.Find(preregisterId);
            if( preregister == null){
                //TODO: Handle entity not found
                return null;
            }

            // Generate new person request
            var newPersonRequest = new PersonRequest(){
                Rfc = request.Rfc,
                Curp = request.Curp,
                Name = request.Name,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Birthdate = request.Birthdate,
                GenderId = request.GenderId,
                MaritalStatusId = request.MaritalStatusId,
                NationalityId = request.NationalityId,
                OccupationId = request.OccupationId,
                AppName = request.AppName,
                Email = preregister.Mail,
                Password = preregister.Password,
                ConfirmPassword = preregister.Password,
            };


            // Create person record
            var newPerson = personService.StorePerson(newPersonRequest, preregister.Id) ?? throw new Exception("Can't store the new person, null data was returned");

            // Delete the pre-register record
            try{
                this.dbContext.Preregistrations.Remove( preregister);
                dbContext.SaveChanges();
            }catch(Exception err){
                this.logger.LogError("Can't delete the pre-register record; {message}", err.Message );
            }

            return newPerson;

        }

    }
}