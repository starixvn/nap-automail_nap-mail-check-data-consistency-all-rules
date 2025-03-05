using Fanex.CheckDataConsistency.Business.Model;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule61Service
{
    List<MessageRule61Model> QueryData();
}

public class DataConsistencyRule61ServiceImp(DataServiceRule61 dataService) : DataConsistencyRule61Service
{
    public List<MessageRule61Model> QueryData()
    {
        var timeJobExecute = DateTime.Now;
        var result = new List<MessageRule61Model>();
        var data = dataService.QueryConsistencyBalanceTrigger();
        var model = new MessageRule61Model
        {
            Rule = "#61",
            Message = $"SiteDBX and DB055/DB055BK: {Environment.NewLine} " +
                      $"Check DATEDIFF(SECOND,ActionDate in BalanceTrigger, @currentTime ) >={AppConfigService.Config.CheckingTime} {Environment.NewLine} {Environment.NewLine}" +
                      $"+ BalanceTrigger at SiteDBx pending means Synchronize from SiteDBx to DBBAH has problem" +
                      $"+ BalanceTrigger at DBBAH pending means data from BalanceTrigger to BalanceRecord has problem",
            SpName = $"Exec DBA_Consistency_BalanceTrigger_Checking @CheckingTime={AppConfigService.Config.CheckingTime}",
            TimeJobExecute = timeJobExecute,
            Data = data
        };

        if (model.Data.Count > 0)
        {
            result.Add(model);
        }

        return result;
    }
}