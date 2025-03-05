using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Client;
using Fanex.MailDelivery.Core.Models;
using System;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule55Service
{
    JobMessage Execute();
}

public class MainRule55ServiceImp(
    Log log,
    RefineDataService refineDataService,
    MailService mailService)
    : MainRule55Service
{
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
        var messageModels = refineDataService.SelectDataRule55();

        var mail = mailService.CreateMailRule55(messageModels);
        if (messageModels.Data.Count > 0)
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