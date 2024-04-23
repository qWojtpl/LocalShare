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
    public int Port { get; }

    public LocalShareClient(int port = 2780)
    {
        Port = port;
        this._listener = new UdpClient(port);
    }

    public async Task Start()
    {
        while(true)
        {
            await Console.Out.WriteLineAsync("Waiting...");
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] responseData = _listener.Receive(ref remoteEP);
            string responseMessage = Encoding.UTF8.GetString(responseData);

            Console.WriteLine(responseMessage);
        }
    }

    public void Dispose()
    {
        _listener.Close();
    }
    
}
