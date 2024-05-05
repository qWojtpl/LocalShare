
namespace LocalShareCommunication.Client;

public class FileProcess
{

    public string Key { get; }
    public FileStream? Writer { get; set; }
    public string? FileName { get; set; }
    public string? TempFileName { get; set; }
    public int FileSize { get; set; } = -1;
    public int ActualSize { get; set; } = 0;
    public long LastIdentifier { get; set; } = -1;


    public FileProcess(string key)
    {
        Key = key;
    }

}
