using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Helper;
using Microsoft.Extensions.Options;

namespace AuthApi.Services
{
    public class EmailNotificationsService
    {
        private readonly IEmailProvider emailProvider;
        private readonly ChangeEmailState changeEmailState;
        private readonly ResetPasswordState resetPasswordState;
        private readonly ResetPasswordSettings resetPasswordSettings;
        private readonly JwtSettings jwtSettings;

        public EmailNotificationsService(IOptions<ResetPasswordSettings> optionsResetPasswordSettings, IEmailProvider emailProvider, ChangeEmailState changeEmailState, JwtSettings jwtSettings, ResetPasswordState resetPasswordState)
        {
            this.resetPasswordSettings = optionsResetPasswordSettings.Value;
            this.emailProvider = emailProvider;
            this.changeEmailState = changeEmailState;
            this.jwtSettings = jwtSettings;
            this.resetPasswordState = resetPasswordState;
        }

        public async Task<string> SendResetPasswordEmail(Person person){
            // * Set token life time for 1 hour
            var tokenLifeTime = TimeSpan.FromSeconds( resetPasswordSettings.TokenLifeTimeSeconds );

            // * Generate the token
            var claims = new Dictionary<string,string>(){
                {"id", person.Id.ToString()},
                {"email", person.Email!}
            };
            var token = await JwTokenHelper.GenerateJwtToken(claims, jwtSettings, tokenLifeTime);

            // * Generate html
            var htmlBody = EmailTemplates.ResetPassword( resetPasswordSettings.DestinationUrl + $"?t={token}" );

            // * Send email
            return await emailProvider.SendEmail( person.Email!, "Restablecer contraseña", htmlBody );
        }
        
        public async Task<string> SendResetPasswordEmailv2(Person person)
        {
            // * Set token life time for 1 hour
            var tokenLifeTime = TimeSpan.FromSeconds(resetPasswordSettings.TokenLifeTimeSeconds);
            var date = DateTime.Now.Add(tokenLifeTime);

            var resetCode = string.Empty;

            // * Check if a previous record exist
            var _record = resetPasswordState.GetByPersonId(person.Id);
            if (_record != null)
            {
                // * check if the code is still valid
                if (resetPasswordState.Validate(_record.ResetCode) != null)
                {
                    // * save the code to be reused
                    resetCode = _record.ResetCode;
                }
            }


            // * if a previous reset code does not exist, make a new one
            if (string.IsNullOrEmpty(resetCode))
            {
                // * Generate the new code
                var _guidString = Guid.NewGuid().ToString().ToUpper();
                resetCode = _guidString.Substring(_guidString.Length - 6);
            }

            // * Store the record
            resetPasswordState.AddRecord(person.Id, resetCode, date);

            // * Generate html
            var htmlBody = EmailTemplates.ResetPasswordCode(resetCode, date.ToShortTimeString());

            // * Send email
            return await emailProvider.SendEmail(person.Email!, "Restablecer contraseña", htmlBody);
        }

        public async Task<string> SendChangeEmailCode(Person person, string newEmail){
            // * Set token life time for 1 hour
            var tokenLifeTime = TimeSpan.FromSeconds( resetPasswordSettings.TokenLifeTimeSeconds );

            // * Generate the code
            var _guidString = Guid.NewGuid().ToString().ToUpper();
            var resetCode = _guidString.Substring(_guidString.Length - 6);

            var lifeTime = TimeSpan.FromSeconds( this.resetPasswordSettings.TokenLifeTimeSeconds );
            var date = DateTime.Now.Add(lifeTime);

            // * Store the record
            changeEmailState.AddRecord( person.Id, resetCode, date, newEmail);


            // * Generate html
            var htmlBody = EmailTemplates.CodeChangeEmail( resetCode, date.ToShortTimeString() );

            // * Send email
            return await emailProvider.SendEmail(
                emailDestination: newEmail,
                subject: "Solicitud de Cambio de Correo Electrónico",
                data: htmlBody
            );
        }

    }
}