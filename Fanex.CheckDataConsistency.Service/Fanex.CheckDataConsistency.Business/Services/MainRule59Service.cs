using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Core.Models;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule59Service
{
    JobMessage Execute();
}

public class MainRule59ServiceImp(
    Log log,
    MailService mailService,
    DataServiceRule59 dataService)
    : MainRule59Service
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
        var transactionIds = GetTransactionIds();
        var email = mailService.CreateMailRule59(transactionIds);
        return new ExecuteResultModel
        {
            Mail = email,
            Message = string.Empty,
            Success = true
        };
    }

    private List<string> GetTransactionIds()
    {
        var transactionIds = dataService.GetTransIdStep1();
        if (transactionIds.Count == 0)
        {
            jobStatus = JobStatus.SuccessWithoutSendmail;
            return [];
        }

        var result = dataService.CheckingWithTransIdStep2(string.Join(",", transactionIds));
        if (result.Count == 0)
        {
            jobStatus = JobStatus.SuccessWithoutSendmail;
            return [];
        }

        jobStatus = JobStatus.Success;
        return result;
    }
}