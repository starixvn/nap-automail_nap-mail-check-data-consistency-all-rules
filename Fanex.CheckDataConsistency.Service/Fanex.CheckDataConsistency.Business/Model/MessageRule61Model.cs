using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule61Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SpName { get; set; }

    public DateTime TimeJobExecute { get; set; }

    public Dictionary<string, string> Data { get; set; }
}