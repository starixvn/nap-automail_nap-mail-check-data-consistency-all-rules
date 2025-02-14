using Fanex.CheckDataConsistency.Business.Services;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.CheckDataConsistency.Service.Services;

public class MainBackgroundRule207Service(MainRule207Service rule207Service, Log log, IMaintenanceService um) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int periodicityInMinute = 3;
        const int delay = periodicityInMinute * 60 * 1000;
        await Task.Delay(delay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (await um.IsUnderMaintenanceAsync()) continue;

            rule207Service.Execute();
            await Task.Delay(delay, stoppingToken);
        }
    }
}