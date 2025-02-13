namespace Fanex.CheckDataConsistency.Business.Model;

public class ExecuteSPResult
{
    public int SiteDBGroupID { get; set; }

    public long TransferLastTransId { get; set; }

    public long NewTransferLastTransId { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }
}