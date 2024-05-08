
namespace LocalShareCommunication.Client;

public class Chunk
{
    
    public int Id { get; }
    public FileProcess Process { get; }
    public FileStream Writer { get; set; } 
    public string ChunkPath { get; }
    public long LastPacket { get; set; }
    public int Min { get; private set; }
    public int Max { get; private set; }

    public Chunk(int id, FileProcess fileProcess, string path)
    {
        Id = id;
        Process = fileProcess;
        ChunkPath = path + "/chunk_" + Id;
        CreateChunkFile();
        Writer = CreateWriter();
        CreateBounds();
    }

    private void CreateChunkFile()
    {
        File.Create(ChunkPath).Close();
    }

    private FileStream CreateWriter()
    {
        return File.OpenWrite(ChunkPath);
    }

    private void CreateBounds()
    {
        Min = Process.ChunkSize * Id;
        Max = Process.ChunkSize * (Id + 1);
        LastPacket = Min;
    }

}
