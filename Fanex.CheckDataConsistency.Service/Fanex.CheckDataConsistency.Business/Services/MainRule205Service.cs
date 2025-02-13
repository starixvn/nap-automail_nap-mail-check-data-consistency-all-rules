using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule205Service
{
    void Execute();
}

public class MainRule205ServiceImp(
    Log log,
    DataService dataService,
    CompareRule205Service rule205Service,
    MailService mailService)
    : MainRule205Service
{
    public void Execute()
    {
        try
        {
            if (AppConfigService.IsOnProcessingRule205)
            {
                return;
            }

            AppConfigService.IsOnProcessingRule205 = true;
            SendNotification();
            AppConfigService.IsOnProcessingRule205 = false;
        }
        catch (Exception e)
        {
            AppConfigService.IsOnProcessingRule205 = false;
            log.Log(e);
        }
    }

    private void SendNotification()
    {
        var rule205Customer = CheckRule205Customer();
        var rule205CustSub = CheckRule205CustSub();
        var data = new List<ResultModel>();
        if (rule205Customer.DictResultCustomer.Count > 0)
        {
            data.Add(rule205Customer);
        }

        if (rule205CustSub.DictResultCustomer.Count > 0)
        {
            data.Add(rule205CustSub);
        }

        if (data.Count <= 0)
        {
            return;
        }

        var mail = mailService.CreateMailRule205(data);
        mail.MailingListId = AppConfigService.Config.MailIDRule205;
        MailClientHandler.SendFunctionMail(mail);
    }

    private ResultModel CheckRule205Customer()
    {
        var configs = dataService.GetConfigData();
        var resultModel = new ResultModel();
        var config = configs.FirstOrDefault(s => s.GroupId == 2 && s.RuleNumber == 205 && s.TableName == "Customer");
        if (config != null)
        {
            rule205Service.CheckRule205Customer(config, resultModel);
        }

        return resultModel;
    }

    private ResultModel CheckRule205CustSub()
    {
        var configs = dataService.GetConfigData();
        var resultModel = new ResultModel();
        var config = configs.FirstOrDefault(s => s.GroupId == 2 && s.RuleNumber == 205 && s.TableName == "CustSub");
        if (config != null)
        {
            rule205Service.CheckRule205CustSub(config, resultModel);
        }

        return resultModel;
    }
}