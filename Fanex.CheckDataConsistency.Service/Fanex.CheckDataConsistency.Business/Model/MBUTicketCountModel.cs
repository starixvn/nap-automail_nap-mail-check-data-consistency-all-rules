using System;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MBUTicketCountModel
{
    public int SiteDBGroupID { get; set; }

    public DateTime WinlostDate { get; set; }

    public int TotalCount { get; set; }

    public int TotalCheckSumAgg { get; set; }
}