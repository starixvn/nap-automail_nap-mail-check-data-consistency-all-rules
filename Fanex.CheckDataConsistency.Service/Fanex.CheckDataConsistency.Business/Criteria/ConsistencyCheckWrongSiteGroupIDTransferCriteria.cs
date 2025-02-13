using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class ConsistencyCheckWrongSiteGroupIDTransferCriteria : CriteriaBase
{
    public int SiteDBGroupID { get; set; }

    public long TransferLastTransId { get; set; }

    public long NewTransferLastTransId { get; set; }

    public override string GetSettingKey() => "DBA_Consistency_CheckWrongSiteGroupID_Transfer";

    public override bool IsValid() => true;
}