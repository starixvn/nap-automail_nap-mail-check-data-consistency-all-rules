using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Core.Models;
using System;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule51Service
{
    JobMessage Execute();
}

public class MainRule51ServiceImp(
    Log log,
    RefineDataService refineDataService,
    MailService mailService)
    : MainRule51Service
{
    private bool hasData;

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
            var jobStatus = hasData ? JobStatus.Success : JobStatus.SuccessWithoutSendmail;
            jobMessage.Mail = result.Mail;
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
        var messageModels = refineDataService.SelectDifferentDataRule51();
        var mail = mailService.CreateMailRule51(messageModels);
        hasData = messageModels.Count > 0;

        return new ExecuteResultModel
        {
            Success = true,
            Message = string.Empty,
            Mail = mail
        };
    }
}