using LocalShareCommunication.Events;
using LocalShareCommunication.Misc;
using LocalShareCommunication.Packets;
using LocalShareCommunication.UdpService;
using System.Net.Sockets;

namespace LocalShareCommunication.Server;

public class LocalShareServer
{

    private bool _disposed = false;
    private readonly UdpCallbacker _udpCallbacker;
    private readonly PacketSender _packetSender;
    private readonly PacketListener _packetListener;
    public int Port { get; }
    public int CallbackPort { get; }
    private Dictionary<string, FileSendProcess> keyFiles = new();
    private Events.EventHandler<FileSendProcess> _eventHandler;

    public LocalShareServer(int port = 2780, int callbackPort = 2781)
    {
        Port = port;
        CallbackPort = callbackPort;
        _udpCallbacker = new UdpCallbacker(port, callbackPort);
        _packetSender = new PacketSender(_udpCallbacker, port);
        _packetListener = new PacketListener(callbackPort, HandleRequest);
        _eventHandler = new Events.EventHandler<FileSendProcess>();
    }

    public void Start()
    {
        _udpCallbacker.Start();
        _packetListener.StartListener();
    }

    public void Stop()
    {
        _udpCallbacker.Stop();
        _packetListener.StopListener();
        _disposed = true;
    }

    public void AddEventHandler(Action<EventType, FileSendProcess> action)
    {
        _eventHandler.AddEventHandler(action);
    }

    public List<FileSendProcess> GetFileSendProcesses()
    {
        return keyFiles.Values.ToList();
    }

    public void Cancel(FileSendProcess process)
    {
        if (!keyFiles.ContainsKey(process.Key))
        {
            return;
        }
        CloseProcess(process);
        SendEvent(EventType.Cancel, process);
    }

    private void CloseProcess(FileSendProcess process)
    {
        process.Reader.Close();
        keyFiles.Remove(process.Key);
    }

    private void HandleRequest(TcpClient client, Packet packet)
    {
        if (!keyFiles.ContainsKey(packet.Key))
        {
            return;
        }

        FileSendProcess process = keyFiles[packet.Key];

        if (PacketType.FileName.Equals(packet.Type))
        {
            SendFileNamePacket(process);
        }
        else if (PacketType.FileSize.Equals(packet.Type))
        {
            SendFileSizePacket(client, process);
        }
        else if (PacketType.Byte.Equals(packet.Type))
        {
            SendFilePacket(client, process, packet.Identifier);
        }

    }

    public void SendFile(string path)
    {
        new Thread(() =>
        {
            string key = KeyGenerator.GenerateKey();
            FileSendProcess process = new FileSendProcess(key, path);
            keyFiles[key] = process;
            _udpCallbacker.SendScan();
            Thread.Sleep(1000);
            SendFileNamePacket(process);
            SendEvent(EventType.StartUploading, process);
            while (!_disposed && keyFiles.ContainsKey(key))
            {
                Thread.Sleep(100);
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - process.LastRequest > Shared.UploadTimeout)
                {
                    break;
                }
            }
            CloseProcess(process);
            SendEvent(EventType.EndUploading, process);
        }).Start();
    }

    private void SendFileNamePacket(FileSendProcess process)
    {
        SendData(PacketType.FileName, process.Key, 0, EncodingManager.GetBytes(process.FileName));
    }

    private void SendFileSizePacket(TcpClient client, FileSendProcess process)
    {
        SendData(client, PacketType.FileSize, process.Key, 0, EncodingManager.GetBytes(process.FileSize + ""));
    }

    private void SendFilePacket(TcpClient client, FileSendProcess process, long identifier)
    {
        long bufferSize = Shared.MaxDataSize;
        /*        if (identifier * Shared.MaxDataSize > process.FileSize) //TODO!!!!
                {
                    bufferSize = process.FileSize - (identifier - 1) * Shared.MaxDataSize;
                }*/
        if (bufferSize <= 0)
        {
            return;
        }
        byte[] buffer = new byte[bufferSize];
        process.Reader.Position = (int) identifier * Shared.MaxDataSize;
        process.Reader.Read(buffer, 0, buffer.Length);
        SendData(client, PacketType.Byte, process.Key, identifier, buffer);
        process.LastRequest = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private void SendData(PacketType packetType, string key, long identifier, byte[] data)
    {
        _packetSender.SendData(packetType, key, identifier, data);
    }

    private void SendData(TcpClient client, PacketType packetType, string key, long identifier, byte[] data)
    {
        _packetSender.SendData(client, packetType, key, identifier, data);
    }

    private void SendEvent(EventType type, FileSendProcess process)
    {
        _eventHandler.SendEvent(type, process);
    }

}
