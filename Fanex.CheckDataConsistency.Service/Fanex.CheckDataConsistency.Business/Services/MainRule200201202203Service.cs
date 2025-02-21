using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Core.Models;
using System;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule200201202203Service
{
    JobMessage Execute();
}

public class MainRule200201202203ServiceImp(
    Log log,
    DataServiceRule200201202203 dataService,
    MailService mailService,
    DataConsistencyRule200201202203Service checkSettingCountService)
    : MainRule200201202203Service
{
    private JobStatus jobStatus;

    public JobMessage Execute()
    {
        var result = new ExecuteResultModel();
        var jobMessage = new JobMessage();
        try
        {
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
            jobMessage.Mail = result.Mail;
            jobMessage.JobStatus = jobStatus;
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
        var configs = dataService.GetConfigData();
        var data = checkSettingCountService.CheckSettingCount(configs);
        if (data.Count > 0)
        {
            if (AppConfigService.RetryCountRule200201202203 >= 3)
            {
                AppConfigService.RetryCountRule200201202203 = 0;
                AppConfigService.TimeModel = new TimeModel();

                jobStatus = JobStatus.Success;
            }
            else
            {
                AppConfigService.RetryCountRule200201202203++;
                jobStatus = JobStatus.DataIsNotReady;
            }
        }
        else
        {
            AppConfigService.RetryCountRule200201202203 = 0;
            jobStatus = JobStatus.SuccessWithoutSendmail;
        }

        var mail = mailService.CreateMailRule200201202203(data);

        return new ExecuteResultModel
        {
            Mail = mail,
            Message = string.Empty,
            Success = true
        };
    }
}