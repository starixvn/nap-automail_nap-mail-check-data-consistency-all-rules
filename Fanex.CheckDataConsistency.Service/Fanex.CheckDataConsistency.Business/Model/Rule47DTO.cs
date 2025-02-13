using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class Rule47DTO
{
    public ExecuteSPResult DBOutputResult { get; set; } = new();

    public string SPNameOutputResult { get; set; }

    public List<ConsistencyRule47Model> DBDataResult { get; set; } = [];

    public string SPNameDataResult { get; set; }
}