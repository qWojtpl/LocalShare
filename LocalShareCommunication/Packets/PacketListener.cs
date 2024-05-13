using System.Net;
using System.Net.Sockets;

namespace LocalShareCommunication.Packets;

public class PacketListener : IDisposable
{
    
    private bool _disposed = false;
    private UdpClient _listener;
    public int Port { get; }
    private Action<Packet> _handler;
    private bool stopped = false;


    public PacketListener(int port, Action<Packet> handler)
    {
        this._listener = new UdpClient(port);
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
            while (!_disposed)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);
                try
                {
                    byte[] responseData = _listener.Receive(ref remoteEP);
                    if (responseData.Length == 0)
                    {
                        continue;
                    }
                    new Thread(() => _handler.Invoke(new Packet(responseData))).Start();
                } catch(Exception)
                {
                    continue;
                }
            }
        }).Start();
    }

    public void StopListener()
    {
        Dispose();
        stopped = true;
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Close();
    }

}
