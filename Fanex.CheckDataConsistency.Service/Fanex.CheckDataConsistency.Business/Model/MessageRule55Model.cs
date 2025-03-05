using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class MessageRule55Model
{
    public string Rule { get; set; }

    public string Message { get; set; }

    public string SPName { get; set; }

    public string DB { get; set; }

    public List<ConsistencyRule55Model> Data { get; set; }

    public List<FromTOSeqID> SeqIds { get; set; }
}