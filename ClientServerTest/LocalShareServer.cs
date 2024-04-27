using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTest;

public class LocalShareServer : IDisposable
{

    private readonly UdpClient _udpClient;
    private bool _disposed = false;
    public int Port { get; }

    public LocalShareServer(int port = 2780)
    {
        Port = port;
        _udpClient = new UdpClient();
        _udpClient.EnableBroadcast = true;
    }

    public async Task Start()
    {
        SendFile("test.zip");
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
        SendData(key, PacketType.FileName, 0, GetBytes(fileInfo.Name));
        SendData(key, PacketType.FileSize, 0, GetBytes(fileSize + ""));
        using(FileStream stream = File.OpenRead(path))
        {
            long bytesRead = 0;
            int i = 0;
            while(bytesRead < fileSize)
            {
                long bufferSize = Shared.MaxDataSize;
                if(fileSize - bytesRead < Shared.MaxDataSize)
                {
                    bufferSize = fileSize - bytesRead;
                }
                byte[] buffer = new byte[bufferSize];
                stream.Seek(i * Shared.MaxDataSize, SeekOrigin.Begin);
                stream.Read(buffer, 0, buffer.Length);
                SendData(key, PacketType.Byte, i, buffer);
                bytesRead += bufferSize;
                i++;
            }
        }
    }

    private void SendData(string key, PacketType packetType, int number, byte[] data)
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
        for(int i = 0; i < Shared.PacketNumberLength; i++)
        {
            int num = 0;
            if(number > 255)
            {
                number -= 255;
                num = 255;
            } else
            {
                num = number;
            }
            newData[Shared.HeaderLength - Shared.PacketNumberLength + i] = (byte) num;
        }
        for(int i = 0; i < data.Length; i++)
        {
            newData[i + Shared.HeaderLength] = data[i];
        }
        _udpClient.Send(newData, newData.Length, new IPEndPoint(IPAddress.Broadcast, Port));
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

    public void Dispose()
    {
        _disposed = true;
        _udpClient.Close();
    }

}
