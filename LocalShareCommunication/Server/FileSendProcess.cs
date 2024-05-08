
namespace LocalShareCommunication.Server;

public class FileSendProcess : IDisposable
{

    public string Key { get; }
    public string Path { get; }
    public string FileName { get; }
    public long FileSize { get; }
    public FileStream Reader { get; }
    public long LastRequest { get; set; }

    public FileSendProcess(string key, string path)
    {
        Key = key;
        Path = path;
        var fileInfo = new FileInfo(path);
        FileName = fileInfo.Name;
        FileSize = fileInfo.Length;
        Reader = File.OpenRead(path);
    }

    public void Dispose()
    {
        Reader.Close();
    }
}
