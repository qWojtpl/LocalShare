
using LocalShareCommunication.Client;
using LocalShareCommunication.Server;

namespace LocalShareApplication.Misc.History;

public static class HistoryManager
{

    public static void AddToHistory(FileProcess process)
    {
        SettingsManager.History += CreateHistoryString(HistoryType.Download, process.FileName);
    }

    public static void AddToHistory(FileSendProcess process)
    {
        SettingsManager.History += CreateHistoryString(HistoryType.Upload, process.FileName);
    }

    private static string CreateHistoryString(HistoryType type, string fileName)
    {
        return (int) type + fileName + "?" + DateTimeOffset.Now.ToUnixTimeSeconds() + "/";
    }

    public static List<HistoryObject> GetHistoryObjects()
    {
        List<HistoryObject> list = new List<HistoryObject>();
        foreach(string obj in SettingsManager.History.Split("/"))
        {
            if(obj.Equals(""))
            {
                continue;
            }
            HistoryType type = (HistoryType) int.Parse(obj.Substring(0, 1));
            string[] split = obj.Substring(1).Split("?");
            list.Add(new HistoryObject(type, split[0], long.Parse(split[1])));
        }
        list.Reverse();
        return list;
    }

}
