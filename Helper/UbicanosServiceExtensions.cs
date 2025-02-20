using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Services;

namespace AuthApi.Helper;

public static class UbicanosServiceExtensions
{
    public static void AddUbicanosServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<UbicanosSettings>(configuration.GetSection("UbicanosSettings"));
        services.AddHttpClient("ubicanosclient", c =>{
            c.BaseAddress = new Uri(configuration.GetValue<string>("UbicanosSettings:Host")!);
        });
        services.AddScoped<UbicanosService>();
    }
}
