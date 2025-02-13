using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule204Service
{
    void Execute();
}

public class MainRule204ServiceImp(
    Log log,
    DataService dataService,
    CompareRule204Service rule204Service,
    MailService mailService)
    : MainRule204Service
{
    public void Execute()
    {
        try
        {
            if (AppConfigService.IsOnProcessingRule204)
            {
                return;
            }

            AppConfigService.IsOnProcessingRule204 = true;
            SendNotification();
            AppConfigService.IsOnProcessingRule204 = false;
        }
        catch (Exception e)
        {
            AppConfigService.IsOnProcessingRule204 = true;
            log.Log(e);
        }
    }

    private void SendNotification()
    {
        var rule204CustInfoMini = CheckRule204CustInfoMini();
        var rule204CustSubMini = CheckRule204CustSubMini();
        var data = new List<ResultModel>();
        if (rule204CustInfoMini.DictResult.Count > 0)
        {
            data.Add(rule204CustInfoMini);
        }

        if (rule204CustSubMini.DictResult.Count > 0)
        {
            data.Add(rule204CustSubMini);
        }

        if (data.Count <= 0)
        {
            return;
        }

        var mail = mailService.CreateMailRule204(data);
        mail.MailingListId = AppConfigService.Config.MailIDRule204;
        MailClientHandler.SendFunctionMail(mail);
    }

    private ResultModel CheckRule204CustInfoMini()
    {
        var configs = dataService.GetConfigData();
        var resultModel = new ResultModel();
        var config = configs.FirstOrDefault(s => s.GroupId == 2 && s.RuleNumber == 204 && s.TableName == "CustInfoMini");
        if (config != null)
        {
            rule204Service.CheckRule204CustInfoMini(config, resultModel);
        }

        return resultModel;
    }

    private ResultModel CheckRule204CustSubMini()
    {
        var configs = dataService.GetConfigData();
        var resultModel = new ResultModel();
        var config = configs.FirstOrDefault(s => s.GroupId == 2 && s.RuleNumber == 204 && s.TableName == "CustSubMini");
        if (config != null)
        {
            rule204Service.CheckRule204CustSubMini(config, resultModel);
        }

        return resultModel;
    }
}