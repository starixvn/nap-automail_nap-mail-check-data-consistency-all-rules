using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule52Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SPName { get; set; }

    public Dictionary<string, List<ConsistencyRule52Model>> Data { get; set; } = new();
}