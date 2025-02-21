using System;

namespace Fanex.CheckDataConsistency.Business.Model;

public class TimeModel
{
    public DateTime StartTime { get; set; } = new DateTime(1900, 01, 01);

    public DateTime EndTime { get; set; } = new DateTime(1900, 01, 01);
}