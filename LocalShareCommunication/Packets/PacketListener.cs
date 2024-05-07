using System.Net;
using System.Net.Sockets;

namespace LocalShareCommunication.Packets;

public class PacketListener : IDisposable
{
    
    private bool _disposed = false;
    private UdpClient _listener;
    public int Port { get; }
    private Action<Packet> _handler;


    public PacketListener(int port, Action<Packet> handler)
    {
        this._listener = new UdpClient(port);
        Port = port;
        this._handler = handler;
    }

    public void StartListener()
    {
        new Thread(() =>
        {
            while (!_disposed)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);
                byte[] responseData = _listener.Receive(ref remoteEP);

                try
                {
                    new Thread(() => _handler.Invoke(new Packet(responseData))).Start();
                } catch(Exception)
                {
                    continue;
                }
            }
        }).Start();
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Close();
    }
}
