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

    public LocalShareClient(int port = 2780)
    {
        Port = port;
        _listener = new UdpClient(port);
    }

    public async Task Start()
    {
        while (!_disposed)
        {
            Console.WriteLine("Waiting...");
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] responseData = _listener.Receive(ref remoteEP);

            PacketType packetType = (PacketType)responseData[0];

            byte[] data = new byte[responseData.Length - 1];

            for (int i = 1; i < responseData.Length; i++)
            {
                data[i - 1] = responseData[i];
            }

            Console.WriteLine("PacketType: " + packetType + ", data: " + GetText(data));
        }
    }

    private string GetText(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Close();
    }

}
