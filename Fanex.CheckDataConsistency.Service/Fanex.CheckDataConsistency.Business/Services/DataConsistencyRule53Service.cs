using Fanex.CheckDataConsistency.Business.Model;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule53Service
{
    List<MessageRule53Model> QueryData();
}

public class DataConsistencyRule53ServiceImp(DataService dataService) : DataConsistencyRule53Service
{
    public List<MessageRule53Model> QueryData()
    {
        var result = new List<MessageRule53Model>();
        var dataSiteDBs = dataService.Rule53SelectSiteDBConsistencyDetectSiteDBGroupID();

        foreach (var dataSiteDB in dataSiteDBs)
        {
            if (dataSiteDB.Value.Item1.Count > 0)
            {
                var siteDB = dataSiteDB.Key.Split('_')[1];
                result.Add(new MessageRule53Model
                {
                    Rule = "#53",
                    Message = "Checking mismatch SiteDBGroupID values which retrieving from bet ticket, server configuration, and customer's site.",
                    SPName = "SiteDB_DBA_Consistency_DetectSiteDBGroupID_Job",
                    Data = new Dictionary<string, Tuple<List<SiteDBGroupIDModel>, List<Rule53FromToModel>>>
                    {
                        { siteDB, dataSiteDB.Value }
                    }
                });
            }
        }

        return result;
    }
}