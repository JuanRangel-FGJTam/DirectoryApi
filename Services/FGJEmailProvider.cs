using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthApi.Data;
using AuthApi.Helper;
using System.Net.Http.Headers;


namespace AuthApi.Services
{
    public class FGJEmailProvider(ILogger<FGJEmailProvider> logger ,IOptions<EmailSettings> optionsEmailSettings) : IEmailProvider
    {
        private readonly EmailSettings emailSettings = optionsEmailSettings.Value;
        private readonly ILogger<FGJEmailProvider> logger = logger;

        public async Task<string> SendEmail(string emailDestination, string subject, object data)
        {

            // Create the payload of the email
            var payload = new {
                from = emailSettings.From,
                to = emailDestination,
                subject = subject,
                message = data.ToString()
            };

            // Convert the payload to JSON string
            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

            // Create httpclient and request
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", emailSettings.Token);
            var request = new HttpRequestMessage(HttpMethod.Post, emailSettings.ApiUri)
            {
                Content = new StringContent( jsonPayload, Encoding.UTF8, "application/json")
            };
            
            // Send the POST request
            var response = await httpClient.SendAsync(request);

            // Check if request was successful
            if(!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to send email. Status code: {statuscode}", response.StatusCode );
                throw new Exception($"Can't send the email, bad response: {response.StatusCode}" );
            }
            
            // Process response
            return await response.Content.ReadAsStringAsync();
        }

    }
}