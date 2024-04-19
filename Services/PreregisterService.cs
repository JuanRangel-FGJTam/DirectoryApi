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
    public class PreregisterService( DirectoryDBContext dbContext, ICryptographyService cryptographyService, ILogger<PreregisterService> logger )
    {
        private readonly DirectoryDBContext dbContext = dbContext;
        private readonly ICryptographyService cryptographyService = cryptographyService;
        private readonly ILogger<PreregisterService> logger = logger;

        private readonly TimeSpan timeBeforeCreateNew = TimeSpan.FromMinutes(5);

        
        /// <summary>
        ///  Create a new preregister record
        /// </summary>
        /// <param name="request"></param>
        /// <returns> People pre-registration id </returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SimpleValidationException"></exception>
        public string? CreatePreregister(PreregistrationRequest request){

            // Prevenet email diplicated
            if( dbContext.People.Where( p => p.DeletedAt == null && p.ValidatedAt != null && p.Email == request.Mail ).Count() > 0 ){
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

    }
}