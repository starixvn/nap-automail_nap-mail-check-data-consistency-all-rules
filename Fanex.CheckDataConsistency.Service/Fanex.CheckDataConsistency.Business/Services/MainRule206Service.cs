using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule206Service
{
    void Execute();
}

public class MainRule206ServiceImp(
    Log log,
    DataService dataService,
    CompareRule206Service rule206Service,
    MailService mailService)
    : MainRule206Service
{
    public void Execute()
    {
        try
        {
            if (AppConfigService.IsOnProcessingRule206)
            {
                return;
            }

            AppConfigService.IsOnProcessingRule206 = true;
            SendNotification();
            AppConfigService.IsOnProcessingRule206 = false;
        }
        catch (Exception e)
        {
            AppConfigService.IsOnProcessingRule206 = false;
            log.Log(e);
        }
    }

    private void SendNotification()
    {
        var rule206Customer = CheckRule206Customer();

        var data = new List<ResultModel>();
        if (rule206Customer.DictResult.Count > 0)
        {
            data.Add(rule206Customer);
        }

        if (data.Count <= 0)
        {
            return;
        }

        var mail = mailService.CreateMailRule206(data);
        mail.MailingListId = AppConfigService.Config.MailIDRule206;
        MailClientHandler.SendFunctionMail(mail);
    }

    private ResultModel CheckRule206Customer()
    {
        var configs = dataService.GetConfigData();
        var resultModel = new ResultModel();
        var config = configs.FirstOrDefault(s => s.GroupId == 2 && s.RuleNumber == 206 && s.TableName == "Customer");
        if (config != null)
        {
            rule206Service.CustomerInHouseChecking(config, resultModel);
        }

        return resultModel;
    }
}