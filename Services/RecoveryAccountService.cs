using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Data.Exceptions;
using AuthApi.Entities;
using AuthApi.Helper;
using AuthApi.Models;

namespace AuthApi.Services
{
    public class RecoveryAccountService(ILogger<RecoveryAccountService> logger, DirectoryDBContext directoryDBContext, PersonService personService, MinioService ms, IEmailProvider emailProvider)
    {
        private readonly ILogger<RecoveryAccountService> logger = logger;
        private readonly DirectoryDBContext directoryDBContext = directoryDBContext;
        private readonly PersonService personService = personService;
        private readonly MinioService minioService = ms;
        private readonly IEmailProvider emailProvider = emailProvider;

        public async Task<IEnumerable<AccountRecoveryResponse>> GetAllRecords(){

            // get data raw
                var records = directoryDBContext.AccountRecoveryRequests
                    .Where(item => item.DeletedAt == null && item.Files != null)
                    .Include( p => p.Files!)
                        .ThenInclude( f => f.DocumentType)
                    .OrderBy(item => item.CreatedAt)
                    .Select( item => AccountRecoveryResponse.FromEntity(item) )
                    .ToList();
            
            await Task.CompletedTask;

            return records;

        }

        public AccountRecovery? GetByID(Guid requestId){
            return this.directoryDBContext.AccountRecoveryRequests.Where(item=> item.DeletedAt == null).FirstOrDefault( item => item.Id == requestId);
        }

        public async Task<AccountRecoveryResponse?> GetRequestWithFiles(Guid requestId){
            var data = this.GetByID(requestId);
            if(data == null){
                return null;
            }

            this.directoryDBContext
                .Entry(data)
                .Collection(p => p.Files)
                .Query()
                .Include( f => f.DocumentType)
                .Load();

            var response = AccountRecoveryResponse.FromEntity(data);
            
            // get the public path
            if(response.Files.Any()){
                // * override the proceding file with the temporally url
                var fileTasks = response.Files.Select(async file => {
                    if( file.DeletedAt != null || string.IsNullOrEmpty(file.FilePath)){
                        return file;
                    }
                    var fileUrl = await minioService.MakeTemporalUrl(file.FilePath!, file.FileType??"application/pdf");
                    return new AccountRecoveryFileResponse {
                        Id = file.Id,
                        FileName = file.FileName,
                        FilePath = file.FilePath,
                        FileType = file.FileType,
                        FileSize = file.FileSize,
                        CreatedAt = file.CreatedAt,
                        FileUrl = fileUrl,
                        DeletedAt = file.DeletedAt,
                        DocumentTypeId = file.DocumentTypeId,
                        DocumentTypeName = file.DocumentTypeName
                    };
                }).ToList();
                
                response.Files = await Task.WhenAll(fileTasks);
            }

            return response;
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="SimpleValidationException"></exception>
        /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException"></exception>
        public AccountRecovery RegisterRecoveryAccountRequest(AccountRecoveryRequest request){
            
            // * parse datetime
            DateTime? birthDate1 = null;
            if(request.BirthDate != null){
                if( DateTime.TryParseExact(
                    request.BirthDate,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedDate
                )){
                    birthDate1 = parsedDate;
                }
            }
            if(birthDate1 == null){
                var errors = new Dictionary<string,string>{{"birthDate", "La fecha de nacimiento es incorrecta, el formato requerido es yyyy-MM-dd"}};
                throw new SimpleValidationException("Fecha de nacimiento incorrecta", errors);
            }

            Gender? gender = null;
            if(request.GenderId != null )
            {
                gender = directoryDBContext.Gender.FirstOrDefault(item => item.Id == request.GenderId);
            }
            
            Nationality? nacionality = null;
            if(request.NationalityId != null )
            {
                nacionality = directoryDBContext.Nationality.FirstOrDefault(item => item.Id == request.NationalityId);
            }


            // * create the record
            var accountRecoveryRequest = new AccountRecovery {
                Name = request.Name!,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = birthDate1!.Value,
                Gender = gender,
                Curp = request.Curp,
                Nationality = nacionality,
                ContactEmail = request.ContactEmail,
                ContactEmail2 = request.ContactEmail2,
                ContactPhone = request.ContactPhone,
                ContactPhone2 = request.ContactPhone2,
                RequestComments = request.RequestComments,
                CreatedAt = DateTime.Now
            };

            // * attempt to get a match por the person
            if(!string.IsNullOrEmpty(request.Curp)){
                try{
                    var person = personService.GetPeople().ToList().FirstOrDefault(p => p.Curp.Contains( request.Curp, StringComparison.CurrentCultureIgnoreCase));
                    if( person != null){
                        accountRecoveryRequest.PersonId = person.Id;
                    }
                }catch{}
            }


            this.directoryDBContext.AccountRecoveryRequests.Add(accountRecoveryRequest);
            this.directoryDBContext.SaveChanges();

            return accountRecoveryRequest;

        }
        
        public int AttachFile(AccountRecovery accountRecovery, AccountRecoveryFile accountRecoveryFile){
            accountRecoveryFile.AccountRecovery = accountRecovery;
            accountRecoveryFile.CreatedAt = DateTime.Now;
            this.directoryDBContext.AccountRecoveryFiles.Add(accountRecoveryFile);
            return this.directoryDBContext.SaveChanges();
        }

        public async Task<string?> SendSuccessEmail(string personFullName, string email)
        {
            // * prepare the parameters
            var _subject = "Actualización de Correo Electrónico";

            // * Make html body
            var _htmlBody = EmailTemplates.RecoveryAccountCompleted(personFullName, email);

            // * Send email
            try
            {
                var emailID = await Task.Run<string>( async ()=>{
                    return await this.emailProvider.SendEmail(
                        emailDestination: email,
                        subject: _subject,
                        data: _htmlBody
                    );
                });
                logger.LogInformation( "Email ID:{emailID} sending", emailID);
                return emailID;
            }
            catch(Exception err)
            {
                logger.LogError(err, "Error at attempting to send Email for validation to email {mail}; {message}", email, err.Message);
                return null;
            }
        }
        public async Task<string?> SendFailEmail(string personFullName, string email, string comments)
        {
            // * prepare the parameters
            var _subject = "Solicitud de Recuperación de Cuenta Incompleta";

            // * Make html body
            var _htmlBody = EmailTemplates.RecoveryAccountInCompleted(personFullName, comments);

            // * Send email
            try
            {
                var emailID = await Task.Run<string>( async ()=>{
                    return await this.emailProvider.SendEmail(
                        emailDestination: email,
                        subject: _subject,
                        data: _htmlBody
                    );
                });
                logger.LogInformation("Email ID:{emailID} sending", emailID);
                return emailID;
            }
            catch(Exception err)
            {
                logger.LogError(err, "Error at attempting to send Email for validation to email {mail}; {message}", email, err.Message);
                return null;
            }
        }
    }
}