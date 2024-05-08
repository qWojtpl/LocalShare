
using System.Diagnostics;
using System.IO;

namespace LocalShareCommunication.Client;

public class FileProcess
{

    public bool Accepted { get; set; }
    public string Key { get; }
    public bool Running { get; set; }
    public FileStream? Writer { get; set; }
    public string? FileName { get; set; }
    public string? TempFileName { get; set; }
    public int FileSize 
    { 
        get => _fileSize;
        set {
            _fileSize = value;
            CreateChunks();
        }
    }
    private int _fileSize = -1;
    public int ActualSize { get; set; } = 0;
    public long LastIdentifier { get; set; } = -1;
    public List<Chunk> Chunks { get; } = new List<Chunk>();
    public int ChunkSize { get; private set; }
    public int TotalChunks { get; private set; }
    private string chunksPath;

    public FileProcess(string key)
    {
        Key = key;
    }

    private void CreateChunks()
    {
        /* todo: better calculation */
        ChunkSize = (int) Math.Ceiling((decimal) FileSize / Shared.MaxDataSize / Shared.GoalChunkCount);
        chunksPath = CreateChunksPath();
        TotalChunks = Shared.GoalChunkCount;
        for(int i = 0; i < TotalChunks; i++)
        {
            Chunks.Add(new Chunk(i, this, chunksPath));
        }
    }

    private string CreateChunksPath()
    {
        if (FileName == null)
        {
            throw new Exception("File name is null!");
        }
        string chunkDirectory = "./files/chunks_" + FileName;
        if (Directory.Exists(chunkDirectory))
        {
            Directory.Delete(chunkDirectory, true);
        }
        Directory.CreateDirectory(chunkDirectory);
        return chunkDirectory;
    }

    public void CloseChunkWriters()
    {
        Console.WriteLine("Closing chunk writers...");
        foreach (Chunk chunk in Chunks)
        {
            Console.WriteLine("Closing writer " + chunk.Id);
            chunk.Writer.Close();
        }
    }

    public void MergeChunks()
    {
        Console.WriteLine("Merging chunks...");
        if (File.Exists("./files/" + FileName))
        {
            FileName = "_" + FileName;
            MergeChunks();
            return;
        }
        Writer = File.OpenWrite("./files/" + FileName);
        foreach(Chunk chunk in Chunks)
        {
            Console.WriteLine("Merging chunk " + chunk.Id);
            byte[] bytes = File.ReadAllBytes(chunk.ChunkPath);
            Writer.Write(bytes, 0, bytes.Length);
        }
        Directory.Delete(chunksPath, true);
        Console.WriteLine("File transfer complete.");
        Writer.Close();
    }

}
