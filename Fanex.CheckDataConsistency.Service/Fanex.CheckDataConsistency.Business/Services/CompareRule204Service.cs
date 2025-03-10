using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface CompareRule204Service
{
    void CheckRule204CustInfoMini(DataConsistencyConfigModel config, ResultModel resultModel);

    void CheckRule204CustSubMini(DataConsistencyConfigModel config, ResultModel resultModel);
}

public class CompareRule204ServiceImp(DataService dataService) : CompareRule204Service
{
    public void CheckRule204CustInfoMini(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        QueryRule204CustInfoMini(config, resultModel);
        while (resultModel.DictResult.Count > 0 && resultModel.RetryCounter < 3)
        {
            resultModel.RetryCounter++;
            QueryRule204CustInfoMini(config, resultModel);
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }

        var command = new DataConsistencyConfigUpdateCommand
        {
            RuleNumber = 204,
            TableName = config.TableName,
            VValue = $"{resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}"
        };

        dataService.UpdateConfig(command);

        resultModel.RulesList.Add("204.1 - Normal Account - Check consistency with Customer VS CustInfoMini");
        resultModel.RulesList.Add($"RepUpdateTime From {config.VValue} To {resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}");
        resultModel.RulesList.Add("DBGLOB.GlobCustDB - DBA_Consistency_CustInfoMini_Checking(table CustInfoMini)");
        resultModel.RulesList.Add("DBTP.bodb02 - DBA_Consistency_CustInfoMini_Checking(table Customer)");
    }

    private void QueryRule204CustInfoMini(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        var rowCount = 1;
        var dictResult = new Dictionary<string, CustInfoTable1Model>();
        var custInfoMiniFromDBGLOB = dataService.QueryCustInfoMiniCheckingFromDBGLOB(config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        var custInfoMiniFromDBTP = dataService.QueryCustInfoFromDBTP(config.TableName, config.SpName, custInfoMiniFromDBGLOB.Item2.CustListJson);

        custInfoMiniFromDBGLOB.Item1.ForEach(row =>
        {
            var dataFromDBTP = custInfoMiniFromDBTP.FirstOrDefault(s => s.Recommend == row.Recommend && s.Mrecommend == row.Mrecommend && s.Srecommend == row.Srecommend);
            if (dataFromDBTP != null) return;
            dictResult.Add($"{rowCount}_{DB.DBGLOB}", row);
            dictResult.Add($"{rowCount}_{DB.DBTP}", null);
            rowCount++;
        });

        custInfoMiniFromDBGLOB.Item1.ForEach(row =>
        {
            var dataFromDBTP = custInfoMiniFromDBTP.FirstOrDefault(s => s.Recommend == row.Recommend && s.Mrecommend == row.Mrecommend && s.Srecommend == row.Srecommend && (s.RowCount != row.RowCount || s.CHECKSUM_Value != row.CHECKSUM_Value));
            if (dataFromDBTP == null) return;
            dictResult.Add($"{rowCount}_{DB.DBGLOB}", row);
            dictResult.Add($"{rowCount}_{DB.DBTP}", dataFromDBTP);
            rowCount++;
        });

        var smaList = dictResult.Where(s => s.Value != null)
            .Select(s => new SMAModel
            { Recommend = s.Value.Recommend, Mrecommend = s.Value.Mrecommend, Srecommend = s.Value.Srecommend })
            .Distinct().ToList();

        var smaListJson = JsonConvert.SerializeObject(smaList, Formatting.Indented);
        resultModel.DictResult = dictResult;
        resultModel.SMAListJson = smaListJson;
        resultModel.CustListJson = custInfoMiniFromDBGLOB.Item2.CustListJson;
        if (resultModel.RetryCounter == 0)
        {
            resultModel.CurrentTime = custInfoMiniFromDBGLOB.Item2.CurrentTime;
        }
    }

    public void CheckRule204CustSubMini(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        QueryRule204CustSubMini(config, resultModel);
        while (resultModel.DictResult.Count > 0 && resultModel.RetryCounter < 3)
        {
            resultModel.RetryCounter++;
            QueryRule204CustSubMini(config, resultModel);
            System.Threading.Thread.Sleep(AppConfigService.Config.SleepIntervalInSecond * 1000);
        }

        var command = new DataConsistencyConfigUpdateCommand
        {
            RuleNumber = 204,
            TableName = config.TableName,
            VValue = $"{resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}"
        };

        dataService.UpdateConfig(command);

        resultModel.RulesList.Add("204.2 - Sub Account - Check consistency with CustSub VS CustSubMini");
        resultModel.RulesList.Add($"RepUpdateTime From {config.VValue} To {resultModel.CurrentTime:yyyy-MM-dd HH:mm:ss.fff}");
        resultModel.RulesList.Add("DBGLOB.GlobCustDB - DBA_Consistency_CustSubMini_Checking(table CustSubMini)");
        resultModel.RulesList.Add("DBTP.bodb02 - DBA_Consistency_CustSubMini_Checking(table CustSub)");
    }

    private void QueryRule204CustSubMini(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        var rowCount = 1;
        var dictResult = new Dictionary<string, CustInfoTable1Model>();
        var custSubMiniFromDBGLOB = dataService.QueryCustSubMiniCheckingFromDBGLOB(config.TableName, config.SpName, config.VValue, resultModel.SMAListJson, resultModel.CustListJson);
        var custSubMiniFromDBTP = dataService.QueryCustInfoFromDBTP(config.TableName, config.SpName, custSubMiniFromDBGLOB.Item2.CustListJson);

        custSubMiniFromDBGLOB.Item1.ForEach(row =>
        {
            var dataFromDBTP = custSubMiniFromDBTP.FirstOrDefault(s => s.CustId == row.CustId);
            if (dataFromDBTP != null) return;
            dictResult.Add($"{rowCount}_{DB.DBGLOB}", row);
            dictResult.Add($"{rowCount}_{DB.DBTP}", null);
            rowCount++;
        });

        custSubMiniFromDBGLOB.Item1.ForEach(row =>
        {
            var dataFromDBTP = custSubMiniFromDBTP.FirstOrDefault(s => s.CustId == row.CustId && (s.RowCount != row.RowCount || s.CHECKSUM_Value != row.CHECKSUM_Value));
            if (dataFromDBTP == null) return;
            dictResult.Add($"{rowCount}_{DB.DBGLOB}", row);
            dictResult.Add($"{rowCount}_{DB.DBTP}", dataFromDBTP);
            rowCount++;
        });

        var custMiniList = dictResult.Where(s => s.Value != null).Select(s => s.Value.CustId).Distinct().ToList();
        resultModel.DictResult = dictResult;
        resultModel.SMAListJson = string.Join(",", custMiniList);
        resultModel.CustListJson = custSubMiniFromDBGLOB.Item2.CustListJson;
        if (resultModel.RetryCounter == 0)
        {
            resultModel.CurrentTime = custSubMiniFromDBGLOB.Item2.CurrentTime;
        }
    }
}