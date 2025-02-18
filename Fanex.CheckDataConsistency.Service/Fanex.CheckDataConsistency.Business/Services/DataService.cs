using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Fanex.Data;
using Fanex.Data.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataService
{
    List<SiteDBModel> QuerySiteDB(string siteDB);

    Dictionary<string, List<ConsistencyCheckModel>> Rule48SelectSiteDBConsistencyCheckTicketCount(DateTime currentTime);

    Dictionary<string, List<ConsistencyCheckModel>> Rule48SelectDB05XDB06XConsistencyCheckTicketCount(DateTime currentTime);

    Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>> Rule48SelectConsistencyCheckTicketCount(DateTime currentTime);

    Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>> Rule49SelectConsistencyCheckSumTicket(DateTime currentTime);

    Dictionary<string, List<ConsistencyCheckModel>> Rule49SelectSiteDBConsistencyCheckSumTicket(DateTime currentTime);

    Dictionary<string, List<ConsistencyCheckModel>> Rule49SelectDB05XDB06XConsistencyCheckSumTicket(DateTime currentTime);

    List<TicketCheckingModel> SelectConsistencyCheckDuplicatedBalanceUpTicket();

    Dictionary<string, List<TicketCheckingModel>> SelectSiteDBConsistencyCheckDuplicatedBalanceUpTicket();

    Dictionary<string, List<TicketCheckingModel>> SelectDB05XDB06XConsistencyCheckDuplicatedBalanceUpTicket();

    List<TicketCheckingModel> SelectConsistencyCheckDuplicatedMoveBetTicket();

    Dictionary<string, List<TicketCheckingModel>> SelectSiteDBConsistencyCheckDuplicatedMoveBetTicket();

    Dictionary<string, List<TicketCheckingModel>> SelectDB05XDB06XConsistencyCheckDuplicatedMoveBetTicket();

    Dictionary<string, Tuple<List<SiteDBGroupIDModel>, List<Rule53FromToModel>>> Rule53SelectSiteDBConsistencyDetectSiteDBGroupID();

    List<MBUTicketCountModel> SelectConsistencyCheckMBUTicketCount(int bettype);

    Dictionary<string, List<MBUTicketCountModel>> SelectSiteDBConsistencyCheckMBUTicketCount(int bettype);

    Dictionary<string, List<MBUTicketCountModel>> SelectDB05XDB06XConsistencyCheckMBUTicketCount(int bettype);

    Dictionary<string, Rule46DTO> SelectSiteDBConsistencyCheckTicketCount();

    List<ConsistencyCheckModelRule46> SelectConsistencyCheckTicketCount(ExecuteSPResultRule46 siteDBOutputResult);

    List<ConsistencyCheckModelRule46> SelectDBXCheckTicketCount(string dbX, ExecuteSPResultRule46 siteDBOutputResult);

    Result UpdateSiteDBSystemSettingRule46(string siteDB, ExecuteSPResultRule46 siteDBOutputResult);

    Dictionary<string, Rule47DTO> QueryRule47();

    List<ConsistencyRule51Model> QueryRule51(Result result);

    Dictionary<string, List<ConsistencyRule52Model>> QueryRule52();

    List<DataConsistencyConfigModel> GetConfigData();

    Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> QueryCustInfoMiniCheckingFromDBGLOB(string tableName, string spName, string vvalue, string smaListJson, byte[] custlistJson);

    Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> QueryCustSubMiniCheckingFromDBGLOB(string tableName, string spName, string vvalue, string custMiniList, byte[] custlistJson);

    List<CustInfoTable1Model> QueryCustInfoFromDBTP(string tableName, string spName, byte[] custlistJson);

    void UpdateConfig(DataConsistencyConfigUpdateCommand command);

    CustomerModel QueryCustomerFromDBTP(string tableName, string spName, string vvalue);

    CustomerModel QueryCustomerFromDBGLOB(string tableName, string spName, string vvalue, int maxCustNew);
}

public class DataServiceImp(IDynamicRepository repository, Log log) : DataService
{
    public List<SiteDBModel> QuerySiteDB(string siteDB)
    {
        var criteria = new SiteDBConfigCriteria();
        if (string.IsNullOrEmpty(siteDB))
        {
            return repository.Fetch<SiteDBModel>(criteria).Where(s => s.SiteDBGroupID > 0).ToList();
        }

        return repository.Fetch<SiteDBModel>(criteria).Where(s => s.SiteDBGroupID > 0 && s.LinkedServerName.Equals(siteDB)).ToList();
    }

    public Dictionary<string, List<ConsistencyCheckModel>> Rule48SelectSiteDBConsistencyCheckTicketCount(DateTime currentTime)
    {
        var dict = new Dictionary<string, List<ConsistencyCheckModel>>();
        var reTryInfos = AppConfigService.RetryInfos[Rule.Rule48];
        string siteDB;
        string recommendList;
        string tableName;
        var count = 1;

        foreach (var reTryInfo in reTryInfos)
        {
            siteDB = reTryInfo.SiteDB;
            recommendList = reTryInfo.RecommendList;
            tableName = reTryInfo.TableName;
            var siteDbs = QuerySiteDB(siteDB);

            siteDbs.ForEach(siteDb =>
            {
                var key = $"{Rule.Rule48}_{siteDb.LinkedServerName}";
                var spName = $"{key}_SiteDB_DBA_Consistency_CheckTicketCount";
                var criteria = new Rule4849QueryDataCriteria(spName) { CurrentTime = currentTime, SMAListJson = recommendList, TableName = tableName };
                var timeStart = DateTime.Now;
                var data = repository.Fetch<ConsistencyCheckModel>(criteria).ToList();
                var timeEnd = DateTime.Now;
                var duration = timeEnd - timeStart;

                dict.Add($"{key}_{count}", data);
                count++;

                log.Log($"the Rule 48-49: Retry [{AppConfigService.RetryCount}] {spName} Duration:{duration:c} {JsonConvert.SerializeObject(criteria, Formatting.Indented)} - [{data.Count}]");
            });
        }

        return dict;
    }

    public Dictionary<string, List<ConsistencyCheckModel>> Rule48SelectDB05XDB06XConsistencyCheckTicketCount(DateTime currentTime)
    {
        var dict = new Dictionary<string, List<ConsistencyCheckModel>>();
        var reTryInfos = AppConfigService.RetryInfos[Rule.Rule48];
        string recommendList;
        string tableName;
        var count = 1;

        var retryInfoFinal = new List<RetryInfo>();
        var distinctRetry = reTryInfos.GroupBy(g => new { g.TableName });
        foreach (var item in distinctRetry)
        {
            var recommends = string.Join(",", item.Select(s => s.RecommendList).Distinct());

            retryInfoFinal.Add(new RetryInfo { TableName = item.Key.TableName, RecommendList = recommends });
        }

        var dbs = DB.DB05X06X;
        dbs.ForEach(db =>
        {
            var data = new List<ConsistencyCheckModel>();
            foreach (var reTryInfo in retryInfoFinal)
            {
                recommendList = reTryInfo.RecommendList;
                tableName = reTryInfo.TableName;
                var spName = $"{db}_DBA_Consistency_CheckTicketCount";
                var criteria = new Rule4849QueryDataCriteria(spName) { CurrentTime = currentTime, SMAListJson = recommendList, TableName = tableName };
                var timeStart = DateTime.Now;
                var dataDbX = repository.Fetch<ConsistencyCheckModel>(criteria).ToList();
                var timeEnd = DateTime.Now;
                var duration = timeEnd - timeStart;
                data.AddRange(dataDbX);

                log.Log($"the Rule 48-49: Retry [{AppConfigService.RetryCount}] {spName} Duration:{duration:c} {JsonConvert.SerializeObject(criteria, Formatting.Indented)} - [{data.Count}]");
            }

            dict.Add($"{db}_{count}", data);
            count++;
        });

        return dict;
    }

    public Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>> Rule48SelectConsistencyCheckTicketCount(DateTime currentTime)
    {
        var data1 = new List<ConsistencyCheckModel>();
        var data2 = new List<Rule4849FromToModel>();
        const string spName = "DBA_Consistency_CheckTicketCount";

        var retryInfos = AppConfigService.RetryInfos[Rule.Rule48];
        foreach (var retryInfo in retryInfos)
        {
            var tableName = retryInfo.TableName;
            var recommendList = retryInfo.RecommendList;
            var criteria = new Rule4849QueryDataCriteria(spName) { CurrentTime = currentTime, SMAListJson = recommendList, TableName = tableName };
            var startTime = DateTime.Now;
            var result = repository.FetchMultiple<ConsistencyCheckModel, Rule4849FromToModel>(criteria);
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            log.Log($"the Rule 48-49: Retry [{AppConfigService.RetryCount}] {spName} Duration: {duration:c} {JsonConvert.SerializeObject(criteria, Formatting.Indented)} - [{result.FirstResultSet.ToList().Count}]");
            data1.AddRange(result.Item1.ToList());
            data2.AddRange(result.Item2.ToList());
        }

        return new Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>>(data1, data2);
    }

    public Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>> Rule49SelectConsistencyCheckSumTicket(DateTime currentTime)
    {
        var data1 = new List<ConsistencyCheckModel>();
        var data2 = new List<Rule4849FromToModel>();
        const string spName = "DBA_Consistency_CheckSumTicket";

        var retryInfos = AppConfigService.RetryInfos[Rule.Rule49];

        foreach (var retryInfo in retryInfos)
        {
            var recommendList = retryInfo.RecommendList;
            var tableName = retryInfo.TableName;

            var criteria = new Rule4849QueryDataCriteria(spName) { CurrentTime = currentTime, SMAListJson = recommendList, TableName = tableName };
            var startTime = DateTime.Now;
            var result = repository.FetchMultiple<ConsistencyCheckModel, Rule4849FromToModel>(criteria);
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            log.Log($"the Rule 48-49: Retry [{AppConfigService.RetryCount}] {spName} Duration: {duration:c} {JsonConvert.SerializeObject(criteria, Formatting.Indented)} - [{result.FirstResultSet.ToList().Count}]");

            result.FirstResultSet.ToList().ForEach(row =>
            {
                row.DisplayColumnName = "TotalCheckSumAgg";
                row.DisplayColumnValue = row.TotalCheckSumAgg;
            });

            data1.AddRange(result.Item1.ToList());
            data2.AddRange(result.Item2.ToList());
        }

        return new Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>>(data1, data2);
    }

    public Dictionary<string, List<ConsistencyCheckModel>> Rule49SelectSiteDBConsistencyCheckSumTicket(DateTime currentTime)
    {
        string recommendList;
        string tableName;
        string siteDB;

        var dict = new Dictionary<string, List<ConsistencyCheckModel>>();
        var retryInfos = AppConfigService.RetryInfos[Rule.Rule49];
        var count = 1;

        foreach (var retryInfo in retryInfos)
        {
            siteDB = retryInfo.SiteDB;
            tableName = retryInfo.TableName;
            recommendList = retryInfo.RecommendList;
            var siteDbs = QuerySiteDB(siteDB);

            siteDbs.ForEach(siteDb =>
            {
                var key = $"{Rule.Rule49}_{siteDb.LinkedServerName}";
                var spName = $"{key}_SiteDB_DBA_Consistency_CheckSumTicket";
                var criteria = new Rule4849QueryDataCriteria(spName) { CurrentTime = currentTime, SMAListJson = recommendList, TableName = tableName };
                var data = repository.Fetch<ConsistencyCheckModel>(criteria).ToList();
                data.ForEach(row =>
                {
                    row.DisplayColumnName = "TotalCheckSumAgg";
                    row.DisplayColumnValue = row.TotalCheckSumAgg;
                });

                dict.Add($"{key}_{count}", data);
                count++;
            });
        }

        return dict;
    }

    public Dictionary<string, List<ConsistencyCheckModel>> Rule49SelectDB05XDB06XConsistencyCheckSumTicket(DateTime currentTime)
    {
        string recommendList;
        string tableName;

        var dict = new Dictionary<string, List<ConsistencyCheckModel>>();
        var retryInfos = AppConfigService.RetryInfos[Rule.Rule49];
        var count = 1;

        var retryInfoFinal = new List<RetryInfo>();
        var distinctRetry = retryInfos.GroupBy(g => new { g.TableName });
        foreach (var item in distinctRetry)
        {
            var recommends = string.Join(",", item.Select(s => s.RecommendList).Distinct());
            retryInfoFinal.Add(new RetryInfo { TableName = item.Key.TableName, RecommendList = recommends });
        }

        var dbs = DB.DB05X06X;
        dbs.ForEach(db =>
        {
            var data = new List<ConsistencyCheckModel>();
            foreach (var retryInfo in retryInfoFinal)
            {
                tableName = retryInfo.TableName;
                recommendList = retryInfo.RecommendList;
                var spName = $"{db}_DBA_Consistency_CheckSumTicket";
                var startTime = DateTime.Now;
                var criteria = new Rule4849QueryDataCriteria(spName) { CurrentTime = currentTime, SMAListJson = recommendList, TableName = tableName };
                var dataDbX = repository.Fetch<ConsistencyCheckModel>(criteria).ToList();
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                data.AddRange(dataDbX);

                log.Log($"the Rule 48-49: Retry [{AppConfigService.RetryCount}] {spName} Duration: {duration:c} {JsonConvert.SerializeObject(criteria, Formatting.Indented)} - [{data.Count}]");
            }

            dict.Add($"{db}_{count}", data);
            count++;
        });

        return dict;
    }

    public List<TicketCheckingModel> SelectConsistencyCheckDuplicatedBalanceUpTicket()
    {
        const string spName = "DBA_Consistency_CheckDuplicatedBalanceUpTicket";
        var criteria = new CriteriaData(spName);
        var data = repository.Fetch<TicketCheckingModel>(criteria).ToList();
        var result = new List<TicketCheckingModel>();
        data.ForEach(row =>
        {
            criteria = new CriteriaData(spName) { CustId = row.CustId, WinlostDate = row.WinlostDate, Retry = 1 };
            var dataByCustId = repository.Fetch<TicketCheckingModel>(criteria).ToList();
            result.AddRange(dataByCustId);
        });

        return result;
    }

    public Dictionary<string, List<TicketCheckingModel>> SelectSiteDBConsistencyCheckDuplicatedBalanceUpTicket()
    {
        var siteDbs = QuerySiteDB(string.Empty);
        var dict = new Dictionary<string, List<TicketCheckingModel>>();
        siteDbs.ForEach(siteDb =>
        {
            var key = $"{Rule.Rule50}_{siteDb.LinkedServerName}";
            var spName = $"{key}_DBA_Consistency_CheckDuplicatedBalanceUpTicket";
            var criteria = new CriteriaData(spName);
            var data = repository.Fetch<TicketCheckingModel>(criteria).ToList();
            var result = new List<TicketCheckingModel>();
            data.ForEach(row =>
            {
                criteria = new CriteriaData(spName) { CustId = row.CustId, WinlostDate = row.WinlostDate, Retry = 1 };
                var dataByCustId = repository.Fetch<TicketCheckingModel>(criteria).ToList();
                result.AddRange(dataByCustId);
            });
            dict.Add(key, result);
        });

        return dict;
    }

    public Dictionary<string, List<TicketCheckingModel>> SelectDB05XDB06XConsistencyCheckDuplicatedBalanceUpTicket()
    {
        var dbs = DB.DB05X06X;
        var dict = new Dictionary<string, List<TicketCheckingModel>>();
        dbs.ForEach(db =>
        {
            var spName = $"{db}_DBA_Consistency_CheckDuplicatedBalanceUpTicket";
            var criteria = new CriteriaData(spName);
            var data = repository.Fetch<TicketCheckingModel>(criteria).ToList();
            var result = new List<TicketCheckingModel>();
            data.ForEach(row =>
            {
                criteria = new CriteriaData(spName) { CustId = row.CustId, WinlostDate = row.WinlostDate, Retry = 1 };
                var dataByCustId = repository.Fetch<TicketCheckingModel>(criteria).ToList();
                result.AddRange(dataByCustId);
            });

            dict.Add(db, result);
        });

        return dict;
    }

    public List<TicketCheckingModel> SelectConsistencyCheckDuplicatedMoveBetTicket()
    {
        const string spName = "DBA_Consistency_CheckDuplicatedMoveBetTicket";
        var criteria = new CriteriaData(spName);
        var data = repository.Fetch<TicketCheckingModel>(criteria).ToList();
        var result = new List<TicketCheckingModel>();
        data.ForEach(row =>
        {
            criteria = new CriteriaData(spName) { CustId = row.CustId, WinlostDate = row.WinlostDate, Retry = 1 };
            var dataByCustId = repository.Fetch<TicketCheckingModel>(criteria).ToList();
            result.AddRange(dataByCustId);
        });

        return result;
    }

    public Dictionary<string, List<TicketCheckingModel>> SelectSiteDBConsistencyCheckDuplicatedMoveBetTicket()
    {
        var siteDbs = QuerySiteDB(string.Empty);
        var dict = new Dictionary<string, List<TicketCheckingModel>>();
        siteDbs.ForEach(siteDb =>
        {
            var key = $"{Rule.Rule50}_{siteDb.LinkedServerName}";
            var spName = $"{key}_DBA_Consistency_CheckDuplicatedMoveBetTicket";
            var criteria = new CriteriaData(spName);
            var data = repository.Fetch<TicketCheckingModel>(criteria).ToList();
            var result = new List<TicketCheckingModel>();
            data.ForEach(row =>
            {
                criteria = new CriteriaData(spName) { CustId = row.CustId, WinlostDate = row.WinlostDate, Retry = 1 };
                var dataByCustId = repository.Fetch<TicketCheckingModel>(criteria).ToList();
                result.AddRange(dataByCustId);
            });
            dict.Add(key, result);
        });

        return dict;
    }

    public Dictionary<string, List<TicketCheckingModel>> SelectDB05XDB06XConsistencyCheckDuplicatedMoveBetTicket()
    {
        var dbs = DB.DB05X06X;
        var dict = new Dictionary<string, List<TicketCheckingModel>>();
        dbs.ForEach(db =>
        {
            var spName = $"{db}_DBA_Consistency_CheckDuplicatedMoveBetTicket";
            var criteria = new CriteriaData(spName);
            var data = repository.Fetch<TicketCheckingModel>(criteria).ToList();
            var result = new List<TicketCheckingModel>();
            data.ForEach(row =>
            {
                criteria = new CriteriaData(spName) { CustId = row.CustId, WinlostDate = row.WinlostDate, Retry = 1 };
                var dataByCustId = repository.Fetch<TicketCheckingModel>(criteria).ToList();
                result.AddRange(dataByCustId);
            });
            dict.Add(db, result);
        });

        return dict;
    }

    public List<MBUTicketCountModel> SelectConsistencyCheckMBUTicketCount(int bettype)
    {
        const string spName = "DBA_Consistency_CheckMBUTicketCount";
        var criteria = new CheckMBUTicketCountCriteria(spName) { Bettype = bettype };
        return repository.Fetch<MBUTicketCountModel>(criteria).ToList();
    }

    public Dictionary<string, List<MBUTicketCountModel>> SelectSiteDBConsistencyCheckMBUTicketCount(int bettype)
    {
        var siteDbs = QuerySiteDB(string.Empty);
        var dict = new Dictionary<string, List<MBUTicketCountModel>>();
        siteDbs.ForEach(siteDb =>
        {
            var key = $"{Rule.Rule50}_{siteDb.LinkedServerName}";
            var spName = $"{key}_SiteDB_DBA_Consistency_CheckMBUTicketCount";
            var criteria = new CheckMBUTicketCountCriteria(spName) { Bettype = bettype };
            var data = repository.Fetch<MBUTicketCountModel>(criteria).ToList();

            dict.Add(key, data);
        });

        return dict;
    }

    public Dictionary<string, List<MBUTicketCountModel>> SelectDB05XDB06XConsistencyCheckMBUTicketCount(int bettype)
    {
        var dbs = DB.DB05X06X;
        var dict = new Dictionary<string, List<MBUTicketCountModel>>();
        dbs.ForEach(db =>
        {
            var spName = $"{db}_DBA_Consistency_CheckMBUTicketCount";
            var criteria = new CheckMBUTicketCountCriteria(spName) { Bettype = bettype };
            var data = repository.Fetch<MBUTicketCountModel>(criteria).ToList();

            dict.Add(db, data);
        });

        return dict;
    }

    public Dictionary<string, Tuple<List<SiteDBGroupIDModel>, List<Rule53FromToModel>>> Rule53SelectSiteDBConsistencyDetectSiteDBGroupID()
    {
        var siteDbs = QuerySiteDB(string.Empty);
        var dict = new Dictionary<string, Tuple<List<SiteDBGroupIDModel>, List<Rule53FromToModel>>>();
        siteDbs.ForEach(siteDb =>
        {
            var key = $"{Rule.Rule53}_{siteDb.LinkedServerName}";
            var spName = $"{key}_SiteDB_DBA_Consistency_DetectSiteDBGroupID_Job";
            var criteria = new ConsistencyDetectSiteDBGroupIDJobCriteria(spName);
            var data = repository.FetchMultiple<SiteDBGroupIDModel, Rule53FromToModel>(criteria);

            dict.Add(key, new Tuple<List<SiteDBGroupIDModel>, List<Rule53FromToModel>>(data.Item1.ToList(), data.Item2.ToList()));
        });

        return dict;
    }

    private static ExecuteSPResultRule46 GetOutputResult(SiteDBRule46Criteria criteria)
    {
        var outputResult = new ExecuteSPResultRule46
        {
            SiteDBGroupID = criteria.SiteDBGroupID,
            TransferLastTransId = criteria.TransferLastTransId,
            NewTransferLastTransId = criteria.NewTransferLastTransId,
            CurrentTime = criteria.CurrentTime
        };

        return outputResult;
    }

    public Dictionary<string, Rule46DTO> SelectSiteDBConsistencyCheckTicketCount()
    {
        var siteDbs = QuerySiteDB(string.Empty);
        var dict = new Dictionary<string, Rule46DTO>();
        siteDbs.ForEach(siteDb =>
        {
            var siteDB = $"{Rule.Rule46}_{siteDb.LinkedServerName}";
            var criteria = new SiteDBRule46Criteria(siteDB);
            var data = new Rule46DTO();

            using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey(), 0);
            data.SiteDBDataResult = db.Query<ConsistencyCheckModelRule46>(criteria).ToList();
            data.SiteDBOutputResult = GetOutputResult(criteria);

            dict.Add(siteDB, data);
        });

        return dict;
    }

    public List<ConsistencyCheckModelRule46> SelectConsistencyCheckTicketCount(ExecuteSPResultRule46 siteDBOutputResult)
    {
        var criteria = new ConsistencyCheckTicketCountTransferCriteria(string.Empty)
        {
            SiteDBGroupID = siteDBOutputResult.SiteDBGroupID,
            TransferLastTransId = siteDBOutputResult.TransferLastTransId,
            NewTransferLastTransId = siteDBOutputResult.NewTransferLastTransId
        };

        var result = repository.Fetch<ConsistencyCheckModelRule46>(criteria).ToList();

        return result;
    }

    public List<ConsistencyCheckModelRule46> SelectDBXCheckTicketCount(string dbX, ExecuteSPResultRule46 siteDBOutputResult)
    {
        var criteria = new ConsistencyCheckTicketCountTransferCriteria(dbX)
        {
            SiteDBGroupID = siteDBOutputResult.SiteDBGroupID,
            TransferLastTransId = siteDBOutputResult.TransferLastTransId,
            NewTransferLastTransId = siteDBOutputResult.NewTransferLastTransId
        };

        var result = repository.Fetch<ConsistencyCheckModelRule46>(criteria).ToList();

        return result;
    }

    public Result UpdateSiteDBSystemSettingRule46(string siteDb, ExecuteSPResultRule46 siteDbOutputResult)
    {
        var cmd = new SiteDBSystemSettingUpdateRule46Command(siteDb)
        {
            CheckConsistencyKeyName = "SiteDB_CheckCons_Transfer_Count_LastTransId",
            NewTransferLastTransId = siteDbOutputResult.NewTransferLastTransId
        };

        if (AppConfigService.Config.IsUpdateData)
        {
            repository.Execute(cmd);
        }

        var result = new Result
        {
            ErrCode = cmd.ErrCode,
            ErrMsg = cmd.ErrMsg
        };

        return result;
    }

    public Dictionary<string, Rule47DTO> QueryRule47()
    {
        var siteDbs = QuerySiteDB(string.Empty);
        var dict = new Dictionary<string, Rule47DTO>();
        const string rule = Rule.Rule47;
        siteDbs.ForEach(siteDb =>
        {
            CallSiteDB(siteDb, dict, rule);
        });

        CallCentralDB(dict, rule);

        return dict;
    }

    private void CallSiteDB(SiteDBModel siteDb, IDictionary<string, Rule47DTO> dict, string rule)
    {
        var siteDB = $"{rule}_{siteDb.LinkedServerName}";
        var siteDBOutputResult = SelectSiteDBConsistencySystemSetting(siteDB);
        var siteDBDataResult = ConsistencyCheckWrongSiteGroupIDTransfer(siteDBOutputResult);
        var data = new Rule47DTO
        {
            DBOutputResult = siteDBOutputResult,
            DBDataResult = siteDBDataResult,
            SPNameOutputResult = "SiteDB_DBA_Consistency_SystemSetting_Select",
            SPNameDataResult = "DBA_Consistency_CheckWrongSiteGroupID_Transfer"
        };
        var result = UpdateSiteDBSystemSetting(siteDB, siteDBOutputResult);
        if (result.ErrCode > 0)
        {
            var errMsg = $"Fanex.CheckDataConsistencyRule47.Service: {result.ErrMsg}";
            throw new Exception(errMsg);
        }

        dict.Add(siteDB, data);
    }

    private Result UpdateSiteDBSystemSetting(string siteDB, ExecuteSPResult siteDBOutputResult)
    {
        var cmd = new SiteDBSystemSettingUpdateCommand(siteDB)
        {
            CheckConsistencyKeyName = "SiteDB_CheckCons_Transfer_DiffSGId_LastTransId",
            NewTransferLastTransId = siteDBOutputResult.NewTransferLastTransId
        };

        if (AppConfigService.Config.IsUpdateData)
        {
            repository.Execute(cmd);
        }

        var result = new Result
        {
            ErrCode = cmd.ErrCode,
            ErrMsg = cmd.ErrMsg
        };

        return result;
    }

    private List<ConsistencyRule47Model> ConsistencyCheckWrongSiteGroupIDTransfer(ExecuteSPResult siteDBOutputResult)
    {
        var criteria = new ConsistencyCheckWrongSiteGroupIDTransferCriteria
        {
            SiteDBGroupID = siteDBOutputResult.SiteDBGroupID,
            TransferLastTransId = siteDBOutputResult.TransferLastTransId,
            NewTransferLastTransId = siteDBOutputResult.NewTransferLastTransId
        };

        return repository.Fetch<ConsistencyRule47Model>(criteria).ToList();
    }

    private void CallCentralDB(IDictionary<string, Rule47DTO> dict, string rule)
    {
        var siteDB = $"{rule}_{DB.DBCONSISTENCY}";
        var centralDBOutputResult = ConsistencySystemSettingSelect();
        var centralDBDataResult = ConsistencyCheckWrongSiteGroupIDTransfer(centralDBOutputResult);
        var data = new Rule47DTO
        {
            DBOutputResult = centralDBOutputResult,
            DBDataResult = centralDBDataResult,
            SPNameOutputResult = "DBA_Consistency_SystemSetting_Select",
            SPNameDataResult = "DBA_Consistency_CheckWrongSiteGroupID_Transfer"
        };
        var result = UpdateCentralDBSystemSetting(centralDBOutputResult);
        if (result.ErrCode > 0)
        {
            var errMsg = $"Fanex.CheckDataConsistencyRule47.Service: {result.ErrMsg}";
            throw new Exception(errMsg);
        }

        dict.Add(siteDB, data);
    }

    private Result UpdateCentralDBSystemSetting(ExecuteSPResult centralDBOutputResult)
    {
        var cmd = new CentralDBSystemSettingUpdateCommand
        {
            CheckConsistencyKeyName = "DiffSGId_LastTransId",
            NewTransferLastTransId = centralDBOutputResult.NewTransferLastTransId
        };

        if (AppConfigService.Config.IsUpdateData)
        {
            repository.Execute(cmd);
        }

        var result = new Result
        {
            ErrCode = cmd.ErrCode,
            ErrMsg = cmd.ErrMsg
        };

        return result;
    }

    private ExecuteSPResult ConsistencySystemSettingSelect()
    {
        var cmd = new ConsistencySystemSettingSelectCommand
        {
            CheckConsistencyKeyName = "DiffSGId_LastTransId"
        };

        repository.Execute(cmd);
        var result = new ExecuteSPResult
        {
            TransferLastTransId = cmd.TransferLastTransId,
            NewTransferLastTransId = cmd.NewTransferLastTransId,
            ErrCode = cmd.ErrCode,
            ErrMsg = cmd.ErrMsg
        };
        return result;
    }

    private ExecuteSPResult SelectSiteDBConsistencySystemSetting(string siteDB)
    {
        var cmd = new SiteDBConsistencySystemSettingSelectCommand(siteDB)
        {
            CheckConsistencyKeyName = "SiteDB_CheckCons_Transfer_DiffSGId_LastTransId"
        };

        repository.Execute(cmd);
        var result = new ExecuteSPResult
        {
            SiteDBGroupID = cmd.SiteDBGroupID,
            TransferLastTransId = cmd.TransferLastTransId,
            NewTransferLastTransId = cmd.NewTransferLastTransId,
            ErrCode = cmd.ErrCode,
            ErrMsg = cmd.ErrMsg
        };

        return result;
    }

    public List<ConsistencyRule51Model> QueryRule51(Result result)
    {
        var criteria = new ConsistencyRule51Criteria();
        return repository.Fetch<ConsistencyRule51Model>(criteria).ToList();
    }

    public Dictionary<string, List<ConsistencyRule52Model>> QueryRule52()
    {
        var siteDbs = QuerySiteDB(string.Empty);
        var dict = new Dictionary<string, List<ConsistencyRule52Model>>();
        const string rule = Rule.Rule52;
        var index = 0;
        siteDbs.ForEach(siteDb =>
        {
            index++;
            var siteDB = $"{rule}_{siteDb.LinkedServerName}";
            var criteria = new SiteDBConsistencyRule52Criteria(siteDB);
            var rs = repository.Fetch<ConsistencyRule52Model>(criteria).ToList();
            if (criteria.ErrCode > 0)
            {
                dict.Add($"{index}_{siteDb.LinkedServerName}", rs);
            }
        });

        return dict;
    }

    public List<DataConsistencyConfigModel> GetConfigData()
    {
        var criteria = new DataConsistencyConfigSelectCriteria();
        return repository.Fetch<DataConsistencyConfigModel>(criteria).Where(s => s.GroupId == 2).ToList();
    }

    public Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> QueryCustInfoMiniCheckingFromDBGLOB(string tableName, string spName, string vvalue, string smaListJson, byte[] custlistJson)
    {
        var criteria = new CustInfoMiniCheckingCriteria(DB.DBGLOB, spName)
        {
            TableName = tableName,
            VValue = vvalue,
            SMAListJson = smaListJson,
            CustListJson = custlistJson
        };
        var result = repository.FetchMultiple<CustInfoTable1Model, CustInfoTable2Model>(criteria);

        var table1 = result.FirstResultSet.ToList();
        var table2 = result.Item2.ToList().FirstOrDefault();

        return new Tuple<List<CustInfoTable1Model>, CustInfoTable2Model>(table1, table2);
    }

    public Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> QueryCustSubMiniCheckingFromDBGLOB(string tableName, string spName, string vvalue, string custMiniList, byte[] custlistJson)
    {
        var criteria = new CustSubMiniCheckingCriteria(DB.DBGLOB, spName)
        {
            TableName = tableName,
            VValue = vvalue,
            CustMiniList = custMiniList,
            CustListJson = custlistJson
        };
        var result = repository.FetchMultiple<CustInfoTable1Model, CustInfoTable2Model>(criteria);

        var table1 = result.FirstResultSet.ToList();
        var table2 = result.Item2.ToList().FirstOrDefault();

        return new Tuple<List<CustInfoTable1Model>, CustInfoTable2Model>(table1, table2);
    }

    public List<CustInfoTable1Model> QueryCustInfoFromDBTP(string tableName, string spName, byte[] custlistJson)
    {
        var criteria = new CustInfoMiniCheckingCriteria(DB.DBTP, spName)
        {
            TableName = tableName,
            CustListJson = custlistJson
        };

        var result = repository.Fetch<CustInfoTable1Model>(criteria).ToList();
        return result;
    }

    public void UpdateConfig(DataConsistencyConfigUpdateCommand command)
    {
        if (AppConfigService.Config.IsUpdateData)
        {
            repository.Execute(command);
        }
    }

    public CustomerModel QueryCustomerFromDBTP(string tableName, string spName, string vvalue)
    {
        var criteria = new CustomerCheckingCriteria(DB.DBTP, spName)
        {
            TableName = tableName,
            VValue = vvalue
        };

        return repository.Get<CustomerModel>(criteria);
    }

    public CustomerModel QueryCustomerFromDBGLOB(string tableName, string spName, string vvalue, int maxCustNew)
    {
        var criteria = new CustomerCheckingCriteria(DB.DBGLOB, spName)
        {
            TableName = tableName,
            VValue = vvalue,
            MaxCust_New = maxCustNew
        };

        return repository.Get<CustomerModel>(criteria);
    }
}