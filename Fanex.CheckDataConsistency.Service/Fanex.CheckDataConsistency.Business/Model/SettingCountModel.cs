namespace Fanex.CheckDataConsistency.Business.Model;

public class SettingCountModel
{
    public string TableName { get; set; }

    public string Site { get; set; }

    public int RowCount { get; set; }

    public long CHECKSUM_Value { get; set; }
}