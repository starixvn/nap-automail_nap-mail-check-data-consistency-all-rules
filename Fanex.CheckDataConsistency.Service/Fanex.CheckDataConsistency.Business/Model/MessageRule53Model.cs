using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule53Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SPName { get; set; }

    public Dictionary<string, Tuple<List<SiteDBGroupIDModel>, List<Rule53FromToModel>>> Data { get; set; }
}