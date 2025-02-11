using System;

namespace Fanex.CheckDataConsistency.Business.Model;

public class TicketCheckingModel
{
    public int CustId { get; set; }

    public int TotalCount { get; set; }

    public DateTime WinlostDate { get; set; }
}