using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule4849Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SPName { get; set; }

    public Dictionary<string, ConsistencyCheckModel> Data { get; set; }

    public List<Rule4849FromToModel> FromToDates { get; set; } = new List<Rule4849FromToModel>();

    public bool IsSeletTop { get; set; }
}