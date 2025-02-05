using Fanex.Data.Repository;
using System;

namespace Fanex.CheckDataConsistency.Business.Criterias.MailStation;

public class GetMailTaskListCriteria : CriteriaBase
{
    public string MailName { get; set; }
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public override string GetSettingKey() => "Mail_Report_GetMailTaskList";

    public override bool IsValid() => true;
}