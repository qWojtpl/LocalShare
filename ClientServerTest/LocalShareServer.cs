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

public class LocalShareServer : IDisposable
{

    private readonly UdpClient _udpClient;
    private readonly PacketSender _packetSender;
    private readonly UdpClient _requestsClient;
    private bool _disposed = false;
    public int Port { get; }
    private Dictionary<string, string> keyFiles = new();

    public LocalShareServer(int port = 2780)
    {
        Port = port;
        _udpClient = new UdpClient();
        _udpClient.EnableBroadcast = true;
        _packetSender = new PacketSender(_udpClient, port);
        _requestsClient = new UdpClient(port + 1);
    }

    public void Start()
    {
        Task.Run(() => SendFile("Heartbeat_Connection.mp4"));
        Task.Run(StartRequestsListener);
    }

    private void StartRequestsListener()
    {
        while(!_disposed)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port + 1);
            byte[] responseData = _requestsClient.Receive(ref remoteEP);

            Task.Run(() => HandleRequest(responseData));
        }
    }

    private void HandleRequest(byte[] responseData)
    {
        if (responseData.Length != Shared.HeaderLength)
        {
            return;
        }

        Packet packet = new Packet(responseData);

        if (!keyFiles.ContainsKey(packet.Key))
        {
            return;
        }

        SendFilePacket(packet.Key, keyFiles[packet.Key], packet.Identifier);
    }

    private void SendText(string key, string text)
    {
        SendData(PacketType.Text, key, 0, EncodingManager.GetBytes(text));
    }
    
    public void SendFile(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException(path);
        }
        string key = KeyGenerator.GenerateKey();
        long fileSize = fileInfo.Length;
        if(fileSize / Shared.MaxDataSize + 1 >= Math.Pow(10, Shared.PacketIdentifierLength))
        {
            Console.WriteLine("Overall file size is too big. Consider changing MaxDataSize.");
            return;
        }
        keyFiles[key] = path;
        SendData(PacketType.FileName, key, 0, EncodingManager.GetBytes(fileInfo.Name));
        SendData(PacketType.FileSize, key, 0, EncodingManager.GetBytes(fileSize + ""));
        SendFilePacket(key, path, 0);
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

    public void Dispose()
    {
        _disposed = true;
        _udpClient.Close();
        _requestsClient.Close();
    }

}
