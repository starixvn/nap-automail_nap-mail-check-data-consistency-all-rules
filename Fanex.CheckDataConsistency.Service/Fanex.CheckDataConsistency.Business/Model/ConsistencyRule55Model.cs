namespace Fanex.CheckDataConsistency.Business.Model;

public class ConsistencyRule55Model
{
    public string TableName { get; set; }

    public long TransID { get; set; }

    public int CustID { get; set; }

    public int SourceSiteDBGroupID { get; set; }

    public int CustomerSiteDBGroupID { get; set; }
}