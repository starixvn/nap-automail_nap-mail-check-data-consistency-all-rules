using Fanex.CheckDataConsistency.Business.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule46Service
{
    List<MessageRule46Model> QueryData();

    void UpdateSiteDBSystemSettingRule46(Dictionary<string, Rule46DTO> dataSiteDBs);
}

public class DataConsistencyRule46ServiceImp(DataService dataService) : DataConsistencyRule46Service
{
    public List<MessageRule46Model> QueryData()
    {
        var timeJobExecute = DateTime.Now;
        var result = new List<MessageRule46Model>();
        var dataSiteDBs = dataService.SelectSiteDBConsistencyCheckTicketCount();

        var notExistsRows = new Dictionary<string, ConsistencyCheckModelRule46>();
        var diffRows = new Dictionary<string, ConsistencyCheckModelRule46>();
        var spList = new Dictionary<string, string>();

        SelectConsistencyCheckTicketCount(dataSiteDBs);
        AppConfigService.DataSiteDBs = dataSiteDBs;

        var jsonDataSiteDBs = JsonConvert.SerializeObject(dataSiteDBs, Formatting.Indented);
        Directory.CreateDirectory(AppConfigService.Config.Path);
        File.WriteAllText($"{AppConfigService.Config.Path}/{Rule.Rule46}-DataSiteDBs.json", jsonDataSiteDBs);

        foreach (var dataSiteDb in dataSiteDBs)
        {
            var siteDbData = dataSiteDb.Value.SiteDBDataResult;
            var dataConsistency = dataSiteDb.Value.CentralDBDataResult;

            var siteDb = dataSiteDb.Key.Split('_')[1];
            CheckDataOnConsistency(notExistsRows, diffRows, spList, dataSiteDb, siteDbData, dataConsistency, siteDb);
        }

        var notExistsModel = new MessageRule46Model
        {
            Rule = "#46",
            Message = "Transfer tickets: Check data latency between SiteBD and DBTP, between SiteDB and [DB052, DB053, DB061, DB062] by comparing ticket count & checksum on columns: " +
                      "Tickets are mismatching between DBTP and SiteDB, between SiteDB and [DB052, DB053, DB061, DB062]",
            SPName = string.Join("", spList.Select(s => s.Value)),
            TimeJobExecute = timeJobExecute,
            Data = notExistsRows.Skip(0).Take(100).ToDictionary(s => s.Key, s => s.Value)
        };

        var diffModel = new MessageRule46Model
        {
            Rule = "#46",
            Message = "Transfer tickets: Check data latency between SiteBD and DBTP by comparing ticket count & checksum on columns: " +
                      "Tickets are different on TicketCount or TotalCheckSum between SiteDB and DBTP, between SiteDB and [DB052, DB053, DB061, DB062]",
            SPName = string.Join("", spList.Select(s => s.Value)),
            TimeJobExecute = timeJobExecute,
            Data = diffRows.Skip(0).Take(100).ToDictionary(s => s.Key, s => s.Value)
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

    private static void CheckDataOnConsistency(
        Dictionary<string, ConsistencyCheckModelRule46> notExistsRows,
        Dictionary<string, ConsistencyCheckModelRule46> diffRows,
        Dictionary<string, string> spList,
        KeyValuePair<string, Rule46DTO> dataSiteDb,
        List<ConsistencyCheckModelRule46> siteDbData,
        List<ConsistencyCheckModelRule46> dataConsistency,
        string siteDb)
    {
        var notExistsIndex = notExistsRows.Count;
        var diffIndex = diffRows.Count;

        siteDbData.ForEach(siteDbRow =>
        {
            var consisRow = dataConsistency.FirstOrDefault(s => s.TableName == siteDbRow.TableName
                                                                && s.SiteDBGroupID == siteDbRow.SiteDBGroupID
                                                                && s.Recommend == siteDbRow.Recommend);

            if (consisRow == null)
            {
                notExistsIndex++;

                if (!notExistsRows.ContainsKey($"{notExistsIndex}-{DB.DBCONSISTENCY}"))
                {
                    notExistsRows.Add($"{notExistsIndex}-{DB.DBCONSISTENCY}", consisRow);
                }

                if (!notExistsRows.ContainsKey($"{notExistsIndex}-{siteDb}"))
                {
                    notExistsRows.Add($"{notExistsIndex}-{siteDb}", siteDbRow);
                }

                if (!spList.ContainsKey(siteDb))
                {
                    spList.Add(siteDb, $"{dataSiteDb.Value.SPNameOutputResult}, {dataSiteDb.Value.SPNameDataResult}");
                }
            }
            else
            {
                if (siteDbRow.TicketCount != consisRow.TicketCount || siteDbRow.TotalCheckSum != consisRow.TotalCheckSum)
                {
                    diffIndex++;

                    if (!diffRows.ContainsKey($"{diffIndex}-{DB.DBCONSISTENCY}"))
                    {
                        diffRows.Add($"{diffIndex}-{DB.DBCONSISTENCY}", consisRow);
                    }

                    if (!diffRows.ContainsKey($"{diffIndex}-{siteDb}"))
                    {
                        diffRows.Add($"{diffIndex}-{siteDb}", siteDbRow);
                    }

                    if (!spList.ContainsKey(siteDb))
                    {
                        spList.Add(siteDb, $"{dataSiteDb.Value.SPNameOutputResult}, {dataSiteDb.Value.SPNameDataResult}");
                    }
                }
            }
        });

        foreach (var dbX in dataSiteDb.Value.Db05Xdb06XDataResult)
        {
            foreach (var siteDbRow in siteDbData)
            {
                var row = siteDbRow;
                var dbXRow = dbX.Value.FirstOrDefault(s => s.TableName == row.TableName
                                                           && s.SiteDBGroupID == row.SiteDBGroupID
                                                           && s.Recommend == row.Recommend);

                if (dbX.Key.Equals("DB052") || dbX.Key.Equals("DB053"))
                {
                    if (siteDbRow.TableName.Equals("Transfer_Coll"))
                    {
                        continue;
                    }
                }

                if (dbXRow == null)
                {
                    notExistsIndex++;

                    if (!notExistsRows.ContainsKey($"{notExistsIndex}-{siteDb}"))
                    {
                        notExistsRows.Add($"{notExistsIndex}-{siteDb}", siteDbRow);
                    }

                    if (!notExistsRows.ContainsKey($"{notExistsIndex}-{dbX.Key}"))
                    {
                        notExistsRows.Add($"{notExistsIndex}-{dbX.Key}", dbXRow);
                    }

                    if (!spList.ContainsKey(siteDb))
                    {
                        spList.Add(siteDb, $"{dataSiteDb.Value.SPNameOutputResult}, {dataSiteDb.Value.SPNameDataResult}");
                    }
                }
                else
                {
                    if (siteDbRow.TicketCount != dbXRow.TicketCount || siteDbRow.TotalCheckSum != dbXRow.TotalCheckSum)
                    {
                        diffIndex++;

                        if (!diffRows.ContainsKey($"{diffIndex}-{siteDb}"))
                        {
                            diffRows.Add($"{diffIndex}-{siteDb}", siteDbRow);
                        }

                        if (!diffRows.ContainsKey($"{diffIndex}-{dbX.Key}"))
                        {
                            diffRows.Add($"{diffIndex}-{dbX.Key}", dbXRow);
                        }

                        if (!spList.ContainsKey(siteDb))
                        {
                            spList.Add(siteDb, $"{dataSiteDb.Value.SPNameOutputResult}, {dataSiteDb.Value.SPNameDataResult}");
                        }
                    }
                }
            }
        }
    }

    private void SelectConsistencyCheckTicketCount(Dictionary<string, Rule46DTO> dataSiteDBs)
    {
        var dbXs = DB.DB05X06X;
        foreach (var dataSiteDb in dataSiteDBs)
        {
            var siteDbOutputResult = dataSiteDb.Value.SiteDBOutputResult;
            var centralDbDataResult = dataService.SelectConsistencyCheckTicketCount(siteDbOutputResult);
            dataSiteDb.Value.CentralDBDataResult = centralDbDataResult;
            dataSiteDb.Value.SPNameOutputResult = "SiteDB_DBA_Consistency_CheckTicketCountAndChecksum_Transfer";
            dataSiteDb.Value.SPNameDataResult = $"DBA_Consistency_CheckTicketCountAndChecksum_Transfer {JsonConvert.SerializeObject(siteDbOutputResult, Formatting.Indented)}";

            foreach (var dbX in dbXs)
            {
                var db = $"{dbX}_";
                var dataDbX = dataService.SelectDBXCheckTicketCount(db, siteDbOutputResult);
                dataSiteDb.Value.Db05Xdb06XDataResult.Add(dbX, dataDbX);
            }
        }
    }

    public void UpdateSiteDBSystemSettingRule46(Dictionary<string, Rule46DTO> dataSiteDBs)
    {
        foreach (var dataSiteDB in dataSiteDBs)
        {
            var siteDBOutputResult = dataSiteDB.Value.SiteDBOutputResult;
            var siteDB = dataSiteDB.Key;
            var resultSiteDB = dataService.UpdateSiteDBSystemSettingRule46(siteDB, siteDBOutputResult);

            if (resultSiteDB.ErrCode > 0)
            {
                var errMsg = $"Fanex.CheckDataConsistencyRule46.Service: {resultSiteDB.ErrMsg}";
                throw new ArgumentException(errMsg);
            }
        }
    }
}