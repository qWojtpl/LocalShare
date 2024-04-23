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

    private UdpClient _udpClient = new UdpClient();
    public int Port { get; }

    public LocalShareServer(int port = 2780)
    {
        Port = port;
    }

    public async Task Start()
    {

        while(true)
        {
            Thread.Sleep(1000);
            string message = "Hello from sender!";
            byte[] data = Encoding.UTF8.GetBytes(message);

            _udpClient.EnableBroadcast = true;
            _udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, Port));

            Console.WriteLine("Broadcast message sent successfully.");
        }

    }

    public void Dispose()
    {
        _udpClient.Close();
    }

}
