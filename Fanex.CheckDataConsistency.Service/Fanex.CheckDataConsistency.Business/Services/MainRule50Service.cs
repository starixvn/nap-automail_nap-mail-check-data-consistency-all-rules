using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Core.Models;
using System;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule50Service
{
    JobMessage Execute(string rule);
}

public class MainRule50ServiceImp(
    Log log,
    RefineDataService refineDataService,
    MailService mailService)
    : MainRule50Service
{
    private bool isSendMail;

    public JobMessage Execute(string rule)
    {
        var result = new ExecuteResultModel();
        var jobMessage = new JobMessage();
        try
        {
            result = SendNotification(rule);
        }
        catch (Exception e)
        {
            result.Success = false;
            result.Message = e.Message;
            log.Log(e);
        }
        finally
        {
            var sucessStatus = isSendMail ? JobStatus.Success : JobStatus.SuccessWithoutSendmail;
            jobMessage.Mail = result.Mail;
            jobMessage.JobStatus = result.Success ? sucessStatus : JobStatus.DataIsNotReady;
            jobMessage.TaskLog = new TaskLog
            {
                LogTime = DateTime.Now,
                Message = result.Message
            };
        }

        return jobMessage;
    }

    private ExecuteResultModel SendNotification(string rule)
    {
        var messageModels = refineDataService.SelectDataRule50(rule);
        isSendMail = messageModels.Count > 0;
        var mail = mailService.CreateMailRule50(messageModels);

        return new ExecuteResultModel
        {
            Success = true,
            Message = string.Empty,
            Mail = mail
        };
    }
}