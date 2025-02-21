using Fanex.CheckDataConsistency.Business.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace Fanex.CheckDataConsistency.Business.Services
{
    public class DataConsistencyServiceRule48Imp : DataConsistencyRule4849Service
    {
        private readonly DataService dataService;
        private Log log;

        public DataConsistencyServiceRule48Imp(
            DataService dataService,
            Log log)
        {
            this.dataService = dataService;
            this.log = log;
        }

        public List<MessageRule4849Model> QueryData(DateTime currentTime)
        {
            var result = new List<MessageRule4849Model>();

            var dataSiteDBs = dataService.Rule48SelectSiteDBConsistencyCheckTicketCount(currentTime);
            var dataConsistency = dataService.Rule48SelectConsistencyCheckTicketCount(currentTime);
            var dataDb05X06x = dataService.Rule48SelectDB05XDB06XConsistencyCheckTicketCount(currentTime);
            var fromToData = dataConsistency.Item2;

            var jsonDataSiteDBs = JsonConvert.SerializeObject(dataSiteDBs, Formatting.Indented);
            var jsonDataConsistency = JsonConvert.SerializeObject(dataConsistency, Formatting.Indented);
            var jsonDataDb05X06x = JsonConvert.SerializeObject(dataDb05X06x, Formatting.Indented);
            var date = $"{DateTime.Today:yyyyMMdd}-Retry-{AppConfigService.RetryCountRule4849}";
            var path = $"{Directory.CreateDirectory(AppConfigService.Config.Path)}/{DateTime.Now:yyyyMMdd}";
            Directory.CreateDirectory(path);
            File.WriteAllText($"{path}/{Rule.Rule48}-{date}-DataSiteDBs.json", jsonDataSiteDBs);
            File.WriteAllText($"{path}/{Rule.Rule48}-{date}-DataConsistency.json", jsonDataConsistency);
            File.WriteAllText($"{path}/{Rule.Rule48}-{date}-DataDb05X06x.json", jsonDataDb05X06x);

            var notExistsRows = new Dictionary<string, ConsistencyCheckModel>();
            var diffRows = new Dictionary<string, ConsistencyCheckModel>();

            var retryInfo = new List<RetryInfo>();
            var retryInfoFinal = new List<RetryInfo>();

            foreach (var dataSiteDB in dataSiteDBs)
            {
                var siteDBInternal = dataSiteDB.Key.Split('_')[1];
                AssignDiffData(dataConsistency, diffRows, dataSiteDB, siteDBInternal, DB.DBCONSISTENCY);
                AssignNotExistsData(dataConsistency, notExistsRows, dataSiteDB, siteDBInternal, DB.DBCONSISTENCY, currentTime);
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
                    AssignNotExistsData(dataDbX, notExistsRows, dataSiteDB, siteDBInternal, dbX, currentTime);

                    var key = $"{siteDBInternal}-{dbX}";
                    GetRetryInfo(notExistsRows, diffRows, retryInfo, siteDBInternal);
                }
            }

            var distinctRetry = retryInfo.GroupBy(g => new { g.SiteDB, g.TableName });
            foreach (var item in distinctRetry)
            {
                var smaList = new List<SMAModel>();
                item.ToList().ForEach(row =>
                {
                    smaList.AddRange(row.SMAList);
                });
                var shortSMAList = new List<SMAModel>();
                smaList.GroupBy(g => new { g.Recommend, g.Mrecommend, g.Srecommend }).ToList().ForEach(sma =>
                {
                    shortSMAList.Add(new SMAModel { Recommend = sma.Key.Recommend, Mrecommend = sma.Key.Mrecommend, Srecommend = sma.Key.Srecommend });
                });

                retryInfoFinal.Add(new RetryInfo { Rule = Rule.Rule48, SiteDB = item.Key.SiteDB, TableName = item.Key.TableName, SMAList = shortSMAList, RecommendList = JsonConvert.SerializeObject(shortSMAList, Formatting.Indented) });
            }

            if (AppConfigService.RetryCountRule4849 > 0 && retryInfoFinal.Count == 0)
            {
                retryInfoFinal.Add(new RetryInfo()
                {
                    RecommendList = "[]",
                    Rule = Rule.Rule48,
                    TableName = "bettrans",
                    SiteDB = "DBSite01"
                });
            }

            if (AppConfigService.RetryInfos.ContainsKey(Rule.Rule48))
            {
                AppConfigService.RetryInfos[Rule.Rule48] = retryInfoFinal;
            }
            else
            {
                AppConfigService.RetryInfos.Add(Rule.Rule48, retryInfoFinal);
            }

            var jSonRetryInfo = JsonConvert.SerializeObject(AppConfigService.RetryInfos, Formatting.Indented);
            log.Log($"Retrying the Rule 48-49: [Rule48] {AppConfigService.RetryCountRule4849} times at {DateTime.Now}, {Environment.NewLine} {jSonRetryInfo}");

            var notExistsModel = new MessageRule4849Model
            {
                Rule = "#48",
                Message = "Tickets are mismatching between SiteDB and DBTP, between SiteDB and DB052, DB053, DB061, DB062",
                SPName = "DBA_Consistency_CheckTicketCount, SiteDB_DBA_Consistency_CheckTicketCount",
                Data = notExistsRows.Skip(0).Take(Rule.TOP100).ToDictionary(d => d.Key, d => d.Value),
                FromToDates = dataConsistency.Item2.Skip(0).Take(Rule.TOP1).ToList(),
                IsSeletTop = notExistsRows.Count > Rule.TOP100
            };

            var diffModel = new MessageRule4849Model
            {
                Rule = "#48",
                Message = "Tickets are different TicketCount between SiteDB and DBTP, between SiteDB and DB052, DB053, DB061, DB062",
                SPName = "DBA_Consistency_CheckTicketCount, SiteDB_DBA_Consistency_CheckTicketCount",
                Data = diffRows.Skip(0).Take(Rule.TOP100).ToDictionary(d => d.Key, d => d.Value),
                FromToDates = dataConsistency.Item2.Skip(0).Take(Rule.TOP1).ToList(),
                IsSeletTop = diffRows.Count > Rule.TOP100
            };

            if (notExistsModel.Data.Count > 0)
            {
                result.Add(notExistsModel);
            }

            if (diffModel.Data.Count > 0)
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
                    var smaList = new List<SMAModel>();
                    table.Select(s => new SMAModel { Recommend = s.Value.Recommend, Mrecommend = s.Value.Mrecommend, Srecommend = s.Value.Srecommend })
                        .GroupBy(g => new { g.Recommend, g.Mrecommend, g.Srecommend })
                        .ToList()
                        .ForEach(sma =>
                        {
                            smaList.Add(new SMAModel { Recommend = sma.Key.Recommend, Mrecommend = sma.Key.Mrecommend, Srecommend = sma.Key.Srecommend });
                        });
                    retryInfo.Add(new RetryInfo
                    {
                        Rule = Rule.Rule48,
                        SiteDB = siteDBInternal,
                        TableName = table.Key.TableName,
                        SMAList = smaList,
                        RecommendList = JsonConvert.SerializeObject(smaList, Formatting.Indented)
                    });
                });

            var dataNotExistsBySiteDB = notExistsRows.Where(s => s.Key.Split('-')[1].Equals(siteDBInternal));
            dataNotExistsBySiteDB.GroupBy(g => new { g.Value.TableName }).ToList()
                .ForEach(table =>
                {
                    var smaList = new List<SMAModel>();
                    table.Select(s => new SMAModel { Recommend = s.Value.Recommend, Mrecommend = s.Value.Mrecommend, Srecommend = s.Value.Srecommend })
                        .GroupBy(g => new { g.Recommend, g.Mrecommend, g.Srecommend })
                        .ToList()
                        .ForEach(sma =>
                        {
                            smaList.Add(new SMAModel { Recommend = sma.Key.Recommend, Mrecommend = sma.Key.Mrecommend, Srecommend = sma.Key.Srecommend });
                        });
                    retryInfo.Add(new RetryInfo
                    {
                        Rule = Rule.Rule48,
                        SiteDB = siteDBInternal,
                        TableName = table.Key.TableName,
                        SMAList = smaList,
                        RecommendList = JsonConvert.SerializeObject(smaList, Formatting.Indented)
                    });
                });
        }

        private void AssignNotExistsData(
            Tuple<List<ConsistencyCheckModel>, List<Rule4849FromToModel>> dataConsistency,
            Dictionary<string, ConsistencyCheckModel> notExistsRows,
            KeyValuePair<string, List<ConsistencyCheckModel>> dataSiteDB,
            string siteDB,
            string dbX,
            DateTime currentTime)
        {
            int notExistsIndex = notExistsRows.Count + 1;
            dataSiteDB.Value.ForEach(siteDBRow =>
            {
                var consisRow = dataConsistency.Item1.FirstOrDefault(s => s.TableName == siteDBRow.TableName
                && s.SiteDBGroupID == siteDBRow.SiteDBGroupID
                && s.Recommend == siteDBRow.Recommend
                && s.Mrecommend == siteDBRow.Mrecommend
                && s.Srecommend == siteDBRow.Srecommend
                && s.Status == siteDBRow.Status);

                if (consisRow == null)
                {
                    notExistsRows.Add($"{notExistsIndex}-{siteDB}", siteDBRow);
                    notExistsRows.Add($"{notExistsIndex}-{dbX}", consisRow);
                    notExistsIndex++;
                }
            });
        }

        private void AssignDiffData(
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
                && s.Recommend == consisRow.Recommend
                && s.Mrecommend == consisRow.Mrecommend
                && s.Srecommend == consisRow.Srecommend
                && s.Status == consisRow.Status);

                if (siteDBRow != null && consisRow.TicketCount != siteDBRow.TicketCount)
                {
                    diffRows.Add($"{diffIndex}-{dbX}", consisRow);
                    diffRows.Add($"{diffIndex}-{siteDB}", siteDBRow);
                    diffIndex++;
                }
            });
        }
    }
}