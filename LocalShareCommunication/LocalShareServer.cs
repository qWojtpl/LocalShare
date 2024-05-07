using LocalShareCommunication.Misc;
using LocalShareCommunication.Packets;

namespace LocalShareCommunication;

public class LocalShareServer
{

    private readonly PacketSender _packetSender;
    private readonly PacketListener _packetListener;
    public int Port { get; }
    private Dictionary<string, string> keyFiles = new();

    public LocalShareServer(int port = 2780)
    {
        Port = port;
        _packetSender = new PacketSender(port);
        _packetListener = new PacketListener(port + 1, HandleRequest);
    }

    public void Start()
    {
        _packetListener.StartListener();
        SendFile("movie.mp4");
    }

    private void HandleRequest(Packet packet)
    {

        if (!keyFiles.ContainsKey(packet.Key))
        {
            return;
        }

        if(PacketType.FileName.Equals(packet.Type))
        {
            SendFileNamePacket(packet.Key);
        } else if(PacketType.FileSize.Equals(packet.Type))
        {
            SendFileSizePacket(packet.Key);
        } else if(PacketType.Byte.Equals(packet.Type))
        {
            SendFilePacket(packet.Key, keyFiles[packet.Key], packet.Identifier);
        }

    }
    
    public void SendFile(string path)
    {
        new Thread(() =>
        {
            CheckFileSize(path);
            string key = KeyGenerator.GenerateKey();
            keyFiles[key] = path;
            SendFileNamePacket(key);
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

    private void SendFileNamePacket(string key)
    {
        FileInfo fileInfo = new FileInfo(keyFiles[key]);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException(keyFiles[key]);
        }
        SendData(PacketType.FileName, key, 0, EncodingManager.GetBytes(fileInfo.Name));
    }

    private void SendFileSizePacket(string key)
    {
        FileInfo fileInfo = new FileInfo(keyFiles[key]);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException(keyFiles[key]);
        }
        SendData(PacketType.FileSize, key, 0, EncodingManager.GetBytes(fileInfo.Length + ""));
    }

    private void SendFilePacket(string key, string path, long identifier)
    {
        long fileSize = new FileInfo(path).Length;
        using (FileStream stream = File.OpenRead(path))
        {
            long bufferSize = Shared.MaxDataSize;
            if((identifier + 1) * Shared.MaxDataSize > fileSize)
            {
                bufferSize = fileSize - identifier * Shared.MaxDataSize;
            }
            byte[] buffer = new byte[bufferSize];
            stream.Seek(identifier * Shared.MaxDataSize, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);
            SendData(PacketType.Byte, key, identifier, buffer);
        }
    }

    private void SendData(PacketType packetType, string key, long identifier, byte[] data)
    {
        _packetSender.SendData(packetType, key, identifier, data);
    }

}
