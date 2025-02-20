using System;
using Microsoft.Extensions.Options;
using AuthApi.Helper;

namespace AuthApi.Services;

public class UbicanosService(IOptions<UbicanosSettings> options, ILogger<UbicanosService> logger, IHttpClientFactory httpClientFactory)
{
    private readonly UbicanosSettings settings = options.Value;
    private readonly ILogger<UbicanosService> logger = logger;
    private readonly HttpClient httpClient = httpClientFactory.CreateClient("ubicanosclient");


    /// <summary>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Fail at retrive the data</exception>
    public async Task<dynamic> GetLocations()
    {
        // Create httpclient and request
        using var httpClient = new HttpClient(){
            BaseAddress = new Uri(settings.Host)
        };

        // Send the POST request
        var response = await httpClient.GetAsync("/api/municipalities");

        // Check if request was successful
        if(!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to retrive the data from the API. Status code: {statuscode}", response.StatusCode );
            throw new KeyNotFoundException($"Can't retrive the data, bad response: {response.StatusCode}" );
        }

        // Process response
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Id not found</exception>
    /// <exception cref="HttpRequestException">Fail at retrive the data</exception>
    public async Task<dynamic> GetMunicipality(string municipalityId)
    {
        // Create httpclient and request
        using var httpClient = new HttpClient(){
            BaseAddress = new Uri(settings.Host)
        };

        // Send the POST request
        var response = await httpClient.GetAsync($"/api/municipalities/{municipalityId}");

        // Check if request was successful
        if(!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to retrive the data from the API. Status code: {statuscode}", response.StatusCode );
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"The municipality was not found" );
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Id not found</exception>
    /// <exception cref="HttpRequestException">Fail at retrive the data</exception>
    public async Task<dynamic> GetLocations(string municipalityId)
    {
        // Create httpclient and request
        using var httpClient = new HttpClient(){
            BaseAddress = new Uri(settings.Host)
        };

        // Send the POST request
        var response = await httpClient.GetAsync($"/api/municipalities/{municipalityId}/locations");

        // Check if request was successful
        if(!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to retrive the data from the API. Status code: {statuscode}", response.StatusCode );
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"The municipality was not found" );
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }

        return await response.Content.ReadAsStringAsync();
    }
}
