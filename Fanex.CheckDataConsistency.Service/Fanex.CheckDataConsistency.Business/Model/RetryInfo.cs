using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class RetryInfo
{
    public string Rule { get; set; }

    public string SiteDB { get; set; }

    public string TableName { get; set; }

    public string RecommendList { get; set; }

    public List<SMAModel> SMAList { get; set; } = [];
}