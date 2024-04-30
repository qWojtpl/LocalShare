using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTest;

public class LocalShareServer : IDisposable
{

    private readonly UdpClient _udpClient;
    private readonly UdpClient _requestsClient;
    private bool _disposed = false;
    public int Port { get; }
    private Dictionary<string, string> keyFiles = new();

    public LocalShareServer(int port = 2780)
    {
        Port = port;
        _udpClient = new UdpClient();
        _udpClient.EnableBroadcast = true;
        _requestsClient = new UdpClient(port + 1);
    }

    public async Task Start()
    {
        SendFile("Heartbeat_Connection.mp4");
        StartRequestsListener();
        //SendFile("test.txt");
    }

    private async Task StartRequestsListener()
    {
        while(!_disposed)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port + 1);
            byte[] responseData = _requestsClient.Receive(ref remoteEP);

            await Console.Out.WriteLineAsync("RECEIVED REQUEST PACKET");

            if (responseData.Length != Shared.KeyLength + Shared.PacketIdentifierLength)
            {
                return;
            }

            byte[] keyBytes = new byte[Shared.KeyLength];
            byte[] identifierBytes = new byte[Shared.PacketIdentifierLength];

            for(int i = 0; i < keyBytes.Length; i++)
            {
                keyBytes[i] = responseData[i];
            }

            string key = GetString(keyBytes);

            if (!keyFiles.ContainsKey(key))
            {
                continue;
            }

            for(int i = 0; i < Shared.PacketIdentifierLength; i++)
            {
                identifierBytes[i] = responseData[Shared.KeyLength + i];
            }

            SendFilePacket(key, keyFiles[key], BitConverter.ToInt64(identifierBytes));
        }
    }

    private void SendText(string key, string text)
    {
        SendData(key, PacketType.Text, 0, GetBytes(text));
    }
    
    public void SendFile(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException(path);
        }
        string key = GenerateKey();
        long fileSize = fileInfo.Length;
        if(fileSize / Shared.MaxDataSize + 1 >= Math.Pow(10, Shared.PacketIdentifierLength))
        {
            Console.WriteLine("Overall file size is too big. Consider changing MaxDataSize.");
            return;
        }
        keyFiles[key] = path;
        SendData(key, PacketType.FileName, 0, GetBytes(fileInfo.Name));
        SendData(key, PacketType.FileSize, 0, GetBytes(fileSize + ""));
        SendFilePacket(key, path, 0);
    }

    private void SendFilePacket(string key, string path, long identifier)
    {
        Console.WriteLine("Request for " + key + " and " + path + " (" + identifier + ")");
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
            SendData(key, PacketType.Byte, identifier, buffer);
        }
    }

    private void SendData(string key, PacketType packetType, long identifier, byte[] data)
    {
        if(data.Length > Shared.MaxDataSize)
        {
            throw new Exception("Data too large.");
        }
        byte[] newData = new byte[data.Length + Shared.HeaderLength];
        newData[0] = (byte) packetType;
        byte[] keyBytes = GetBytes(key);
        for(int i = 0; i < Shared.KeyLength; i++)
        {
            newData[i + 1] = keyBytes[i];
        }
        byte[] identifierBytes = BitConverter.GetBytes(identifier);
        for(int i = 0; i < Shared.PacketIdentifierLength && i < 8; i++)
        {
            newData[Shared.KeyLength + Shared.PacketTypeLength + i] = identifierBytes[i];
        }
        for(int i = 0; i < data.Length; i++)
        {
            newData[i + Shared.HeaderLength] = data[i];
        }
        _udpClient.SendAsync(newData, newData.Length, new IPEndPoint(IPAddress.Broadcast, Port));
    }

    private string GenerateKey()
    {
        StringBuilder builder = new StringBuilder();
        Random random = new Random();
        for(int i = 0; i < Shared.KeyLength; i++)
        {
            builder.Append((char) random.Next(48, 123));
        }
        return builder.ToString();
    }

    private byte[] GetBytes(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

    private string GetString(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public void Dispose()
    {
        _disposed = true;
        _udpClient.Close();
        _requestsClient.Close();
    }

}
