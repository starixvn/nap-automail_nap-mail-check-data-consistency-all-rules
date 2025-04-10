namespace Fanex.CheckDataConsistency.Business.Model;

public class CounterModel
{
    public int CounterNotExists { get; set; }

    public int CounterNotMatchRowCount { get; set; }

    public int Counter => CounterNotExists + CounterNotMatchRowCount;
}