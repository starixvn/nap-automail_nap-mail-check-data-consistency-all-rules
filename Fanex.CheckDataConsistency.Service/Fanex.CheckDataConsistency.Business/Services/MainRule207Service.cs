using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule207Service
{
    void Execute();
}

public class MainRule207ServiceImp(
    Log log,
    DataService dataService,
    CompareRule207Service rule207Service,
    MailService mailService)
    : MainRule207Service
{
    public void Execute()
    {
        try
        {
            if (AppConfigService.IsOnProcessingRule207)
            {
                return;
            }

            AppConfigService.IsOnProcessingRule207 = true;
            SendNotification();
            AppConfigService.IsOnProcessingRule207 = false;
        }
        catch (Exception e)
        {
            AppConfigService.IsOnProcessingRule207 = false;
            log.Log(e);
        }
    }

    private void SendNotification()
    {
        var rule207CustInfo = CheckRule207Customer();

        var data = new List<ResultModel>();
        if (rule207CustInfo.DictResult.Count > 0)
        {
            data.Add(rule207CustInfo);
        }

        if (data.Count <= 0)
        {
            return;
        }

        var mail = mailService.CreateMailRule207(data);
        mail.MailingListId = AppConfigService.Config.MailIDRule207;
        MailClientHandler.SendFunctionMail(mail);
    }

    private ResultModel CheckRule207Customer()
    {
        var configs = dataService.GetConfigData();

        var resultModel = new ResultModel();
        var config = configs.FirstOrDefault(s => s.GroupId == 2 && s.RuleNumber == 207 && s.TableName == "CustInfo");
        if (config != null)
        {
            rule207Service.CustInfoInHouseChecking(config, resultModel);
        }

        return resultModel;
    }
}