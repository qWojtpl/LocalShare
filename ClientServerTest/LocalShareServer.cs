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
        while (!_disposed)
        {
            Thread.Sleep(1000);

            SendFile("test.txt");

            Console.WriteLine("File sent successfully.");
        }
    }

    private void SendText(string text)
    {
        SendData(PacketType.Text, GetBytes(text));
    }

    public void SendFile(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException(path);
        }
        SendData(PacketType.FileName, GetBytes(fileInfo.Name));
        SendData(PacketType.FileSize, GetBytes(fileInfo.Length + ""));
    }

    private byte[] GetBytes(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

    private void SendData(PacketType packetType, byte[] data)
    {
        byte[] newData = new byte[data.Length + 1];
        newData[0] = (byte)packetType;
        for (int i = 0; i < data.Length; i++)
        {
            newData[i + 1] = data[i];
        }
        _udpClient.Send(newData, newData.Length, new IPEndPoint(IPAddress.Broadcast, Port));
    }

    public void Dispose()
    {
        _disposed = true;
        _udpClient.Close();
    }

}
