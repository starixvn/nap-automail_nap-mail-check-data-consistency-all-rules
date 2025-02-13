using Fanex.CheckDataConsistency.Business.Services;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.CheckDataConsistency.Service.Services;

public class MainBackgroundRule46Service(MainRule46Service mainService, IMaintenanceService um, Log log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int periodicityInMinute = 15;
        const int delay = periodicityInMinute * 60 * 1000;
        await Task.Delay(delay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            // log.Log($"Fanex.CheckDataConsistency.Service - Rule 46 - {DateTime.Now}");
            if (await um.IsUnderMaintenanceAsync())
            {
                continue;
            }

            mainService.Execute();
            await Task.Delay(delay, stoppingToken);
        }
    }
}