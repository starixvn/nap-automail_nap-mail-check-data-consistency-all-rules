using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface CompareRule207Service
{
    void CustInfoInHouseChecking(DataConsistencyConfigModel config, ResultModel resultModel);
}

public class CompareRule207ServiceImp(DataServiceRule207 dataServiceRule207, DataService dataService) : CompareRule207Service
{
    private int RowCount { get; set; }
    private string PreviousHashCustInfoFromDBCUST { get; set; }
    private string PreviousHashCustInfoFromDBCUST_DBPRO { get; set; }

    public void CustInfoInHouseChecking(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        QueryRule207CusInfoInHouse(config, resultModel);
        RetryQueryIfNeeded(config, resultModel);

        UpdateConfig(config, resultModel);
        AddRulesToResult(resultModel, config);
    }

    private void QueryRule207CusInfoInHouse(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        RowCount = 1;
        var dictResult = new Dictionary<string, CustInfoTable1Model>();
        var customerFromDBCUST = CustInfoFromDbcustAndDBTP(config, resultModel, dictResult);

        //CustInfoFromDbcustAndDBPRO(config, resultModel, dictResult);
        CustInfoFromDbcustAndReplicationDB(customerFromDBCUST.Item1, config, resultModel, dictResult);

        UpdateResultModel(resultModel, dictResult, customerFromDBCUST);
    }

    private void RetryQueryIfNeeded(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        while (resultModel.DictResult.Count > 0 && resultModel.RetryCounter < 3)
        {
            resultModel.RetryCounter++;
            resultModel.RetryCounterFromDBCUST = 0;
            resultModel.RetryCounterFromDBCUST_DBPRO = 0;
            PreviousHashCustInfoFromDBCUST = string.Empty;
            PreviousHashCustInfoFromDBCUST_DBPRO = string.Empty;
            QueryRule207CusInfoInHouse(config, resultModel);
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }
    }

    private void UpdateConfig(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        var command = new DataConsistencyConfigUpdateCommand
        {
            RuleNumber = 207,
            TableName = config.TableName,
            VValue = $"{resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}"
        };

        dataService.UpdateConfig(command);
    }

    private static void AddRulesToResult(ResultModel resultModel, DataConsistencyConfigModel config)
    {
        resultModel.RulesList.Add("207 - Check sum/count of new records of CustInfo table between DBCust and all Repl servers that apply in-house");
        resultModel.RulesList.Add($"RepUpdateTime From {config.VValue} To {resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}");
        resultModel.RulesList.Add("DBA_Consistency_Custinfo_InhouseRepl_Checking - DBCUST-CustDB,DBAGE-bodb02,DBBOF-bodb02,DBACC-bodb02,DBBAH-bodb02,DBSIG-CustDB,DBCTG-CustDB,DBWH-bodb02,DB04ADMIN1-bobd02");
        resultModel.RulesList.Add("DBA_Consistency_Custinfo_InhouseRepl_Checking_DBTP - DBTP-bodb02");
        resultModel.RulesList.Add("DBA_Consistency_CustInfo_InhouseRepl_Checking_DBPRO - DBCUST-CustDB,DBPRO-bodb02");
        if (resultModel.CustList != null)
        {
            resultModel.RulesList.Add($"CustList: {string.Join(", ", resultModel.CustList.Split(','))}");
        }
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

    private Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> CustInfoFromDbcustAndDBTP(DataConsistencyConfigModel config, ResultModel resultModel,
        IDictionary<string, CustInfoTable1Model> dictResult)
    {
        var customerFromDBCUST = CustInfoFromDbcust(config, resultModel);
        resultModel.CustListJson ??= customerFromDBCUST.Item2.CustListJson;

        var customerFromDBTP = dataServiceRule207.QueryCustInfoInHouseCheckingRule207(DB.DBTP, config.TableName, config.SpName + "_DBTP", config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        CompareAndAddToDictResult(customerFromDBCUST.Item1, customerFromDBTP.Item1, dictResult, DB.DBCUST, DB.DBTP);

        return customerFromDBCUST;
    }

    private Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> CustInfoFromDbcust(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        var custInfoFromDBCUST = dataServiceRule207.QueryCustInfoInHouseCheckingRule207(DB.DBCUST, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        var jsonCustInfoFromDBCUST = JsonConvert.SerializeObject(custInfoFromDBCUST.Item1, Formatting.Indented);
        var hashCustInfoFromDBCUST = StringHasher.HashLongStringToSmallString(jsonCustInfoFromDBCUST);

        if (string.IsNullOrEmpty(PreviousHashCustInfoFromDBCUST))
        {
            PreviousHashCustInfoFromDBCUST = hashCustInfoFromDBCUST;
        }

        if (PreviousHashCustInfoFromDBCUST == hashCustInfoFromDBCUST)
        {
            return custInfoFromDBCUST;
        }

        RetryCustInfoFromDbcust(config, resultModel, ref custInfoFromDBCUST, ref jsonCustInfoFromDBCUST, ref hashCustInfoFromDBCUST);

        return custInfoFromDBCUST;
    }

    private void RetryCustInfoFromDbcust(DataConsistencyConfigModel config, ResultModel resultModel, ref Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> custInfoFromDBCUST, ref string jsonCustInfoFromDBCUST, ref string hashCustInfoFromDBCUST)
    {
        while (resultModel.RetryCounterFromDBCUST < 3)
        {
            custInfoFromDBCUST = dataServiceRule207.QueryCustInfoInHouseCheckingRule207(DB.DBCUST, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
            jsonCustInfoFromDBCUST = JsonConvert.SerializeObject(custInfoFromDBCUST.Item1, Formatting.Indented);
            hashCustInfoFromDBCUST = StringHasher.HashLongStringToSmallString(jsonCustInfoFromDBCUST);

            if (PreviousHashCustInfoFromDBCUST == hashCustInfoFromDBCUST)
            {
                break;
            }

            PreviousHashCustInfoFromDBCUST = hashCustInfoFromDBCUST;
            resultModel.RetryCounterFromDBCUST++;
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }
    }

    private void CustInfoFromDbcustAndDBPRO(DataConsistencyConfigModel config, ResultModel resultModel,
     IDictionary<string, CustInfoTable1Model> dictResult)
    {
        var custInfoFromDBCUST = GetCustInfoFromDbcust(config, resultModel);
        resultModel.CustListJson ??= custInfoFromDBCUST.Item2.CustListJson;

        var currentRowCount = RowCount;
        var custInfoFromDBPRO = dataServiceRule207.QueryCustInfoInHouseCheckingDBPRORule207(DB.DBPRO, config.SpName + "_DBPRO", resultModel.CustListJson);

        CompareAndAddToDictResult(custInfoFromDBCUST.Item1, custInfoFromDBPRO.Item1, dictResult, DB.DBCUST, DB.DBPRO);
        UpdateCustListIfRowCountChanged(resultModel, custInfoFromDBCUST, currentRowCount);
    }

    private Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> GetCustInfoFromDbcust(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        var custInfoFromDBCUST = dataServiceRule207.QueryCustInfoInHouseCheckingDBPRORule207(DB.DBCUST, config.SpName + "_DBPRO", resultModel.CustListJson);
        var jsonCustInfoFromDBCUST = JsonConvert.SerializeObject(custInfoFromDBCUST.Item1, Formatting.Indented);
        var hashCustInfoFromDBCUST = StringHasher.HashLongStringToSmallString(jsonCustInfoFromDBCUST);

        if (string.IsNullOrEmpty(PreviousHashCustInfoFromDBCUST_DBPRO))
        {
            PreviousHashCustInfoFromDBCUST_DBPRO = hashCustInfoFromDBCUST;
        }

        if (PreviousHashCustInfoFromDBCUST_DBPRO == hashCustInfoFromDBCUST)
        {
            return custInfoFromDBCUST;
        }

        RetryCustInfoFromDbcustDBPRO(config, resultModel, ref custInfoFromDBCUST, ref jsonCustInfoFromDBCUST, ref hashCustInfoFromDBCUST);
        return custInfoFromDBCUST;
    }

    private void UpdateCustListIfRowCountChanged(ResultModel resultModel, Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> custInfoFromDBCUST, int currentRowCount)
    {
        var newRowCount = RowCount;
        if (newRowCount != currentRowCount)
        {
            resultModel.CustList = custInfoFromDBCUST.Item2.CustList;
        }
    }

    private void RetryCustInfoFromDbcustDBPRO(DataConsistencyConfigModel config, ResultModel resultModel, ref Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> custInfoFromDBCUST, ref string jsonCustInfoFromDBCUST, ref string hashCustInfoFromDBCUST)
    {
        while (resultModel.RetryCounterFromDBCUST_DBPRO < 3)
        {
            custInfoFromDBCUST = dataServiceRule207.QueryCustInfoInHouseCheckingDBPRORule207(DB.DBCUST, config.SpName + "_DBPRO", resultModel.CustListJson);
            jsonCustInfoFromDBCUST = JsonConvert.SerializeObject(custInfoFromDBCUST.Item1, Formatting.Indented);
            hashCustInfoFromDBCUST = StringHasher.HashLongStringToSmallString(jsonCustInfoFromDBCUST);

            if (PreviousHashCustInfoFromDBCUST_DBPRO == hashCustInfoFromDBCUST)
            {
                break;
            }

            PreviousHashCustInfoFromDBCUST_DBPRO = hashCustInfoFromDBCUST;
            resultModel.RetryCounterFromDBCUST_DBPRO++;
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }
    }

    private void CustInfoFromDbcustAndReplicationDB(List<CustInfoTable1Model> dbCust, DataConsistencyConfigModel config, ResultModel resultModel,
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
            var custInfoFromReplDB = dataServiceRule207.QueryCustInfoInHouseCheckingRule207(dbRepl, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
            CompareAndAddToDictResult(dbCust, custInfoFromReplDB.Item1, dictResult, DB.DBCUST, dbRepl);
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