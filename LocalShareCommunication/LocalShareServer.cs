using LocalShareCommunication.Misc;
using LocalShareCommunication.Packets;
using LocalShareCommunication.Server;

namespace LocalShareCommunication;

public class LocalShareServer
{

    private readonly PacketSender _packetSender;
    private readonly PacketListener _packetListener;
    public int Port { get; }
    public int CallbackPort { get; }
    private Dictionary<string, FileSendProcess> keyFiles = new();

    public LocalShareServer(int port = 2780, int callbackPort = 2781)
    {
        Port = port;
        CallbackPort = callbackPort;
        _packetSender = new PacketSender(port);
        _packetListener = new PacketListener(callbackPort, HandleRequest);
    }

    public void Start()
    {
        _packetListener.StartListener();
        SendFile("Heartbeat_Connection.mp4");
    }

    private void HandleRequest(Packet packet)
    {
        if (!keyFiles.ContainsKey(packet.Key))
        {
            return;
        }

        FileSendProcess process = keyFiles[packet.Key];

        if(PacketType.FileName.Equals(packet.Type))
        {
            SendFileNamePacket(process);
        } else if(PacketType.FileSize.Equals(packet.Type))
        {
            SendFileSizePacket(process);
        } else if(PacketType.Byte.Equals(packet.Type))
        {
            SendFilePacket(process, packet.Identifier);
        }

    }
    
    public void SendFile(string path)
    {
        new Thread(() =>
        {
            CheckFileSize(path);
            string key = KeyGenerator.GenerateKey();
            FileSendProcess process = new FileSendProcess(key, path);
            keyFiles[key] = process;
            SendFileNamePacket(process);
        }).Start();
    }

    private void CheckFileSize(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException(path);
        }
        long fileSize = fileInfo.Length;
        if (fileSize / Shared.MaxDataSize + 1 >= Math.Pow(10, Shared.PacketIdentifierLength))
        {
            Console.WriteLine("Overall file size is too big. Consider changing MaxDataSize.");
            return;
        }
    }

    private void SendFileNamePacket(FileSendProcess process)
    {
        SendData(PacketType.FileName, process.Key, 0, EncodingManager.GetBytes(process.FileName));
    }

    private void SendFileSizePacket(FileSendProcess process)
    {
        SendData(PacketType.FileSize, process.Key, 0, EncodingManager.GetBytes(process.FileSize + ""));
    }

    private void SendFilePacket(FileSendProcess process, long identifier)
    {
        long bufferSize = Shared.MaxDataSize;
        if ((identifier + 1) * Shared.MaxDataSize > process.FileSize)
        {
            bufferSize = process.FileSize - identifier * Shared.MaxDataSize;
        }
        if (bufferSize <= 0)
        {
            return;
        }
        byte[] buffer = new byte[bufferSize];
        process.Reader.Position = (int) identifier * Shared.MaxDataSize;
        process.Reader.Read(buffer, 0, buffer.Length);
        SendData(PacketType.Byte, process.Key, identifier, buffer);
        process.LastRequest = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private void SendData(PacketType packetType, string key, long identifier, byte[] data)
    {
        _packetSender.SendData(packetType, key, identifier, data);
    }

}
