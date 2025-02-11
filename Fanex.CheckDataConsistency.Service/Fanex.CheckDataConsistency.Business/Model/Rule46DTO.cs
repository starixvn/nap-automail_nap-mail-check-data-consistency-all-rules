using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class Rule46DTO
{
    public List<ConsistencyCheckModelRule46> SiteDBDataResult { get; set; } = [];

    public ExecuteSPResultRule46 SiteDBOutputResult { get; set; } = new();

    public List<ConsistencyCheckModelRule46> CentralDBDataResult { get; set; } = [];

    public Dictionary<string, List<ConsistencyCheckModelRule46>> Db05Xdb06XDataResult { get; set; } = new();

    public string SPNameOutputResult { get; set; }

    public string SPNameDataResult { get; set; }
}