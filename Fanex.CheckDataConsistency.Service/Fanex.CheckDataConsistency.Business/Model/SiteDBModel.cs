namespace Fanex.CheckDataConsistency.Business.Model;

public class SiteDBModel
{
    public int SiteDBGroupID { get; set; }

    public string LinkedServerName { get; set; }

    public string AGIP { get; set; }

    public bool IsReadOnly { get; set; }
}