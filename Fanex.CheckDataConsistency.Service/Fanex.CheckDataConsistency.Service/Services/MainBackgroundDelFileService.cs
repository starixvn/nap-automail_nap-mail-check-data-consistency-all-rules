using System;
using System.Threading;
using System.Threading.Tasks;

using Fanex.CheckDataConsistency.Business.Services;

using Microsoft.Extensions.Hosting;

using ONEbook.UM.Services;

namespace Fanex.CheckDataConsistency.Service.Services;

public class MainBackgroundDelFileService(DeleteFilesService deleteFilesService, Log log, IMaintenanceService um) : BackgroundService
{
    private const int MaxRetryAttempts = 3;
    private const int RetryDelayInSeconds = 5;
    private int retryCount;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delayToNext21Hour = CalculateDelayToNext21Hour();
                log.Log($"Fanex.CheckDataConsistency.Service MainBackgroundDelFileService Waiting {delayToNext21Hour} until next run at {DateTime.UtcNow.Add(delayToNext21Hour)}");
                await Task.Delay(delayToNext21Hour, stoppingToken);

                if (!await um.IsUnderMaintenanceAsync())
                {
                    await ExecuteWithRetryAsync(stoppingToken);
                }
                else
                {
                    log.Log("System is under maintenance, skipping file deletion");
                }
            }
            catch (OperationCanceledException)
            {
                log.Log("Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                log.Log($"Error occurred: {ex.Message}");
                if (retryCount >= MaxRetryAttempts)
                {
                    log.Log($"Max retry attempts ({MaxRetryAttempts}) reached. Service will continue after delay.");
                    retryCount = 0;
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                else
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromSeconds(RetryDelayInSeconds), stoppingToken);
                }
            }
        }
    }

    private async Task ExecuteWithRetryAsync(CancellationToken stoppingToken)
    {
        try
        {
            await deleteFilesService.ExecuteAsync().WaitAsync(stoppingToken);
            retryCount = 0; // Reset retry count on successful execution
            log.Log("File deletion completed successfully");
        }
        catch (Exception ex)
        {
            log.Log($"Error during file deletion: {ex.Message}");
            throw;
        }
    }

    private static TimeSpan CalculateDelayToNext21Hour()
    {
        var now = DateTime.UtcNow;
        var nextRun = new DateTime(now.Year, now.Month, now.Day, 21, 15, 0);
        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1);
        }
        return nextRun - now;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        log.Log("MainBackgroundDelFileService is stopping");
        await base.StopAsync(cancellationToken);
    }
}
