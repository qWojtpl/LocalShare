using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTest;

public class LocalShareClient : IDisposable
{

    private readonly UdpClient _listener;
    private bool _disposed = false;
    public int Port { get; }

    private bool downloadStarted;
    private string? fileName;
    private int fileSize;
    private int uploadId;

    public LocalShareClient(int port = 2780)
    {
        Port = port;
        _listener = new UdpClient(port);
    }

    public async Task Start()
    {
        while (!_disposed)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] responseData = _listener.Receive(ref remoteEP);

            PacketType packetType;

            if(responseData[0] < 3)
            {
                packetType = (PacketType) responseData[0];
            } else
            {
                packetType = PacketType.Byte;
            }

            if(!downloadStarted)
            {
                if(packetType.Equals(PacketType.FileName))
                {
                    fileName = GetTextFromResponse(responseData);
                } else if(packetType.Equals(PacketType.FileSize))
                {
                    fileSize = int.Parse(GetTextFromResponse(responseData));
                }
            }

            Console.WriteLine("PacketType: " + packetType + ", data: " + GetTextFromResponse(responseData));
        }
    }

    private string GetText(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    private string GetTextFromResponse(byte[] bytes)
    {
        byte[] data = new byte[bytes.Length - 1];

        for (int i = 1; i < bytes.Length; i++)
        {
            data[i - 1] = bytes[i];
        }

        return GetText(data);
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Close();
    }

}
