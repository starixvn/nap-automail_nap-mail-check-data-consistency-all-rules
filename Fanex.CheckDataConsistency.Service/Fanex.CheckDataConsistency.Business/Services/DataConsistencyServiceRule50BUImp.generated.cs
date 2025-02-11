using System;
using Fanex.CheckDataConsistency.Business.Model;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services
{
    public class DataConsistencyServiceRule50BUImp : DataConsistencyRule50Service
    {
        private readonly DataService dataService;

        public DataConsistencyServiceRule50BUImp(DataService dataService)
        {
            this.dataService = dataService;
        }

        public List<MessageRule50Model> QueryData()
        {
            var result = new List<MessageRule50Model>();
            var dup = CheckDuplicatedMoveBetTicket();
            var mbu = CheckMBUTicketCount();

            result.AddRange(dup);
            result.AddRange(mbu);

            return result;
        }

        private List<MessageRule50Model> CheckDuplicatedMoveBetTicket()
        {
            var result = new List<MessageRule50Model>();
            var dataConsistency = dataService.SelectConsistencyCheckDuplicatedBalanceUpTicket();
            var dataSiteDBs = dataService.SelectSiteDBConsistencyCheckDuplicatedBalanceUpTicket();
            var db05X06X = dataService.SelectDB05XDB06XConsistencyCheckDuplicatedBalanceUpTicket();

            if (dataConsistency.Count > 0)
            {
                result.Add(new MessageRule50Model
                {
                    Rule = "#50",
                    Message = $"Ticket 20000: alert if exist custid having > 1 row of 20000 in SiteDB and CentralDB (check in DBTP)" +
                    $", run after balance up process done (web call)",
                    SPName = "DBA_Consistency_CheckDuplicatedBalanceUpTicket",
                    Data = new Dictionary<string, List<TicketCheckingModel>>
                    {
                        { DB.DBCONSISTENCY, dataConsistency }
                    }
                });
            }

            foreach (var dataSiteDB in dataSiteDBs)
            {
                if (dataSiteDB.Value.Count > 0)
                {
                    var siteDB = dataSiteDB.Key.Split('_')[1];
                    result.Add(new MessageRule50Model
                    {
                        Rule = "#50",
                        Message = $"Ticket 20000: alert if exist custid having > 1 row of 20000 in SiteDB and CentralDB (check in DBTP)" +
                    $", run after balance up process done (web call)",
                        SPName = "DBA_Consistency_CheckDuplicatedBalanceUpTicket",
                        Data = new Dictionary<string, List<TicketCheckingModel>>
                        {
                            { siteDB, dataSiteDB.Value }
                        }
                    });
                }
            }

            foreach (var dbX in db05X06X)
            {
                if (dbX.Value.Count > 0)
                {
                    var db = dbX.Key;
                    result.Add(new MessageRule50Model
                    {
                        Rule = "#50",
                        Message = $"Ticket 20000: alert if exist custid having > 1 row of 20000 in SiteDB and {db}" +
                                  $", run after balance up process done (web call)",
                        SPName = "DBA_Consistency_CheckDuplicatedBalanceUpTicket",
                        Data = new Dictionary<string, List<TicketCheckingModel>>
                        {
                            { db, dbX.Value }
                        }
                    });
                }
            }

            return result;
        }

        private List<MessageRule50Model> CheckMBUTicketCount()
        {
            var result = new List<MessageRule50Model>();
            var dataConsistency = dataService.SelectConsistencyCheckMBUTicketCount(20000);
            var dataSiteDBs = dataService.SelectSiteDBConsistencyCheckMBUTicketCount(20000);
            var db05X06X = dataService.SelectDB05XDB06XConsistencyCheckMBUTicketCount(20000);

            var diffRows = new Dictionary<string, MBUTicketCountModel>();
            int diffIndex = 0;

            foreach (var row in dataConsistency)
            {
                foreach (var dataSiteDB in dataSiteDBs)
                {
                    var siteDB = dataSiteDB.Key.Split('_')[1];
                    var Id = siteDB.Replace("DBSite", string.Empty);
                    var siteDbGroupId = 0;
                    Int32.TryParse(Id, out siteDbGroupId);
                    if (siteDbGroupId == row.SiteDBGroupID)
                    {
                        var siteDBRow = dataSiteDB.Value.FirstOrDefault(s => s.SiteDBGroupID == siteDbGroupId
                                                                             && s.WinlostDate == row.WinlostDate);
                        if (siteDBRow != null && (row.TotalCount != siteDBRow.TotalCount || row.TotalCheckSumAgg != siteDBRow.TotalCheckSumAgg))
                        {
                            diffIndex++;
                            diffRows.Add($"{diffIndex}-{DB.DBCONSISTENCY}", row);
                            diffRows.Add($"{diffIndex}-{siteDB}", siteDBRow);
                        }
                    }
                }
            }

            foreach (var dbX in db05X06X)
            {
                foreach (var row in dbX.Value)
                {
                    foreach (var dataSiteDB in dataSiteDBs)
                    {
                        var siteDB = dataSiteDB.Key.Split('_')[1];
                        var Id = siteDB.Replace("DBSite", string.Empty);
                        var siteDbGroupId = 0;
                        Int32.TryParse(Id, out siteDbGroupId);
                        if (siteDbGroupId == row.SiteDBGroupID)
                        {
                            var siteDBRow = dataSiteDB.Value.FirstOrDefault(s => s.SiteDBGroupID == siteDbGroupId
                                && s.WinlostDate == row.WinlostDate);
                            if (siteDBRow != null && (row.TotalCount != siteDBRow.TotalCount || row.TotalCheckSumAgg != siteDBRow.TotalCheckSumAgg))
                            {
                                diffIndex++;
                                diffRows.Add($"{diffIndex}-{dbX.Key}", row);
                                diffRows.Add($"{diffIndex}-{siteDB}", siteDBRow);
                            }
                        }
                    }
                }
            }

            var diffModel = new MessageRule50Model
            {
                Rule = "#50",
                Message = "Check ticket count of 20000 by winlostdate in SiteDB and DBTP: Tickets are mismatching between DBTP and SiteDB, between SiteDB and DB052, DB053, DB061, DB062",
                SPName = "DBA_Consistency_CheckMBUTicketCount, SiteDB_DBA_Consistency_CheckMBUTicketCount",
                MBUData = diffRows
            };

            if (diffModel.MBUData.Count > 0)
            {
                result.Add(diffModel);
            }

            return result;
        }
    }
}