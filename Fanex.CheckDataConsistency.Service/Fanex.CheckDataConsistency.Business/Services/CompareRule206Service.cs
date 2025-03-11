using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        while (resultModel.DictResult.Count > 0 && resultModel.RetryCounter < 3)
        {
            resultModel.RetryCounter++;
            resultModel.RetryCounterFromDBCUST = 0;
            QueryRule206CustomerInHouse(config, resultModel);
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }

        var command = new DataConsistencyConfigUpdateCommand
        {
            RuleNumber = 206,
            TableName = config.TableName,
            VValue = $"{resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}"
        };

        dataService.UpdateConfig(command);

        resultModel.RulesList.Add("206 - Check sum/count of new records of Customer table between DBCust and all Repl servers that apply in-house");
        resultModel.RulesList.Add($"RepUpdateTime From {config.VValue} To {resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}");
        resultModel.RulesList.Add("DBA_Consistency_Customer_InhouseRepl_Checking - DBCUST-CustDB,DBAGE-bodb02,DBBOF-bodb02,DBACC-bodb02,DBBAH-bodb02,DBSIG-CustDB,DBCTG-CustDB,DBWH-bodb02,DB04ADMIN1-bobd02");
        resultModel.RulesList.Add("DBA_Consistency_Customer_InhouseRepl_Checking_DBTP - DBTP/bodb02");
    }

    private void QueryRule206CustomerInHouse(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        RowCount = 1;
        var dictResult = new Dictionary<string, CustInfoTable1Model>();
        var customerFromDBCUST = CustomerFromDbcustAndDBTP(config, resultModel, dictResult);
        CustomerFromDbcustAndReplicationDB(customerFromDBCUST.Item1, config, resultModel, dictResult);

        var smaShortList = new List<SMAModel>();
        dictResult.Where(s => s.Value != null)
            .Select(s => new SMAModel
            {
                Recommend = s.Value.Recommend,
                Mrecommend = s.Value.Mrecommend,
                Srecommend = s.Value.Srecommend
            })
            .GroupBy(g => new { g.Recommend, g.Mrecommend, g.Srecommend })
            .ToList()
            .ForEach(sma =>
            {
                smaShortList.Add(new SMAModel { Recommend = sma.Key.Recommend, Mrecommend = sma.Key.Mrecommend, Srecommend = sma.Key.Srecommend });
            });

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

        var customerFromDBTP = dataServiceRule206.QueryCustomerInHouseCheckingRule206(DB.DBTP, config.TableName, config.SpName + "_DBTP", config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        customerFromDBCUST.Item1.ForEach(row =>
        {
            var dataFromDBTP = customerFromDBTP.Item1.FirstOrDefault(s => s.Recommend == row.Recommend
                                                                          && s.Mrecommend == row.Mrecommend
                                                                          && s.Srecommend == row.Srecommend);
            if (dataFromDBTP != null) return;
            dictResult.Add($"{RowCount}_{DB.DBCUST}", row);
            dictResult.Add($"{RowCount}_{DB.DBTP}", null);
            RowCount++;
        });

        customerFromDBCUST.Item1.ForEach(row =>
        {
            var dataFromDBTP = customerFromDBTP.Item1.FirstOrDefault(s => s.Recommend == row.Recommend
                                                                          && s.Mrecommend == row.Mrecommend
                                                                          && s.Srecommend == row.Srecommend
                                                                          && (s.RowCount != row.RowCount || s.CHECKSUM_Value != row.CHECKSUM_Value));
            if (dataFromDBTP == null) return;
            dictResult.Add($"{RowCount}_{DB.DBCUST}", row);
            dictResult.Add($"{RowCount}_{DB.DBTP}", dataFromDBTP);
            RowCount++;
        });

        return customerFromDBCUST;
    }

    private Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> CustomerFromDbcust(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        var customerFromDBCUST = dataServiceRule206.QueryCustomerInHouseCheckingRule206(DB.DBCUST, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        var jsonCustomerFromDBCUST = JsonConvert.SerializeObject(customerFromDBCUST.Item1, Formatting.Indented);
        var hashCustomerFromDBCUST = StringHasher.HashLongStringToSmallString(jsonCustomerFromDBCUST);
        if (string.IsNullOrEmpty(PreviousHashCustomerFromDBCUST))
        {
            PreviousHashCustomerFromDBCUST = hashCustomerFromDBCUST;
        }

        if (PreviousHashCustomerFromDBCUST == hashCustomerFromDBCUST)
        {
            return customerFromDBCUST;
        }

        while (resultModel.RetryCounterFromDBCUST < 3)
        {
            customerFromDBCUST = dataServiceRule206.QueryCustomerInHouseCheckingRule206(DB.DBCUST, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
            jsonCustomerFromDBCUST = JsonConvert.SerializeObject(customerFromDBCUST.Item1, Formatting.Indented);
            hashCustomerFromDBCUST = StringHasher.HashLongStringToSmallString(jsonCustomerFromDBCUST);
            if (PreviousHashCustomerFromDBCUST == hashCustomerFromDBCUST)
            {
                break;
            }

            PreviousHashCustomerFromDBCUST = hashCustomerFromDBCUST;
            resultModel.RetryCounterFromDBCUST++;
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }

        return customerFromDBCUST;
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
            dbCust.ForEach(row =>
            {
                var dataFromDBRepl = customerFromReplDB.Item1.FirstOrDefault(s => s.Recommend == row.Recommend
                                                                              && s.Mrecommend == row.Mrecommend
                                                                              && s.Srecommend == row.Srecommend);
                if (dataFromDBRepl != null) return;
                dictResult.Add($"{RowCount}_{DB.DBCUST}", row);
                dictResult.Add($"{RowCount}_{dbRepl}", null);
                RowCount++;
            });

            dbCust.ForEach(row =>
            {
                var dataFromDBRepl = customerFromReplDB.Item1.FirstOrDefault(s => s.Recommend == row.Recommend
                                                                              && s.Mrecommend == row.Mrecommend
                                                                              && s.Srecommend == row.Srecommend
                                                                              && (s.RowCount != row.RowCount || s.CHECKSUM_Value != row.CHECKSUM_Value));
                if (dataFromDBRepl == null) return;
                dictResult.Add($"{RowCount}_{DB.DBCUST}", row);
                dictResult.Add($"{RowCount}_{dbRepl}", dataFromDBRepl);
                RowCount++;
            });
        });
    }
}