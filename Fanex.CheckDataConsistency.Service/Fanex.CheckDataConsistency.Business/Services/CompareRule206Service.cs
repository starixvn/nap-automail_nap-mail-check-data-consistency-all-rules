using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface CompareRule206Service
{
    void CustomerInHouseChecking(DataConsistencyConfigModel config, ResultModel resultModel);
}

public class CompareRule206ServiceImp(DataServiceRule206 dataServiceRule206, DataService dataService) : CompareRule206Service
{
    private int RowCount { get; set; }
    private string PreviousHashCustomerFromDBCUST { get; set; }

    public void CustomerInHouseChecking(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        QueryRule206CustomerInHouse(config, resultModel);
        RetryQueryIfNeeded(config, resultModel);

        UpdateConfig(config, resultModel);
        AddRulesToResult(resultModel, config);
    }

    private void QueryRule206CustomerInHouse(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        RowCount = 1;
        var dictResult = new Dictionary<string, CustInfoTable1Model>();
        var customerFromDBCUST = CustomerFromDbcustAndDBTP(config, resultModel, dictResult);

        if (resultModel.RetryCounterFromDBCUST < 3)
        {
            CustomerFromDbcustAndReplicationDB(customerFromDBCUST.Item1, config, resultModel, dictResult);
        }

        UpdateResultModel(resultModel, dictResult, customerFromDBCUST);
    }

    private void RetryQueryIfNeeded(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        while (resultModel.DictResult.Count > 0 && resultModel.RetryCounter < 3)
        {
            resultModel.RetryCounter++;
            resultModel.RetryCounterFromDBCUST = 0;
            QueryRule206CustomerInHouse(config, resultModel);
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }
    }

    private void UpdateConfig(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        var command = new DataConsistencyConfigUpdateCommand
        {
            RuleNumber = 206,
            TableName = config.TableName,
            VValue = $"{resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}"
        };

        dataService.UpdateConfig(command);
    }

    private static void AddRulesToResult(ResultModel resultModel, DataConsistencyConfigModel config)
    {
        resultModel.RulesList.Add("206 - Check sum/count of new records of Customer table between DBCust and all Repl servers that apply in-house");
        resultModel.RulesList.Add($"RepUpdateTime From {config.VValue} To {resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}");
        resultModel.RulesList.Add("DBA_Consistency_Customer_InhouseRepl_Checking - DBCUST-CustDB,DBAGE-bodb02,DBBOF-bodb02,DBACC-bodb02,DBBAH-bodb02,DBSIG-CustDB,DBCTG-CustDB,DBWH-bodb02,DB04ADMIN1-bobd02");
        resultModel.RulesList.Add("DBA_Consistency_Customer_InhouseRepl_Checking_DBTP - DBTP/bodb02");
    }

    private static void UpdateResultModel(ResultModel resultModel, Dictionary<string, CustInfoTable1Model> dictResult, Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> customerFromDBCUST)
    {
        var smaShortList = dictResult.Where(s => s.Value != null)
            .Select(s => new SMAModel
            {
                Recommend = s.Value.Recommend,
                Mrecommend = s.Value.Mrecommend,
                Srecommend = s.Value.Srecommend
            })
            .GroupBy(g => new { g.Recommend, g.Mrecommend, g.Srecommend })
            .Select(sma => new SMAModel { Recommend = sma.Key.Recommend, Mrecommend = sma.Key.Mrecommend, Srecommend = sma.Key.Srecommend })
            .ToList();

        var smaListJson = JsonConvert.SerializeObject(smaShortList, Formatting.Indented);
        resultModel.DictResult = dictResult;
        resultModel.SMAListJson = smaListJson;

        if (resultModel.RetryCounter == 0)
        {
            resultModel.CurrentTime = customerFromDBCUST.Item2!.CurrentTime;
        }
    }

    private Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> CustomerFromDbcustAndDBTP(DataConsistencyConfigModel config, ResultModel resultModel,
        IDictionary<string, CustInfoTable1Model> dictResult)
    {
        var customerFromDBCUST = CustomerFromDbcust(config, resultModel);
        resultModel.CustListJson ??= customerFromDBCUST.Item2.CustListJson;
        if (resultModel.RetryCounterFromDBCUST >= 3)
        {
            var json = JsonConvert.SerializeObject(customerFromDBCUST, Formatting.Indented);
            var date = $"{DateTime.Today:yyyyMMdd}-{DateTime.Now.Ticks}-Retry-{resultModel.RetryCounterFromDBCUST}";
            var path = $"{Directory.CreateDirectory(AppConfigService.Config.Path)}/{DateTime.Now:yyyyMMdd}";
            Directory.CreateDirectory(path);
            File.WriteAllText($"{path}/Rule206-{date}-CustomerFromDbcust.json", json);
            return customerFromDBCUST;
        }

        var customerFromDBTP = dataServiceRule206.QueryCustomerInHouseCheckingRule206(DB.DBTP, config.TableName, config.SpName + "_DBTP", config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        CompareAndAddToDictResult(customerFromDBCUST.Item1, customerFromDBTP.Item1, dictResult, DB.DBCUST, DB.DBTP);

        return customerFromDBCUST;
    }

    private Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> CustomerFromDbcust(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        var customerFromDBCUST = dataServiceRule206.QueryCustomerInHouseCheckingRule206(DB.DBCUST, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        var customerFromDBCUST_CHECKSUM = customerFromDBCUST.Item1.Select(s => new CustInfoTable1Model { TableName = s.TableName, Recommend = s.Recommend, Mrecommend = s.Mrecommend, Srecommend = s.Srecommend, CustId = s.CustId, CHECKSUM_Value = s.CHECKSUM_Value });
        var jsonCustomerFromDBCUST = JsonConvert.SerializeObject(customerFromDBCUST_CHECKSUM, Formatting.Indented);
        var hashCustomerFromDBCUST = StringHasher.HashLongStringToSmallString(jsonCustomerFromDBCUST);

        if (string.IsNullOrEmpty(PreviousHashCustomerFromDBCUST))
        {
            PreviousHashCustomerFromDBCUST = hashCustomerFromDBCUST;
        }

        if (PreviousHashCustomerFromDBCUST == hashCustomerFromDBCUST)
        {
            return customerFromDBCUST;
        }

        RetryCustomerFromDbcust(config, resultModel, ref customerFromDBCUST, ref jsonCustomerFromDBCUST, ref hashCustomerFromDBCUST);

        return customerFromDBCUST;
    }

    private void RetryCustomerFromDbcust(DataConsistencyConfigModel config, ResultModel resultModel, ref Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> customerFromDBCUST, ref string jsonCustomerFromDBCUST, ref string hashCustomerFromDBCUST)
    {
        while (resultModel.RetryCounterFromDBCUST < 3)
        {
            customerFromDBCUST = dataServiceRule206.QueryCustomerInHouseCheckingRule206(DB.DBCUST, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
            var customerFromDBCUST_CHECKSUM = customerFromDBCUST.Item1.Select(s => new CustInfoTable1Model { TableName = s.TableName, Recommend = s.Recommend, Mrecommend = s.Mrecommend, Srecommend = s.Srecommend, CustId = s.CustId, CHECKSUM_Value = s.CHECKSUM_Value });
            jsonCustomerFromDBCUST = JsonConvert.SerializeObject(customerFromDBCUST_CHECKSUM, Formatting.Indented);
            hashCustomerFromDBCUST = StringHasher.HashLongStringToSmallString(jsonCustomerFromDBCUST);

            if (PreviousHashCustomerFromDBCUST == hashCustomerFromDBCUST)
            {
                break;
            }

            PreviousHashCustomerFromDBCUST = hashCustomerFromDBCUST;
            resultModel.RetryCounterFromDBCUST++;
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }
    }

    private void CustomerFromDbcustAndReplicationDB(List<CustInfoTable1Model> dbCust, DataConsistencyConfigModel config, ResultModel resultModel,
        IDictionary<string, CustInfoTable1Model> dictResult)
    {
        var dbReplList = new List<string>
        {
            DB.DB052,
            DB.DB053,
            DB.DB054,
            DB.DB055,
            DB.DB061,
            DB.DB062,
            DB.DB05,
            DB.DB05BK1,
            DB.DBGRP01L,
            DB.DBGRP02L,
            DB.DBGRP03L,
            DB.DBTRANS,
            DB.DBDW,
            DB.DBDW2,
            DB.DB04ADMIN1,
            DB.DB04ADMIN2
        };

        dbReplList.ForEach(dbRepl =>
        {
            var customerFromReplDB = dataServiceRule206.QueryCustomerInHouseCheckingRule206(dbRepl, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
            CompareAndAddToDictResult(dbCust, customerFromReplDB.Item1, dictResult, DB.DBCUST, dbRepl);
        });
    }

    private void CompareAndAddToDictResult(List<CustInfoTable1Model> sourceList, List<CustInfoTable1Model> targetList, IDictionary<string, CustInfoTable1Model> dictResult, string sourceDB, string targetDB)
    {
        sourceList.ForEach(row =>
        {
            var dataFromTargetDB = targetList.FirstOrDefault(s => s.Recommend == row.Recommend
                                                                  && s.Mrecommend == row.Mrecommend
                                                                  && s.Srecommend == row.Srecommend);
            if (dataFromTargetDB != null) return;
            dictResult.Add($"{RowCount}_{sourceDB}", row);
            dictResult.Add($"{RowCount}_{targetDB}", null);
            RowCount++;
        });

        sourceList.ForEach(row =>
        {
            var dataFromTargetDB = targetList.FirstOrDefault(s => s.Recommend == row.Recommend
                                                                  && s.Mrecommend == row.Mrecommend
                                                                  && s.Srecommend == row.Srecommend
                                                                  && (s.RowCount != row.RowCount || s.CHECKSUM_Value != row.CHECKSUM_Value));
            if (dataFromTargetDB == null) return;
            dictResult.Add($"{RowCount}_{sourceDB}", row);
            dictResult.Add($"{RowCount}_{targetDB}", dataFromTargetDB);
            RowCount++;
        });
    }
}