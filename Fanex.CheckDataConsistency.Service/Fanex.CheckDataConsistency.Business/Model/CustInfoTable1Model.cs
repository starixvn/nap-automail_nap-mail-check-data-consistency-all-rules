namespace Fanex.CheckDataConsistency.Business.Model;

public class CustInfoTable1Model
{
    public string TableName { get; set; } = string.Empty;

    public long Recommend { get; set; }

    public long Mrecommend { get; set; }

    public long Srecommend { get; set; }

    public long CustId { get; set; }

    public int RowCount { get; set; }

    public long CHECKSUM_Value { get; set; }
}