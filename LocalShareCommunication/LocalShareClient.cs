using System.Net;
using LocalShareCommunication.Packets;
using LocalShareCommunication.Misc;
using LocalShareCommunication.Client;

namespace LocalShareCommunication;

public class LocalShareClient : IDisposable
{

    private readonly PacketListener _packetListener;
    private readonly PacketSender _packetSender;
    public int Port { get; }

    private Dictionary<string, FileProcess> fileProcesses = new();

    public LocalShareClient(int port = 2780)
    {
        Port = port;
        _packetListener = new PacketListener(port, HandlePacket);
        _packetSender = new PacketSender(port + 1);
    }

    public void Start()
    {
        _packetListener.StartListener();
    }

    private void HandlePacket(Packet packet)
    {
        string key = packet.Key;

        FileProcess process;

        if (!fileProcesses.ContainsKey(key))
        {
            process = new FileProcess(key);
            fileProcesses[key] = process;
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

        if(process.LastIdentifier == -1)
        {
            if(process.FileName != null && process.FileSize != -1)
            {
                RequestPacket(process, 0);
            }
        }
    }

    private void HandleFileNamePacket(FileProcess process, Packet packet)
    {
        process.FileName = EncodingManager.GetText(packet.Data); 
        CreateFileWithDirectory(process.FileName);
        RequestFileSizePacket(process);
    }

    private void HandleFileSizePacket(FileProcess process, Packet packet)
    {
        process.FileSize = int.Parse(EncodingManager.GetText(packet.Data));
    }

    private void HandleBytePacket(FileProcess process, Packet packet)
    {
        long identifier = packet.Identifier;
        if(process.LastIdentifier + 1 != identifier)
        {
            return;
        }
        process.LastIdentifier = identifier;
        process.ActualSize += packet.Data.Length;
        if (process.Writer == null)
        {
            Console.WriteLine("Found filename, creating new file...");
            process.Writer = File.OpenWrite("./files/" + process.FileName);
        }
        process.Writer.Write(packet.Data, 0, packet.Data.Length);
        if (process.FileSize != -1)
        {
            if (process.ActualSize == process.FileSize)
            {
                Console.WriteLine("CLOSING!");
                
                process.Writer.Close();
                fileProcesses.Remove(process.Key, out _);
                return;
            }
            else if(process.ActualSize >= process.FileSize)
            {
                process.Writer.Close();
                throw new Exception("Sent file is too large!");
            }
        }
        RequestPacket(process, process.LastIdentifier + 1);
    }

    private void RequestPacket(FileProcess process, long identifier, int sleepTime = 350)
    {
        if(identifier <= process.LastIdentifier)
        {
            return;
        }
        Console.WriteLine("Requesting " + identifier + " from " + IPAddress.Broadcast + ":" + (Port + 1));
        _packetSender.SendData(PacketType.Byte, process.Key, identifier, new byte[0]);
        Thread.Sleep(sleepTime);
        if(!fileProcesses.ContainsKey(process.Key))
        {
            return;
        }
        if(process.LastIdentifier <= identifier)
        {
            Console.WriteLine("Request timed out.");
            RequestPacket(process, identifier, sleepTime + 100);
        }
    }

    private void RequestFileSizePacket(FileProcess process)
    {
        _packetSender.SendData(PacketType.FileSize, process.Key, 0, new byte[0]);
    }

    private void CreateFileWithDirectory(string fileName)
    {
        Directory.CreateDirectory("./files/");
        File.Create("./files/" + fileName).Close();
    }

    public void Dispose()
    {
        foreach(var process in fileProcesses)
        {
            fileProcesses[process.Key].Writer.Close();
        }
    }

}
