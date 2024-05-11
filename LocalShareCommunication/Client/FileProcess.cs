
namespace LocalShareCommunication.Client;

public class FileProcess
{

    public bool Accepted { get; set; }
    public bool Closed { get; set; }
    public string Key { get; }
    public bool Running { get; set; }
    public FileStream Writer { get; set; }
    public string? FileName { get; set; }
    public int FileSize { get; set; }
    public int ActualSize { get; set; } = 0;
    public long LastPacket { get; set; }

    public FileProcess(string key)
    {
        Key = key;
    }

}
