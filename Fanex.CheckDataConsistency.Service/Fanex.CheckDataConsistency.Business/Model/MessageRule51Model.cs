using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule51Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SPName { get; set; }

    public List<ConsistencyRule51Model> Data { get; set; }
}