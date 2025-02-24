using Fanex.CheckDataConsistency.Business.Services;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.CheckDataConsistency.Service.Services;

public class MainBackgroundRule55Service(MainRule55Service mainService, IMaintenanceService um) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int periodicityInMinute = 5;
        const int delay = periodicityInMinute * 60 * 1000;
        await Task.Delay(delay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (await um.IsUnderMaintenanceAsync())
            {
                continue;
            }

            mainService.Execute();
            await Task.Delay(delay, stoppingToken);
        }
    }
}