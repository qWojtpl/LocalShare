using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClientServerTest.Misc;
using ClientServerTest.Packets;

namespace ClientServerTest;

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
        SendFile("Heartbeat_Connection.mp4");
    }

    private void HandleRequest(Packet packet)
    {

        if (!keyFiles.ContainsKey(packet.Key))
        {
            return;
        }

        SendFilePacket(packet.Key, keyFiles[packet.Key], packet.Identifier);
    }
    
    public void SendFile(string path)
    {
        new Thread(() =>
        {
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(path);
            }
            string key = KeyGenerator.GenerateKey();
            long fileSize = fileInfo.Length;
            if (fileSize / Shared.MaxDataSize + 1 >= Math.Pow(10, Shared.PacketIdentifierLength))
            {
                Console.WriteLine("Overall file size is too big. Consider changing MaxDataSize.");
                return;
            }
            keyFiles[key] = path;
            SendData(PacketType.FileName, key, 0, EncodingManager.GetBytes(fileInfo.Name));
            SendData(PacketType.FileSize, key, 0, EncodingManager.GetBytes(fileSize + ""));
            SendFilePacket(key, path, 0);
        }).Start();
    }

    private void SendFilePacket(string key, string path, long identifier)
    {
        long fileSize = new FileInfo(path).Length;
        using (FileStream stream = File.OpenRead(path))
        {
            long bufferSize = Shared.MaxDataSize;
            if(identifier * Shared.MaxDataSize > fileSize)
            {
                bufferSize = identifier * Shared.MaxDataSize - fileSize;
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
