using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AuthApi.Data;
using Microsoft.OpenApi.Models;
using Minio.DataModel;
using Quartz;
using Serilog;

namespace AuthApi.Jobs;
internal class RemovePreregistersJob : IJob
{
    private readonly DirectoryDBContext dbcontext;
    private readonly ILogger<RemovePreregistersJob> logger;

    public RemovePreregistersJob(DirectoryDBContext c, ILogger<RemovePreregistersJob> logger)
    {
        this.dbcontext = c;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Start job Remove Preregisters.");
        var targetDate = DateTime.Now.AddDays(-1);
        var records2Delete = this.dbcontext.Preregistrations.Where(item => item.ValidTo <= targetDate).ToList();
        this.dbcontext.Preregistrations.RemoveRange(records2Delete);
        var total = await this.dbcontext.SaveChangesAsync();
        logger.LogInformation("Finished job Remove Preregisters, {total} records removed.", total);
    }
}