namespace Fanex.CheckDataConsistency.Business.Model;

public class ConsistencyCheckModel
{
    public string TableName { get; set; } = string.Empty;

    public int SiteDBGroupID { get; set; }

    public string Recommend { get; set; }

    public string Mrecommend { get; set; }

    public string Srecommend { get; set; }

    public string Status { get; set; }

    public int TicketCount { get; set; }

    public int TotalCheckSumAgg { get; set; }

    public string DisplayColumnName { get; set; }

    public int DisplayColumnValue { get; set; }
}