using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataServiceRule61
{
    Dictionary<string, string> QueryConsistencyBalanceTrigger();
}

public class DataServiceRule61Imp(IDynamicRepository repository) : DataServiceRule61
{
    private List<SiteDBModel> QuerySiteDb()
    {
        var criteria = new SiteDBConfigCriteria();
        return repository.Fetch<SiteDBModel>(criteria).Where(s => s.SiteDBGroupID > 0).ToList();
    }

    private void Db5XChecking(string db, long lastRowInserted, IDictionary<string, string> result)
    {
        var spNameDb5XChecking = $"{db}_DBA_Consistency_BalanceTrigger_Checking";
        var commandDb5XChecking = new ConsistencyBalanceTriggerCheckingCommand(spNameDb5XChecking)
        {
            CheckingTime = AppConfigService.Config.CheckingTime,
            LastRowInserted = lastRowInserted
        };
        repository.Execute(commandDb5XChecking);

        if (commandDb5XChecking.ErrCode <= 0)
        {
            return;
        }

        var sampleData = commandDb5XChecking.SampleData;
        var sp = $"EXEC DBA_Consistency_BalanceTrigger_Checking @CheckingTime={AppConfigService.Config.CheckingTime}";
        var key = $"{db}|{sampleData}|{sp}";
        if (!result.ContainsKey(key))
        {
            result.Add(key, commandDb5XChecking.ErrMsg);
        }
    }

    private void SiteDbChecking(SiteDBModel siteDb, string targetServer, long lastRowInserted, IDictionary<string, string> result)
    {
        var spNameSiteDbChecking = $"{Rule.Rule61}_{siteDb.LinkedServerName}_DBA_Consistency_BalanceTrigger_Checking";
        var commandSiteDbChecking = new ConsistencyBalanceTriggerCheckingCommand(spNameSiteDbChecking)
        {
            CheckingTime = AppConfigService.Config.CheckingTime,
            LastRowInserted = lastRowInserted,
            TargetServer = targetServer
        };

        repository.Execute(commandSiteDbChecking);
        if (commandSiteDbChecking.ErrCode <= 0)
        {
            return;
        }

        var sampleData = commandSiteDbChecking.SampleData;
        var sp = $"EXEC DBA_Consistency_BalanceTrigger_Checking @CheckingTime={AppConfigService.Config.CheckingTime}, @LastRowInserted={lastRowInserted}, @TargetServer='{targetServer}'";
        var key = $"{siteDb.LinkedServerName}|{sampleData}|{sp}";
        if (!result.ContainsKey(key))
        {
            result.Add(key, commandSiteDbChecking.ErrMsg);
        }
    }

    public Dictionary<string, string> QueryConsistencyBalanceTrigger()
    {
        var result = new Dictionary<string, string>();
        var siteDbs = QuerySiteDb();

        DB.DB05X.ForEach(db =>
        {
            siteDbs.ForEach(siteDb =>
            {
                var spNameSiteDbConfig = $"{db}_BalanceTriggerSiteDBConfig_Select";
                var commandSiteDbConfig = new BalanceTriggerSiteDBConfigCommand(spNameSiteDbConfig)
                {
                    IsTest = AppConfigService.Config.IsTest,
                    SiteDBGroupID = siteDb.SiteDBGroupID
                };
                repository.Execute(commandSiteDbConfig);
                var lastRowInserted = commandSiteDbConfig.LastRowInserted;

                Db5XChecking(db, lastRowInserted, result);
                SiteDbChecking(siteDb, db, lastRowInserted, result);
            });
        });

        return result;
    }
}