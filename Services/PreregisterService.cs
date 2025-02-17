using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Data.Exceptions;
using AuthApi.Entities;
using AuthApi.Helper;
using AuthApi.Models;
using Microsoft.Extensions.Options;

namespace AuthApi.Services
{
    public class PreregisterService( DirectoryDBContext dbContext, ICryptographyService cryptographyService, ILogger<PreregisterService> logger, PersonService personService, IEmailProvider emailProvider, IOptions<WelcomeEmailSources> WelcomeEmailSourcesOptions, IOptions<ResetPasswordSettings> resetPasswordOptions)
    {
        private readonly DirectoryDBContext dbContext = dbContext;
        private readonly ICryptographyService cryptographyService = cryptographyService;
        private readonly ILogger<PreregisterService> logger = logger;
        private readonly PersonService personService = personService;
        private readonly IEmailProvider emailProvider = emailProvider;
        private readonly WelcomeEmailSources welcomeEmailSources = WelcomeEmailSourcesOptions.Value;
        private readonly ResetPasswordSettings resetPasswordSettings = resetPasswordOptions.Value;
        
        /// <summary>
        ///  Create a new preregister record
        /// </summary>
        /// <param name="request"></param>
        /// <returns> People pre-registration id </returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SimpleValidationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string?> CreatePreregister(PreregistrationRequest request)
        {

            // * Prevenet email diplicated
            if( dbContext.People.Where(p => p.DeletedAt == null && p != null && p.Email == request.Mail ).Any())
            {
                var errors = new List<KeyValuePair<string, string>> {
                    new("email", "El correo ya se encuentra almacenado en la base de datos.")
                };
                throw new SimpleValidationException("Can't stored the pre-register", errors);
            }

            // * prepare the validation code and the lifetime
            string validationCode = PreregisterToken.GenerateCode();
            var lifeTime = TimeSpan.FromSeconds(resetPasswordSettings.TokenLifeTimeSeconds);
            var codeLifeTime = DateTime.Now.Add(lifeTime);


            // * Verify if a same preregistration is already stored in the database
            Preregistration? preRegister = dbContext.Preregistrations
                .Where(p => p.Mail!.ToLower() == request.Mail.ToLower())
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            if(preRegister != null)
            {
                // * Update the password if are diferent
                if(preRegister.Password != request.Password)
                {
                    preRegister.Password = request.Password;
                    dbContext.Preregistrations.Update(preRegister);
                }

                // * use the same code if the request is not expired
                if(preRegister.ValidTo >= DateTime.Now)
                {
                    validationCode = preRegister.Token;
                }
                else
                {
                    preRegister.Token = validationCode;
                }

                preRegister.ValidTo = codeLifeTime;
                preRegister.UpdatedAt = DateTime.Now;
                dbContext.SaveChanges();
            }
            else
            {
                // * Create a new pre-register record
                preRegister = new Preregistration()
                {
                    Mail = request.Mail,
                    Password = request.Password,
                    Token = validationCode,
                    ValidTo = codeLifeTime,
                    UpdatedAt = DateTime.Now
                };

                // * Insert record into db
                dbContext.Preregistrations.Add(preRegister);
                dbContext.SaveChanges();
            }


            // * Sende the email with the code
            var response = await SendEmailCode(preRegister, validationCode, codeLifeTime)
                ?? throw new Exception("Error at sending email of validation, response is null");
            logger.LogError("Error at sending email to validate the user: response {response}", response );
            

            // * Return the id generated
            return preRegister.Id.ToString();
        }

        public Person? ValidateRegister(Guid preregisterId, ValidateRegisterRequest request)
        {
            
            // Retrive validation enity
            var preregister = this.dbContext.Preregistrations.Find(preregisterId)
                ?? throw new Exception($"No se encontro el preregistro ID:{preregisterId}");
            

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
            var newPerson = personService.StorePerson(newPersonRequest, preregister.Id, DateTime.Now)
              ?? throw new Exception("Excepcion no controlada, la respuesta al registrar la persona es nula");

            // send welcome mail
            try
            {
                var taskEmail = this.SendWelcomeMail(newPerson);
                taskEmail.Wait();
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(ex, "Cant send the welcome mail");
            }


            // Delete the pre-register record
            try
            {
                this.dbContext.Preregistrations.Remove(preregister);
                dbContext.SaveChanges();
            }
            catch(Exception err)
            {
                this.logger.LogError("Can't delete the pre-register record; {message}", err.Message );
            }

            return newPerson;
        }

        public Preregistration? GetPreregistrationByToken(string token)
        {
            return this.dbContext.Preregistrations
                .Where(item => item.Token == token)
                .FirstOrDefault();
        }
        

        [Obsolete("This method is not longer used and will be removed in future versions. Use SendEmailCode instead.")]
        private async Task<string?> SendEmail( Preregistration preregistration, string? url = null){

            // * prepare the parameters
            var _subject = "¡Estás a un Paso de Crear Tu Llave Digital!";
            // TODO: create a configuration entry for the url validattion endpoint 
            var _ref = string.Format("{0}?t={1}",
                url ?? "http://auth.fgjtam.gob.mx/person/validate/email",
                preregistration.Token
            );

            // * Make html body
            var _htmlBody = EmailTemplates.ValidationEmail(_ref);

            // * Send email
            try{
                var emailID = await Task.Run<string>( async ()=>{
                    return await this.emailProvider.SendEmail(
                        emailDestination: preregistration.Mail!,
                        subject: _subject,
                        data: _htmlBody
                    );
                });
                logger.LogInformation( "Email ID:{emailID} sending", emailID);
                return emailID;
            }catch(Exception err){
                logger.LogError( err, "Error at attempting to send Email for validation to email {mail}; {message}", preregistration.Mail!, err.Message);
                return null;
            }
        }

        private async Task<string?> SendEmailCode(Preregistration preregistration, string code, DateTime time)
        {


            // * prepare the parameters
            var _subject = "¡Estás a un Paso de Crear Tu Llave Digital!";
            
            // * Make html body
            var _htmlBody = EmailTemplates.PreregisterCode(code, time.ToShortTimeString());

            // * Send email
            try
            {
                var emailID = await Task.Run<string>( async ()=>{
                    return await this.emailProvider.SendEmail(
                        emailDestination: preregistration.Mail!,
                        subject: _subject,
                        data: _htmlBody
                    );
                });
                logger.LogInformation( "Email ID:{emailID} sending", emailID);
                return emailID;
            }
            catch(Exception err)
            {
                logger.LogError( err, "Error at attempting to send Email for validation to email {mail}; {message}", preregistration.Mail!, err.Message);
                return null;
            }
        }

        public async Task<string> SendWelcomeMail(Person person){
            
            // * prepare the message resources
            dbContext.Entry(person).Reference(p => p.Gender).Load();
            var _welcome = "Bienvenido(a)";
            if( person.Gender != null){
                _welcome = person.Gender.Id == 1 ? "Bienvenido" : "Bienvenida";
            }
            var welcomeMessage = $"¡{_welcome} a la Fiscalía Digital!";
            var subjectMessage = $"{_welcome} a la Fiscalía Digital del Estado de Tamaulipas";

            // * make the email message
            var mailtemplate = EmailTemplates.Welcome(
                personFullName: person.FullName,
                imageNameSrc: welcomeEmailSources.ImageNameSrc,
                imageProfileSrc: welcomeEmailSources.ImageProfileSrc,
                welcomeMessage: welcomeMessage
            );

            // * send the email
            var response = await emailProvider.SendEmail(
                emailDestination: person.Email!,
                subject: subjectMessage,
                data: mailtemplate
            );

            return response;
        }

    }
}