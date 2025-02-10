using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule46Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SPName { get; set; }

    public DateTime TimeJobExecute { get; set; }

    public Dictionary<string, ConsistencyCheckModelRule46> Data { get; set; } = [];
}