using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class ResultModel
{
    public Dictionary<string, CustInfoTable1Model> DictResult { get; set; } = new();

    public Dictionary<string, CustomerModel> DictResultCustomer { get; set; } = new();

    public string SMAListJson { get; set; }

    public byte[] CustListJson { get; set; }

    public int RetryCounter { get; set; }

    public List<string> RulesList { get; set; } = [];

    public List<string> CommandList { get; set; } = [];

    public DateTime CurrentTime { get; set; }

    public int MaxCust_New { get; set; }
}