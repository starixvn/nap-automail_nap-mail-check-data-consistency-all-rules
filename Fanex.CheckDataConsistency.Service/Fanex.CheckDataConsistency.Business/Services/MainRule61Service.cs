using Fanex.MailDelivery.Client;
using System;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MainRule61Service
{
    void Execute();
}

public class MainRule61ServiceImp(
    Log log,
    MailService mailService,
    RefineDataService refineDataService)
    : MainRule61Service
{
    public void Execute()
    {
        try
        {
            if (AppConfigService.IsOnProcessingRule61)
            {
                return;
            }

            AppConfigService.IsOnProcessingRule61 = true;
            SendNotification();
            AppConfigService.IsOnProcessingRule61 = false;
        }
        catch (Exception e)
        {
            AppConfigService.IsOnProcessingRule61 = false;

            log.Log(e);
        }
    }

    private void SendNotification()
    {
        var messageModels = refineDataService.SelectDifferentDataRule61();
        var mail = mailService.CreateMailRule61(messageModels);
        if (messageModels.Count > 0)
        {
            MailClientHandler.SendFunctionMail(mail);
        }
    }
}