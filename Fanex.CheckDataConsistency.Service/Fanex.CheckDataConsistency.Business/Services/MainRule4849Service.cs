using Fanex.CheckDataConsistency.Business.Model;
using Fanex.CheckDataConsistency.Business.Services.MailStation;
using Fanex.MailDelivery.Client;
using Fanex.MailDelivery.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule4849Service
{
    JobMessage Execute();
}

public class MainRule4849ServiceImp(
    Log log,
    RefineDataService refineDataService,
    MailService mailService,
    DataServiceMailStation dataServiceMailStation)
    : MainRule4849Service, IDisposable
{
    private bool isHasData;

    private Timer timer;
    private const int MaxRetry = 4;

    ~MainRule4849ServiceImp()
    {
        Dispose();
    }

    private void Register()
    {
        const int milisecond = 1000;
        const int second = 60;
        const int minute = 5;
        timer = new Timer();
        timer.Interval = minute * second * milisecond;
        timer.Enabled = true;
        timer.AutoReset = true;
        timer.Elapsed += TimerElapsed;
    }

    private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        log.Log($"Retrying the Rule 48-49 call TimerElapsed, Retry [{AppConfigService.RetryCountRule4849}], IsOnProcessing [{AppConfigService.IsOnProcessingRule4849}] at {DateTime.Now}");
        if (AppConfigService.IsOnProcessingRule4849)
        {
            return;
        }

        log.Log($"Retrying the Rule 48-49 call TimerElapsed, Retry [{AppConfigService.RetryCountRule4849}], IsOnProcessing [{AppConfigService.IsOnProcessingRule4849}] BEGIN at {DateTime.Now}");
        QueryData();
        log.Log($"Retrying the Rule 48-49 call TimerElapsed, Retry [{AppConfigService.RetryCountRule4849}], IsOnProcessing [{AppConfigService.IsOnProcessingRule4849}] END at {DateTime.Now}");
    }

    private void QueryData()
    {
        AppConfigService.IsOnProcessingRule4849 = true;
        Execute();
        AppConfigService.IsOnProcessingRule4849 = false;
    }

    public JobMessage Execute()
    {
        var result = new ExecuteResultModel();
        var jobMessage = new JobMessage();
        try
        {
            if (!AppConfigService.CurrentTime.HasValue)
            {
                Register();
                timer.Start();
            }

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
            AppConfigService.IsOnProcessingRule4849 = false;
            JobStatus jobStatus;
            jobMessage.Mail = result.Mail;

            if (isHasData)
            {
                jobStatus = JobStatus.Running;
            }
            else
            {
                jobStatus = JobStatus.SuccessWithoutSendmail;
                StopRetry(false);
            }

            jobMessage.JobStatus = result.Success ? jobStatus : JobStatus.DataIsNotReady;
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
        var currentTime = DateTime.Now.AddMinutes(-3);
        if (!AppConfigService.CurrentTime.HasValue)
        {
            AppConfigService.CurrentTime = currentTime;
            AppConfigService.RetryCountRule4849 = 0;
        }
        else
        {
            currentTime = AppConfigService.CurrentTime.Value;
            AppConfigService.RetryCountRule4849++;
            var retryInfo = JsonConvert.SerializeObject(AppConfigService.RetryInfos, Formatting.Indented);
            log.Log($"Retrying the Rule 48-49: {AppConfigService.RetryCountRule4849} times at {DateTime.Now}, {Environment.NewLine} {retryInfo}");
        }

        var messageModels = refineDataService.SelectDifferentDataRule4849(currentTime);
        var mail = mailService.CreateMailRule4849(messageModels, currentTime);
        isHasData = messageModels.Count > 0;

        if (isHasData && AppConfigService.RetryCountRule4849 > MaxRetry)
        {
            mail.MailingListId = AppConfigService.Config.MailIDRule4849Sub;
            MailClientHandler.SendFunctionMail(mail);
        }

        if (AppConfigService.RetryCountRule4849 > MaxRetry)
        {
            StopRetry(true);
        }

        return new ExecuteResultModel
        {
            Success = true,
            Message = string.Empty,
            Mail = mail
        };
    }

    private void StopRetry(bool isExceedRetry)
    {
        log.Log(isExceedRetry
            ? $"The Rule 48-49 stopped retrying at {DateTime.Now} because there it's exceeded retry counter."
            : $"The Rule 48-49 stopped retrying at {DateTime.Now} because there is no data.");

        timer.Stop();

        var countUpdate = 0;
        while (countUpdate < 10)
        {
            dataServiceMailStation.UpdateMailStatus(JobStatus.SuccessWithoutSendmail);
            countUpdate++;
            Thread.Sleep(10000);
            GC.SuppressFinalize(this);
        }

        AppConfigService.CurrentTime = null;
        AppConfigService.RetryCountRule4849 = 0;
        AppConfigService.RetryInfos = new Dictionary<string, List<RetryInfo>>
        {
            { Rule.Rule48, [new RetryInfo()] },
            { Rule.Rule49, [new RetryInfo()] }
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        timer?.Dispose();
        timer = null;
    }
}