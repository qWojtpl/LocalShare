
using System.Net;
using System.Net.Sockets;

namespace LocalShareCommunication.Packets;

public class PacketListener : IDisposable
{

    private bool _disposed = false;
    private TcpListener _listener;
    public int Port { get; }
    private Action<TcpClient, Packet> _handler;
    private bool stopped = false;


    public PacketListener(int port, Action<TcpClient, Packet> handler)
    {
        this._listener = new TcpListener(IPAddress.Any, port);
        Port = port;
        this._handler = handler;
    }

    public void StartListener()
    {
        if (stopped)
        {
            throw new Exception("You can't start the same PacketListener object after stopping it.");
        }
        new Thread(() =>
        {
            _listener.Start();
            while (!_disposed)
            {
                TcpClient client = _listener.AcceptTcpClient();

                HandleClient(client);
            }
        }).Start();
    }

    public void StopListener()
    {
        Dispose();
        stopped = true;
    }

    private void HandleClient(TcpClient client)
    {
        new Thread(() =>
        {
            NetworkStream stream = client.GetStream();
            while (!_disposed)
            {
                if (!stream.DataAvailable)
                {
                    continue;
                }
                int totalBytesRead = 0;
                int bytesRead;
                byte[] buffer = new byte[1];
                byte[] totalResponse = new byte[Shared.PacketLength];

                while (stream.DataAvailable && totalBytesRead < Shared.PacketLength)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    totalResponse[totalBytesRead++] = buffer[0];
                }

                if (totalBytesRead == 0)
                {
                    return;
                }

                byte[] responseData = new byte[totalBytesRead];
                for (int i = 0; i < totalBytesRead; i++)
                {
                    responseData[i] = totalResponse[i];
                }

                new Thread(() => _handler.Invoke(client, new Packet(responseData))).Start();
            }
        }).Start();
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Stop();
    }

}
