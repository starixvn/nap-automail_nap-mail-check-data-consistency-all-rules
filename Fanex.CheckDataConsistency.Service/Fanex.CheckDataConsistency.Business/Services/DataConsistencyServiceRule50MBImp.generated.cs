using Fanex.CheckDataConsistency.Business.Model;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services
{
    public class DataConsistencyServiceRule50MBImp : DataConsistencyRule50Service
    {
        private readonly DataService dataService;

        public DataConsistencyServiceRule50MBImp(DataService dataService)
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
            var dataConsistency = dataService.SelectConsistencyCheckDuplicatedMoveBetTicket();
            var dataSiteDBs = dataService.SelectSiteDBConsistencyCheckDuplicatedMoveBetTicket();
            var dbx = dataService.SelectDB05XDB06XConsistencyCheckDuplicatedMoveBetTicket();

            if (dataConsistency.Count > 0)
            {
                result.Add(new MessageRule50Model
                {
                    Rule = "#50",
                    Message = "Ticket 19999: alert if exist custid having > 1 row of 19999 by winlostdate = current movebet date in SiteDB and CentralDB (check in DBTP)," +
                    " run after movebet process done (web call)",
                    SPName = "DBA_Consistency_CheckDuplicatedMoveBetTicket",
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
                        Message = "Ticket 19999: alert if exist custid having > 1 row of 19999 by winlostdate = current movebet date in SiteDB and CentralDB (check in DBTP)," +
                        " run after movebet process done (web call)",
                        SPName = "DBA_Consistency_CheckDuplicatedMoveBetTicket",
                        Data = new Dictionary<string, List<TicketCheckingModel>>
                        {
                            { siteDB, dataSiteDB.Value }
                        }
                    });
                }
            }

            foreach (var db in dbx)
            {
                if (db.Value.Count > 0)
                {
                    result.Add(new MessageRule50Model
                    {
                        Rule = "#50",
                        Message = $"Ticket 19999: alert if exist custid having > 1 row of 19999 by winlostdate = current movebet date in SiteDB and {db.Key}," +
                                  " run after movebet process done (web call)",
                        SPName = "DBA_Consistency_CheckDuplicatedMoveBetTicket",
                        Data = new Dictionary<string, List<TicketCheckingModel>>
                        {
                            { db.Key, db.Value }
                        }
                    });
                }
            }

            return result;
        }

        private List<MessageRule50Model> CheckMBUTicketCount()
        {
            var result = new List<MessageRule50Model>();
            var dataConsistency = dataService.SelectConsistencyCheckMBUTicketCount(19999);
            var dataSiteDBs = dataService.SelectSiteDBConsistencyCheckMBUTicketCount(19999);
            var db05Xdb06X = dataService.SelectDB05XDB06XConsistencyCheckMBUTicketCount(19999);

            var notExistsRows = new Dictionary<string, MBUTicketCountModel>();
            var diffRows = new Dictionary<string, MBUTicketCountModel>();
            var notExistsIndex = 0;
            int diffIndex = 0;

            foreach (var dataSiteDB in dataSiteDBs)
            {
                var siteDB = dataSiteDB.Key.Split('_')[1];
                dataConsistency.ForEach(consisRow =>
                {
                    var siteDBRow = dataSiteDB.Value.FirstOrDefault(s => s.SiteDBGroupID == consisRow.SiteDBGroupID
                    && s.WinlostDate == consisRow.WinlostDate);

                    if (siteDBRow != null && (consisRow.TotalCount != siteDBRow.TotalCount || consisRow.TotalCheckSumAgg != siteDBRow.TotalCheckSumAgg))
                    {
                        diffIndex++;
                        diffRows.Add($"{diffIndex}-{DB.DBCONSISTENCY}", consisRow);
                        diffRows.Add($"{diffIndex}-{siteDB}", siteDBRow);
                    }
                });

                dataSiteDB.Value.ForEach(siteDBRow =>
                {
                    var consisRow = dataConsistency.FirstOrDefault(s => s.SiteDBGroupID == siteDBRow.SiteDBGroupID
                    && s.WinlostDate == siteDBRow.WinlostDate);

                    if (consisRow == null)
                    {
                        notExistsIndex++;
                        notExistsRows.Add($"{notExistsIndex}-{siteDB}", siteDBRow);
                        notExistsRows.Add($"{notExistsIndex}-{DB.DBCONSISTENCY}", consisRow);
                    }
                });
            }

            foreach (var dbX in db05Xdb06X)
            {
                foreach (var dataSiteDB in dataSiteDBs)
                {
                    var siteDB = dataSiteDB.Key.Split('_')[1];
                    dbX.Value.ForEach(dbXRow =>
                    {
                        var siteDBRow = dataSiteDB.Value.FirstOrDefault(s => s.SiteDBGroupID == dbXRow.SiteDBGroupID
                                                                             && s.WinlostDate == dbXRow.WinlostDate);

                        if (siteDBRow != null && (dbXRow.TotalCount != siteDBRow.TotalCount || dbXRow.TotalCheckSumAgg != siteDBRow.TotalCheckSumAgg))
                        {
                            diffIndex++;
                            diffRows.Add($"{diffIndex}-{dbX.Key}", dbXRow);
                            diffRows.Add($"{diffIndex}-{siteDB}", siteDBRow);
                        }
                    });

                    dataSiteDB.Value.ForEach(siteDBRow =>
                    {
                        var dbXRow = dbX.Value.FirstOrDefault(s => s.SiteDBGroupID == siteDBRow.SiteDBGroupID
                                                                && s.WinlostDate == siteDBRow.WinlostDate);

                        if (dbXRow == null)
                        {
                            notExistsIndex++;
                            notExistsRows.Add($"{notExistsIndex}-{siteDB}", siteDBRow);
                            notExistsRows.Add($"{notExistsIndex}-{dbX.Key}", dbXRow);
                        }
                    });
                }
            }

            var notExistsModel = new MessageRule50Model
            {
                Rule = "#50",
                Message = "Check ticket count of 19999 by winlostdate in SiteDB and DBTP: Tickets are mismatching between DBTP and SiteDB, between SiteDB and DB052, DB053, DB061, DB062",
                SPName = "DBA_Consistency_CheckMBUTicketCount, SiteDB_DBA_Consistency_CheckMBUTicketCount",
                MBUData = notExistsRows
            };

            var diffModel = new MessageRule50Model
            {
                Rule = "#50",
                Message = "Check ticket count of 19999 by winlostdate in SiteDB and DBTP: Tickets are mismatching between DBTP and SiteDB, between SiteDB and DB052, DB053, DB061, DB062",
                SPName = "DBA_Consistency_CheckMBUTicketCount, SiteDB_DBA_Consistency_CheckMBUTicketCount",
                MBUData = diffRows
            };

            if (notExistsModel.MBUData.Count > 0)
            {
                result.Add(notExistsModel);
            }

            if (diffModel.MBUData.Count > 0)
            {
                result.Add(diffModel);
            }

            return result;
        }
    }
}