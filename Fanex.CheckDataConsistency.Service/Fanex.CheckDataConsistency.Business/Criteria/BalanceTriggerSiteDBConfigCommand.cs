using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class BalanceTriggerSiteDBConfigCommand(string spName) : NonQueryCommand
{
    public int SiteDBGroupID { get; set; }

    public bool IsTest { get; set; }

    public int Mode => 1;

    public long LastRowInserted { get; set; }

    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}