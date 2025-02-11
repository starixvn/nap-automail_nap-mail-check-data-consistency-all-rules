using Fanex.Data.Repository;
using System;

namespace Fanex.CheckDataConsistency.Business.Criteria.MailStation;

public class UpdateMailStatusCommand : NonQueryCommand
{
    public long TaskID { get; set; }

    public int Status { get; set; }

    public DateTime LastSent { get; set; } = DateTime.Now;

    public override string GetSettingKey() => "Mail_Core_UpdateMailStatus";

    public override bool IsValid() => true;
}