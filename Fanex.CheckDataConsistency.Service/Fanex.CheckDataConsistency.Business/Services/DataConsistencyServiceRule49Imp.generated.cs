using Fanex.CheckDataConsistency.Business.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services
{
    public class DataConsistencyServiceRule49Imp : DataConsistencyRule4849Service
    {
        private readonly DataService dataService;
        private readonly Log log;

        public DataConsistencyServiceRule49Imp(DataService dataService, Log log)
        {
            this.dataService = dataService;
            this.log = log;
        }

        public List<MessageRule4849Model> QueryData(DateTime currentTime)
        {
            var result = new List<MessageRule4849Model>();

            var dataSiteDBs = dataService.Rule49SelectSiteDBConsistencyCheckSumTicket(currentTime);
            var dataConsistency = dataService.Rule49SelectConsistencyCheckSumTicket(currentTime);
            var dataDb05X06x = dataService.Rule49SelectDB05XDB06XConsistencyCheckSumTicket(currentTime);
            var fromToData = dataConsistency.Item2;

            var jsonDataSiteDBs = JsonConvert.SerializeObject(dataSiteDBs, Formatting.Indented);
            var jsonDataConsistency = JsonConvert.SerializeObject(dataConsistency, Formatting.Indented);
            var jsonDataDb05X06x = JsonConvert.SerializeObject(dataDb05X06x, Formatting.Indented);
            var date = $"{DateTime.Today:yyyyMMdd}-Retry-{AppConfigService.RetryCount}";
            var path = $"{Directory.CreateDirectory(AppConfigService.Config.Path)}/{DateTime.Now:yyyyMMdd}";
            Directory.CreateDirectory(path);
            File.WriteAllText($"{path}/{Rule.Rule49}-{date}-DataSiteDBs.json", jsonDataSiteDBs);
            File.WriteAllText($"{path}/{Rule.Rule49}-{date}-DataConsistency.json", jsonDataConsistency);
            File.WriteAllText($"{path}/{Rule.Rule49}-{date}-DataDb05X06x.json", jsonDataDb05X06x);

            var notExistsRows = new Dictionary<string, ConsistencyCheckModel>();
            var diffRows = new Dictionary<string, ConsistencyCheckModel>();
            var retryInfo = new List<RetryInfo>();
            var retryInfoFinal = new List<RetryInfo>();

            foreach (var dataSiteDB in dataSiteDBs)
            {
                var siteDBInternal = dataSiteDB.Key.Split('_')[1];
                AssignDiffData(dataConsistency, diffRows, dataSiteDB, siteDBInternal, DB.DBCONSISTENCY);
                AssignNotExistsData(dataConsistency, notExistsRows, dataSiteDB, siteDBInternal, DB.DBCONSISTENCY);
                GetRetryInfo(notExistsRows, diffRows, retryInfo, siteDBInternal);
            }

            foreach (var dataSiteDB in dataSiteDBs)
            {
                foreach (var db05x06x in dataDb05X06x)
                {
                    var siteDBInternal = dataSiteDB.Key.Split('_')[1];
                    var dataDbX = new Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>>(db05x06x.Value, fromToData);
                    var dbX = db05x06x.Key.Split('_')[0];
                    AssignDiffData(dataDbX, diffRows, dataSiteDB, siteDBInternal, dbX);
                    AssignNotExistsData(dataDbX, notExistsRows, dataSiteDB, siteDBInternal, dbX);
                    GetRetryInfo(notExistsRows, diffRows, retryInfo, siteDBInternal);
                }
            }

            var distinctRetry = retryInfo.GroupBy(g => new { g.SiteDB, g.TableName });
            var recommendList = new List<int>();
            foreach (var item in distinctRetry)
            {
                var recommends = string.Join(",", item.Select(s => s.RecommendList).Distinct());
                recommendList.Add(item.Select(s => s.RecommendList).Distinct().Count());
                retryInfoFinal.Add(new RetryInfo { Rule = Rule.Rule49, SiteDB = item.Key.SiteDB, TableName = item.Key.TableName, RecommendList = recommends });
            }

            var countRecommendList = recommendList.Count > 0 ? recommendList.Max() : 0;
            var isNeedResetRule49 = countRecommendList > 100;

            if (AppConfigService.RetryCount > 0 && retryInfoFinal.Count == 0)
            {
                retryInfoFinal.Add(new RetryInfo()
                {
                    RecommendList = "1",
                    Rule = Rule.Rule49,
                    TableName = "bettrans",
                    SiteDB = "DBSite01"
                });
            }

            if (isNeedResetRule49)
            {
                retryInfoFinal.Clear();
                retryInfoFinal.Add(new RetryInfo { Rule = Rule.Rule49, SiteDB = string.Empty, TableName = string.Empty, RecommendList = string.Empty });
            }

            if (AppConfigService.RetryInfos.ContainsKey(Rule.Rule49))
            {
                AppConfigService.RetryInfos[Rule.Rule49] = retryInfoFinal;
            }
            else
            {
                AppConfigService.RetryInfos.Add(Rule.Rule49, retryInfoFinal);
            }
            var jSonRetryInfo = JsonConvert.SerializeObject(AppConfigService.RetryInfos, Formatting.Indented);
            log.Log($"Retrying the Rule 48-49: [Rule49] {AppConfigService.RetryCount} times at {DateTime.Now}, {Environment.NewLine} {jSonRetryInfo}");

            var notExistsModel = new MessageRule4849Model
            {
                Rule = "#49",
                Message = "Tickets are mismatching between SiteDB and DBTP, between SiteDB and DB052, DB053, DB061, DB062",
                SPName = "DBA_Consistency_CheckSumTicket, SiteDB_DBA_Consistency_CheckSumTicket",
                Data = notExistsRows.Skip(0).Take(Rule.TOP100).ToDictionary(d => d.Key, d => d.Value),
                FromToDates = dataConsistency.Item2.Skip(0).Take(Rule.TOP1).ToList(),
                IsSeletTop = notExistsRows.Count > Rule.TOP100
            };

            var diffModel = new MessageRule4849Model
            {
                Rule = "#49",
                Message = "Tickets are different TotalCheckSumAgg between SiteDB and DBTP, between SiteDB and DB052, DB053, DB061, DB062",
                SPName = "DBA_Consistency_CheckSumTicket, SiteDB_DBA_Consistency_CheckSumTicket",
                Data = diffRows.Skip(0).Take(Rule.TOP100).ToDictionary(d => d.Key, d => d.Value),
                FromToDates = dataConsistency.Item2.Skip(0).Take(Rule.TOP1).ToList(),
                IsSeletTop = diffRows.Count > Rule.TOP100
            };

            if (notExistsRows.Count > 0)
            {
                result.Add(notExistsModel);
            }

            if (diffRows.Count > 0)
            {
                result.Add(diffModel);
            }

            return result;
        }

        private static void GetRetryInfo(
            Dictionary<string, ConsistencyCheckModel> notExistsRows,
            Dictionary<string, ConsistencyCheckModel> diffRows,
            List<RetryInfo> retryInfo,
            string siteDBInternal)
        {
            var dataDiffBySiteDB = diffRows.Where(s => s.Key.Split('-')[1].Equals(siteDBInternal));
            dataDiffBySiteDB.GroupBy(g => new { g.Value.TableName }).ToList()
                .ForEach(table =>
                {
                    var recommends = table.Select(s => s.Value.Recommend).Distinct();
                    retryInfo.Add(new RetryInfo
                    {
                        Rule = Rule.Rule49,
                        SiteDB = siteDBInternal,
                        TableName = table.Key.TableName,
                        RecommendList = string.Join(",", recommends)
                    });
                });

            var dataNotExistsBySiteDB = notExistsRows.Where(s => s.Key.Split('-')[1].Equals(siteDBInternal));
            dataNotExistsBySiteDB.GroupBy(g => new { g.Value.TableName }).ToList()
                .ForEach(table =>
                {
                    var recommends = table.Select(s => s.Value.Recommend).Distinct();
                    retryInfo.Add(new RetryInfo
                    {
                        Rule = Rule.Rule49,
                        SiteDB = siteDBInternal,
                        TableName = table.Key.TableName,
                        RecommendList = string.Join(",", recommends)
                    });
                });
        }

        private static void AssignNotExistsData(
            Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>> dataConsistency,
            Dictionary<string, ConsistencyCheckModel> notExistsRows,
            KeyValuePair<string, List<ConsistencyCheckModel>> dataSiteDB,
            string siteDB,
            string dbX)
        {
            int notExistsIndex = notExistsRows.Count + 1;
            dataSiteDB.Value.ForEach(siteDBRow =>
            {
                var consisRow = dataConsistency.Item1.FirstOrDefault(s => s.TableName == siteDBRow.TableName
                && s.SiteDBGroupID == siteDBRow.SiteDBGroupID
                && s.Recommend == siteDBRow.Recommend);

                if (consisRow == null)
                {
                    notExistsRows.Add($"{notExistsIndex}-{siteDB}", siteDBRow);
                    notExistsRows.Add($"{notExistsIndex}-{dbX}", consisRow);
                    notExistsIndex++;
                }
            });
        }

        private static void AssignDiffData(
            Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>> dataConsistency,
            Dictionary<string, ConsistencyCheckModel> diffRows,
            KeyValuePair<string, List<ConsistencyCheckModel>> dataSiteDB,
            string siteDB,
            string dbX)
        {
            int diffIndex = diffRows.Count + 1;
            dataConsistency.Item1.ForEach(consisRow =>
            {
                var siteDBRow = dataSiteDB.Value.FirstOrDefault(s => s.TableName == consisRow.TableName
                && s.SiteDBGroupID == consisRow.SiteDBGroupID
                && s.Recommend == consisRow.Recommend);

                if (siteDBRow != null && consisRow.TotalCheckSumAgg != siteDBRow.TotalCheckSumAgg)
                {
                    diffRows.Add($"{diffIndex}-{dbX}", consisRow);
                    diffRows.Add($"{diffIndex}-{siteDB}", siteDBRow);
                    diffIndex++;
                }
            });
        }
    }
}