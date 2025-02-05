using Fanex.CheckDataConsistency.Business.Model;
using Fanex.CheckDataConsistency.Business.Services;
using Fanex.MailDelivery.Client.Jobs;
using Fanex.MailDelivery.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Fanex.CheckDataConsistency.Service.Services;

public class ExecuteServiceRule50MoveBet : JobBase
{
    protected override JobMessage Run(Schedule schedule, IJobExecutionContext jobContext)
    {
        var services = ServiceCollectionExtensions.Services;
        var serviceProvider = services.BuildServiceProvider();

        var mainService = serviceProvider.GetService<MainRule50Service>();
        var result = mainService!.Execute(Rule.Rule50MoveBet);
        return new JobMessage
        {
            JobStatus = result.JobStatus,
            Schedule = schedule,
            Mail = result.Mail
        };
    }
}