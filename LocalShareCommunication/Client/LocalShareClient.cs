using System.Net;
using LocalShareCommunication.Packets;
using LocalShareCommunication.Misc;
using LocalShareCommunication.Events;

namespace LocalShareCommunication.Client;

public class LocalShareClient : IDisposable
{

    private bool stopped = false;
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
        stopped = true;
    }

    public void AddEventHandler(Action<EventType, FileProcess> action)
    {
        _eventHandler.AddEventHandler(action);
    }

    private void HandlePacket(Packet packet)
    {
        string key = packet.Key;

        FileProcess process;

        PacketType packetType = packet.Type;

        if (!fileProcesses.ContainsKey(key))
        {
            if(PacketType.FileName.Equals(packetType))
            {
                process = CreateProcess(packet);
            } else
            {
                return;
            }
        } else
        {
            process = fileProcesses[key];
        }

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
        if (File.Exists(Shared.FilesPath + process.FileName))
        {
            process.FileName = "_" + process.FileName;
            Accept(process);
            return;
        }
        process.Accepted = true;
        process.Writer = File.OpenWrite(Shared.FilesPath + process.FileName);
        RequestFileSizePacket(process);
        SendEvent(EventType.Accept, process);
    }

    public void Decline(string key)
    {
        if (!fileProcesses.ContainsKey(key))
        {
            return;
        }
        Decline(fileProcesses[key]);
    }

    public void Decline(FileProcess process)
    {
        if (process.Accepted)
        {
            return;
        }
        fileProcesses.Remove(process.Key);
        SendEvent(EventType.Decline, process);
    }

    public void Cancel(FileProcess process)
    {
        if (!fileProcesses.ContainsKey(process.Key))
        {
            return;
        }
        CloseProcess(process);
        File.Delete(process.Writer.Name);
        SendEvent(EventType.Cancel, process);
    }

    private void CloseProcess(FileProcess process)
    {
        process.Closed = true;
        process.Writer.Close();
        fileProcesses.Remove(process.Key);
    }

    private FileProcess CreateProcess(Packet packet)
    {
        FileProcess process = new FileProcess(packet.Key);
        fileProcesses[packet.Key] = process;
        new Thread(() =>
        {
            for(int i = 0; i < 1000 && fileProcesses.ContainsKey(packet.Key) && !stopped; i++)
            {
                Thread.Sleep(Shared.UploadTimeout - 2);
            }
            Decline(process);
        }).Start();
        return process;
    }

    private void HandleFileNamePacket(FileProcess process, Packet packet)
    {
        process.FileName = EncodingManager.GetText(packet.Data); 
        CreateDirectory();
        SendEvent(EventType.StartDownloading, process);
        // Only for debugging
        //Accept(process);
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
            RequestPacket(process, process.LastPacket);
        }
    }

    private void HandleBytePacket(FileProcess process, Packet packet)
    {
        if (process.FileName == null || process.FileSize == -1 || !process.Accepted)
        {
            return;
        }
        long identifier = packet.Identifier;

        if (process.LastPacket != identifier)
        {
            return;
        }

        if(process.Writer.CanWrite)
        {
            process.ActualSize += packet.Data.Length;
            process.Writer.Write(packet.Data, 0, packet.Data.Length);
            process.LastPacket++;
            RequestPacket(process, process.LastPacket);
        }

        if (process.ActualSize >= process.FileSize && !process.Closed)
        {
            CloseProcess(process);
            SendEvent(EventType.EndDownloading, process);
        } else
        {
            Console.WriteLine((double) process.ActualSize / process.FileSize);
            Console.WriteLine(process.ActualSize + "/" + process.FileSize);
        }

    }

    private void RequestPacket(FileProcess process, long identifier, int sleepTime = 1000)
    {
        if(identifier < process.LastPacket)
        {
            return;
        }
        Console.WriteLine("Requesting " + identifier + " from " + IPAddress.Broadcast + ":" + (Port + 1));
        _packetSender.SendData(PacketType.Byte, process.Key, identifier, new byte[0]);
        new Thread(() => CheckTimeout(process, identifier, sleepTime)).Start();
    }

    private void CheckTimeout(FileProcess process, long identifier, int sleepTime)
    {
        Thread.Sleep(sleepTime);
        if (process.LastPacket == identifier && !process.Closed)
        {
            Console.WriteLine($"Request timed out. ({identifier})");
            RequestPacket(process, identifier, sleepTime + 100);
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
