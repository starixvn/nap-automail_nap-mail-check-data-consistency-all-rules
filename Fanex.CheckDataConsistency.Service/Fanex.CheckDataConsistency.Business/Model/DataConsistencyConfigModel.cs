namespace Fanex.CheckDataConsistency.Business.Model;

public class DataConsistencyConfigModel
{
    public string TableName { get; set; }

    public int RuleNumber { get; set; }

    public string SpName { get; set; }

    public int GroupId { get; set; }

    public string VValue { get; set; }
}