using Fanex.CheckDataConsistency.Business.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule47Service
{
    List<MessageRule47Model> QueryData();
}

public class DataConsistencyRule47ServiceImp(DataService dataService) : DataConsistencyRule47Service
{
    public List<MessageRule47Model> QueryData()
    {
        var result = new List<MessageRule47Model>();
        var dataSiteDBs = dataService.QueryRule47();

        foreach (var dataSiteDB in dataSiteDBs)
        {
            var dBOutputResult = dataSiteDB.Value.DBOutputResult;

            if (dBOutputResult.ErrCode > 0)
            {
                var errMsg = $"Fanex.CheckDataConsistencyRule47.Service: {dBOutputResult.ErrMsg}";
                throw new Exception(errMsg);
            }

            var siteDB = dataSiteDB.Key;
            var dBDataResult = dataSiteDB.Value.DBDataResult;

            if (dBDataResult.Count > 0)
            {
                var db = siteDB.Split('_')[1];
                var data = new Dictionary<string, ConsistencyRule47Model>();
                var index = 0;

                dBDataResult.ForEach(row =>
                {
                    index++;
                    data.Add($"{index}-{db}", row);
                });

                var message = new MessageRule47Model
                {
                    Rule = "#47",
                    Message = "Check if transfer tickets are inserted into the correct SiteDBGroupID by reverting the transID",
                    SPName = $"{dataSiteDB.Value.SPNameOutputResult} " +
                             $"{dataSiteDB.Value.SPNameDataResult} " +
                             $"{JsonConvert.SerializeObject(dataSiteDB.Value.DBOutputResult, Formatting.Indented)}",
                    DB = DB.DBCONSISTENCY,
                    Data = data
                };

                result.Add(message);
            }
        }

        return result;
    }
}