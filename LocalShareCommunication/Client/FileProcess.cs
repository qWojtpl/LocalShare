﻿
namespace LocalShareCommunication.Client;

public class FileProcess
{

    public bool Accepted { get; set; }
    public bool Closed { get; set; }
    public string Key { get; }
    public bool Running { get; set; }
    public FileStream? Writer { get; set; }
    public string? FileName { get; set; }
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
    private string? chunksPath;

    public FileProcess(string key)
    {
        Key = key;
    }

    private void CreateChunks()
    {
        ChunkSize = (int) Math.Ceiling((decimal) FileSize / Shared.MaxDataSize / Shared.GoalChunkCount);
        chunksPath = CreateChunksPath();
        for(int i = 0; i < Shared.GoalChunkCount; i++)
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
        string chunkDirectory = Shared.FilesPath + "chunks_" + FileName;
        if (Directory.Exists(chunkDirectory))
        {
            Directory.Delete(chunkDirectory, true);
        }
        Directory.CreateDirectory(chunkDirectory);
        return chunkDirectory;
    }
    
    public void CloseChunkWriters()
    {
        Closed = true;
        Console.WriteLine("Closing chunk writers...");
        foreach (Chunk chunk in Chunks)
        {
            Console.WriteLine("Closing writer " + chunk.Id);
            chunk.Writer.Close();
            Thread.Sleep(1);
        }
    }

    public void MergeChunks()
    {
        if (chunksPath == null)
        {
            throw new Exception("Chunks path is null!");
        }
        Console.WriteLine("Merging chunks...");
        if (File.Exists(Shared.FilesPath + FileName))
        {
            FileName = "_" + FileName;
            MergeChunks();
            return;
        }
        Writer = File.OpenWrite(Shared.FilesPath + FileName);
        foreach(Chunk chunk in Chunks)
        {
            Console.WriteLine("Merging chunk " + chunk.Id);
            try
            {
                byte[] bytes = File.ReadAllBytes(chunk.ChunkPath);
                Writer.Write(bytes, 0, bytes.Length);
            }
            catch (Exception)
            {
                continue;
            }
        }
        Directory.Delete(chunksPath, true);
        Console.WriteLine("File transfer complete.");
        Writer.Close();
    }

}
