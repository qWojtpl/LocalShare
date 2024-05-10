using System.Net;
using LocalShareCommunication.Packets;
using LocalShareCommunication.Misc;
using LocalShareCommunication.Client;
using LocalShareCommunication.Events;

namespace LocalShareCommunication;

public class LocalShareClient : IDisposable
{

    private readonly PacketListener _packetListener;
    private readonly PacketSender _packetSender;
    public int Port { get; }
    public int CallbackPort { get; }
    private Dictionary<string, FileProcess> fileProcesses = new();
    private Events.EventHandler<FileProcess> _eventHandler;

    public LocalShareClient(int port = 2780, int callbackPort = 2781)
    {
        Port = port;
        CallbackPort = callbackPort;
        _packetListener = new PacketListener(port, HandlePacket);
        _packetSender = new PacketSender(callbackPort);
        _eventHandler = new Events.EventHandler<FileProcess>();
    }

    public void Start()
    {
        _packetListener.StartListener();
    }

    public void Stop()
    {
        _packetListener.StopListener();
    }

    public void AddEventHandler(Action<EventType, FileProcess> action)
    {
        _eventHandler.AddEventHandler(action);
    }

    private void HandlePacket(Packet packet)
    {
        string key = packet.Key;

        FileProcess process;

        if (!fileProcesses.ContainsKey(key))
        {
            process = new FileProcess(key);
            fileProcesses[key] = process;
            SendEvent(EventType.StartDownloading, process);
        } else
        {
            process = fileProcesses[key];
        }

        PacketType packetType = packet.Type;

        if(PacketType.FileName.Equals(packetType))
        {
            HandleFileNamePacket(process, packet);
        }
        else if(PacketType.FileSize.Equals(packetType))
        {
            HandleFileSizePacket(process, packet);
        } else if(PacketType.Byte.Equals(packetType))
        {
            HandleBytePacket(process, packet);
        }

    }

    public List<FileProcess> GetProcessesToAccept()
    {
        return fileProcesses.Values.Where(n => !n.Accepted).ToList();
    }

    public void Accept(string key)
    {
        if (!fileProcesses.ContainsKey(key))
        {
            return;
        }
        Accept(fileProcesses[key]);
    }

    public void Accept(FileProcess process)
    {
        if (process.Accepted)
        {
            return;
        }
        process.Accepted = true;
        RequestFileSizePacket(process);
        SendEvent(EventType.Accept, process);
    }

    private void HandleFileNamePacket(FileProcess process, Packet packet)
    {
        process.FileName = EncodingManager.GetText(packet.Data); 
        CreateDirectory();
        /*
         *
         * ONLY FOR DEV BUILD
         * AUTO-ACCEPTING FILES NEED TO BE REMOVED ON RELEASE!
         * 
        */
        Accept(process);
    }

    private void HandleFileSizePacket(FileProcess process, Packet packet)
    {
        if (!process.Accepted)
        {
            return;
        }
        process.FileSize = int.Parse(EncodingManager.GetText(packet.Data));
        if (!process.Running)
        {
            RunChunks(process);
        }
    }

    private void RunChunks(FileProcess process)
    {
        process.Running = true;
        for (int i = 0; i < Shared.GoalChunkCount; i++)
        {
            int tmp = i;
            new Thread(() => RunChunk(process, tmp)).Start();
        }
       // new Thread(() => StartMonitoring(process)).Start();
    }

    private void RunChunk(FileProcess process, int chunkIdentifier)
    {
        Chunk chunk = process.Chunks[chunkIdentifier];
        NextChunkRequest(process, chunk);
    }

    private void StartMonitoring(FileProcess process)
    {
        if (!fileProcesses.ContainsKey(process.Key))
        {
            return;
        }
        Console.Clear();
        Console.WriteLine("Total chunks: " + process.Chunks.Count);
        foreach (Chunk chunk in process.Chunks)
        {
            Console.WriteLine("Chunk " + chunk.Id + ": " + ((double) (chunk.LastPacket - chunk.Min) / (chunk.Max - chunk.Min)) * 100 + "%" + " (" + (chunk.LastPacket - chunk.Min) + "/" + (chunk.Max - chunk.Min) + ")");
        }
        Thread.Sleep(2000);
        StartMonitoring(process);
    }

    private void NextChunkRequest(FileProcess process, Chunk chunk)
    {
        if (chunk.LastPacket * Shared.MaxDataSize > process.FileSize || chunk.LastPacket == chunk.Max)
        {
            return;
        }
        RequestPacket(chunk, chunk.LastPacket);
    }

    private void HandleBytePacket(FileProcess process, Packet packet)
    {
        if (process.FileName == null || process.FileSize == -1 || !process.Accepted)
        {
            return;
        }
        long identifier = packet.Identifier;
        Chunk chunk = process.Chunks[(int) (identifier / process.ChunkSize)];
        if (chunk.LastPacket != identifier)
        {
            return;
        }

        if(chunk.Writer.CanWrite)
        {
            process.ActualSize += packet.Data.Length;
            chunk.Writer.Write(packet.Data, 0, packet.Data.Length);
            chunk.LastPacket++;
            NextChunkRequest(process, chunk);
        } else
        {
            NextChunkRequest(process, chunk);
        }

        if (process.ActualSize >= process.FileSize && !process.Closed)
        {
            process.CloseChunkWriters();
            process.MergeChunks();
            SendEvent(EventType.EndDownloading, process);
            fileProcesses.Remove(process.Key, out _);
        }

    }

    private void RequestPacket(Chunk chunk, long identifier, int sleepTime = 350)
    {
        if(identifier < chunk.LastPacket)
        {
            return;
        }
        Console.WriteLine("Requesting " + identifier + " from " + IPAddress.Broadcast + ":" + (Port + 1));
        _packetSender.SendData(PacketType.Byte, chunk.Process.Key, identifier, new byte[0]);
        new Thread(() => CheckTimeout(chunk, identifier, sleepTime)).Start();
    }

    private void CheckTimeout(Chunk chunk, long identifier, int sleepTime)
    {
        Thread.Sleep(sleepTime);
        if (chunk.LastPacket <= identifier && chunk.LastPacket != chunk.Max)
        {
            Console.WriteLine("Request timed out.");
            RequestPacket(chunk, identifier, sleepTime + 100);
        }
    }

    private void RequestFileSizePacket(FileProcess process)
    {
        _packetSender.SendData(PacketType.FileSize, process.Key, 0, new byte[0]);
    }

    private void CreateDirectory()
    {
        if(Directory.Exists(Shared.FilesPath))
        {
            return;
        }
        Directory.CreateDirectory(Shared.FilesPath);
    }

    private void SendEvent(EventType type, FileProcess process)
    {
        _eventHandler.SendEvent(type, process);
    }

    public void Dispose()
    {
        foreach(var process in fileProcesses)
        {
            fileProcesses[process.Key].Writer.Close();
        }
    }

}
