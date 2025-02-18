using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Jobs;
using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.Core;
using Quartz.Xml.JobSchedulingData20;
using Serilog;

namespace AuthApi.Helper
{
    public static class QuartzServiceExtensions
    {
        public static void ConfigureQuartz(this IServiceCollection services )
        {
            // if you are using persistent job store, you might want to alter some options
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });
            
            services.AddQuartz( q=>{
                // handy when part of cluster or you want to otherwise identify multiple schedulers
                q.SchedulerId = "DirectoryAPI-Core";
                
                // we take this from appsettings.json, just show it's possible
                q.SchedulerName = "DirectoryAPI Scheduler";
                
                // or for scoped service support like EF Core DbContext
                // q.UseMicrosoftDependencyInjectionScopedJobFactory();

                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 10;
                });


                // here's a known job for triggers
                q.ScheduleJob<RemovePreregistersJob>(trigger => trigger
                    .WithIdentity(typeof(RemovePreregistersJob).Name)
                    .WithCronSchedule("0 0 2 * * ?") // 2 am every day
                    .WithDescription("Remove the old preregister records.")
                );

            });
            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
        }
    }
}