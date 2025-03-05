using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Client;
using Fanex.MailDelivery.Core.Models;
using System;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule47Service
{
    JobMessage Execute();
}

public class MainRule47ServiceImp(
    Log log,
    RefineDataService refineDataService,
    MailService mailService)
    : MainRule47Service
{
    public JobMessage Execute()
    {
        var result = new ExecuteResultModel();
        var jobMessage = new JobMessage();
        try
        {
            if (!AppConfigService.IsOnProcessingRule47)
            {
                AppConfigService.IsOnProcessingRule47 = true;
                result = SendNotification();
                AppConfigService.IsOnProcessingRule47 = false;
            }
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
            jobMessage.JobStatus = result.Success ? JobStatus.SuccessWithoutSendmail : JobStatus.DataIsNotReady;
            jobMessage.TaskLog = new TaskLog
            {
                LogTime = DateTime.Now,
                Message = result.Message
            };

            AppConfigService.IsOnProcessingRule47 = true;
        }

        return jobMessage;
    }

    private ExecuteResultModel SendNotification()
    {
        var messageModels = refineDataService.SelectDifferentDataRule47();
        var mail = mailService.CreateMailRule47(messageModels);
        if (messageModels.Count > 0)
        {
            MailClientHandler.SendFunctionMail(mail);
        }

        return new ExecuteResultModel
        {
            Success = true,
            Message = string.Empty,
            Mail = mail
        };
    }
}