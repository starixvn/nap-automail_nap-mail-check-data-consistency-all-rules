namespace Fanex.CheckDataConsistency.Business.Model;

public class CustInfoTable1Model
{
    public string TableName { get; set; } = string.Empty;

    public string Recommend { get; set; }

    public string Mrecommend { get; set; }

    public string Srecommend { get; set; }

    public string CustId { get; set; }

    public int RowCount { get; set; }

    public long CHECKSUM_Value { get; set; }
}