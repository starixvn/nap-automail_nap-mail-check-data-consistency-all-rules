using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class ConsistencyRule51Criteria : CriteriaBase
{
    public override string GetSettingKey() => "DBA_Consistency_AutoTransfer_Check_TotalTicketCorrect";

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override bool IsValid() => true;
}