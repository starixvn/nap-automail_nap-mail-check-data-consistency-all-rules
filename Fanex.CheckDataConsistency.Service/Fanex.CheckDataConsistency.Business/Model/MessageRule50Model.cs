using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule50Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SPName { get; set; }

    public Dictionary<string, List<TicketCheckingModel>> Data { get; set; } = new Dictionary<string, List<TicketCheckingModel>>();

    public Dictionary<string, MBUTicketCountModel> MBUData { get; set; } = new Dictionary<string, MBUTicketCountModel>();
}