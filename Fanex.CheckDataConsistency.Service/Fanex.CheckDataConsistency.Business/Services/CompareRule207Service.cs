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

    public void CustInfoInHouseChecking(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        QueryRule207CusInfoInHouse(config, resultModel);
        while (resultModel.DictResult.Count > 0 && resultModel.RetryCounter < 3)
        {
            resultModel.RetryCounter++;
            QueryRule207CusInfoInHouse(config, resultModel);
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }

        var command = new DataConsistencyConfigUpdateCommand
        {
            RuleNumber = 207,
            TableName = config.TableName,
            VValue = $"{resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}"
        };

        dataService.UpdateConfig(command);

        resultModel.RulesList.Add("207 - Check sum/count of new records of CustInfo table between DBCust and all Repl servers that apply in-house");
        resultModel.RulesList.Add($"RepUpdateTime From {config.VValue} To {resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}");
        resultModel.RulesList.Add("DBA_Consistency_Custinfo_InhouseRepl_Checking - DBCUST-CustDB,DBAGE-bodb02,DBBOF-bodb02,DBACC-bodb02,DBBAH-bodb02,DBSIG-CustDB,DBCTG-CustDB,DBWH-bodb02,DB04ADMIN1-bobd02");
        resultModel.RulesList.Add("DBA_Consistency_Custinfo_InhouseRepl_Checking_DBTP - DBTP/bodb02");

        if (resultModel.CustListJson == null) return;
        var custListJsonBase64 = Convert.ToBase64String(resultModel.CustListJson);
        resultModel.CommandList.Add("DECLARE @CustListJson VARBINARY(MAX) = NULL;");
        resultModel.CommandList.Add($"DECLARE @CustListJsonBase64 NVARCHAR(MAX) = '{custListJsonBase64}'");
        resultModel.CommandList.Add("SET @CustListJson = CAST(N'' AS XML).value('xs:base64Binary(sql:variable(\"@CustListJsonBase64\"))', 'VARBINARY(MAX)');");
        resultModel.CommandList.Add("SELECT CustID FROM OPENJSON(CAST(DECOMPRESS(@CustListJson) AS NVARCHAR(MAX))) WITH(CustID INT);");
    }

    private void QueryRule207CusInfoInHouse(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        RowCount = 1;
        var dictResult = new Dictionary<string, CustInfoTable1Model>();
        var customerFromDBCUST = CustInfoFromDbcustAndDBTP(config, resultModel, dictResult);
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

    private Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> CustInfoFromDbcustAndDBTP(DataConsistencyConfigModel config, ResultModel resultModel,
        IDictionary<string, CustInfoTable1Model> dictResult)
    {
        var customerFromDBCUST = dataServiceRule207.QueryCustInfoInHouseCheckingRule207(DB.DBCUST, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        resultModel.CustListJson ??= customerFromDBCUST.Item2.CustListJson;

        var customerFromDBTP = dataServiceRule207.QueryCustInfoInHouseCheckingRule207(DB.DBTP, config.TableName, config.SpName + "_DBTP", config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
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
            DB.DB04ADMIN2,
          //  DB.DB02
        };

        dbReplList.ForEach(dbRepl =>
        {
            var customerFromReplDB = dataServiceRule207.QueryCustInfoInHouseCheckingRule207(dbRepl, config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
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