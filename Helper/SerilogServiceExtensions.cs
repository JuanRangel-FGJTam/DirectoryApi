using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Serilog;

namespace AuthApi.Helper
{
    public static class SerilogServiceExtensions
    {
        public static void ConfigureSerilog(this IServiceCollection services )
        {
            var serilogConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(serilogConfiguration)
                .CreateLogger();

            services.AddLogging().AddSerilog(logger);
        }
    }
}