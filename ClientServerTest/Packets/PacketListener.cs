using System.Net;
using System.Net.Sockets;

namespace ClientServerTest.Packets;

public class PacketListener : IDisposable
{
    
    private bool _disposed = false;
    private UdpClient _listener;
    public int Port { get; }
    private Action<Packet> _action;


    public PacketListener(UdpClient listener, int port, Action<Packet> action)
    {
        this._listener = listener;
        Port = port;
        this._action = action;
    }

    public void StartListener()
    {
        Task.Run(() =>
        {
            while (!_disposed)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);
                byte[] responseData = _listener.Receive(ref remoteEP);

                if (responseData.Length < Shared.HeaderLength)
                {
                    continue;
                }

                Task.Run(() => _action.Invoke(new Packet(responseData)));
            }
        });
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Close();
    }
}
