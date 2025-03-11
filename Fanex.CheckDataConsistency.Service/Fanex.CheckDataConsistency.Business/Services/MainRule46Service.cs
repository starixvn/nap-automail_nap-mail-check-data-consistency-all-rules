using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Client;
using Fanex.MailDelivery.Core.Models;
using System;
using Timer = System.Timers.Timer;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule46Service
{
    JobMessage Execute();
}

public class MainRule46ServiceImp(
    Log log,    
    RefineDataService refineDataService,
    MailService mailService)
    : MainRule46Service
{
    private Timer timer;

    private const int MaxRetry = 4;
    private int retryCounter;
    private double interval = 120000;

    ~MainRule46ServiceImp()
    {
        Dispose(false);
    }

    private void Register()
    {
        const int millisecond = 1000;
        const int second = 60;
        const int minute = 2;
        interval = minute * second * millisecond;
        timer = new Timer
        {
            Enabled = true,
            Interval = interval
        };
        timer.Elapsed += TimerElapsed;
    }

    private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (!AppConfigService.IsOnProcessingRule46)
        {
            QueryData();
        }
    }

    private void QueryData()
    {
        AppConfigService.IsOnProcessingRule46 = true;
        SendNotification();
        AppConfigService.IsOnProcessingRule46 = false;
    }

    public JobMessage Execute()
    {
        var result = new ExecuteResultModel();
        var jobMessage = new JobMessage();
        try
        {
            Register();
            result = SendNotification();
        }
        catch (Exception e)
        {
            result.Success = false;
            result.Message = e.Message;
            log.Log(e);
        }
        finally
        {
            AppConfigService.IsOnProcessingRule46 = false;
            jobMessage.Mail = result.Mail;
            jobMessage.JobStatus = result.Success ? JobStatus.SuccessWithoutSendmail : JobStatus.DataIsNotReady;
            jobMessage.TaskLog = new TaskLog
            {
                LogTime = DateTime.Now,
                Message = result.Message
            };
        }

        return jobMessage;
    }

    private ExecuteResultModel SendNotification()
    {
        var messageModels = refineDataService.SelectDifferentDataRule46();
        var mail = mailService.CreateMailRule46(messageModels);
        if (messageModels.Count > 0)
        {
            timer.Start();
            timer.Interval = interval;
            var success = false;
            retryCounter++;
            log.Log($"Retrying The Rule 46: {retryCounter} times at {DateTime.Now}, interval {timer.Interval} ms");
            try
            {
                if (retryCounter > MaxRetry)
                {
                    log.Log($"The Rule 46 stopped retrying and mailing to users at {DateTime.Now} because there it's exceeded retry counter.");
                    MailClientHandler.SendFunctionMail(mail);
                    retryCounter = 0;
                    timer.Stop();
                    success = true;
                }
            }
            catch (Exception ex)
            {
                success = false;
                log.Log(ex);
            }

            if (success)
            {
                refineDataService.UpdateSiteDBSystemSettingRule46();
            }
        }
        else
        {
            if (retryCounter > 0)
            {
                log.Log($"The Rule 46: {retryCounter} stopped retrying at {DateTime.Now} because data is ok now.");
            }

            refineDataService.UpdateSiteDBSystemSettingRule46();
            retryCounter = 0;
            timer.Stop();
        }

        return new ExecuteResultModel
        {
            Success = true,
            Message = string.Empty,
            Mail = mail
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        timer.Dispose();
        timer = null;
    }
}