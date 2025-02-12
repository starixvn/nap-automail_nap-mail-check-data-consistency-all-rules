using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule47Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SPName { get; set; }

    public string DB { get; set; }

    public Dictionary<string, ConsistencyRule47Model> Data { get; set; }
}