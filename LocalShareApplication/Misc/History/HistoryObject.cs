
namespace LocalShareApplication.Misc.History;

public class HistoryObject
{

    public HistoryType Type { get; }
    public string Name { get; }
    public string Date { get; }

    public HistoryObject(HistoryType type, string name, long unix)
    {
        Type = type;
        Name = name;
        Date = DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().ToString().Substring(0, 19);
    }
}
