
using LocalShareCommunication.Misc;
using System.Net;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    HandleClient(client);
                } catch(Exception) 
                {
                    return;
                }
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
                byte[] dataLengthBytes = new byte[Shared.DataLength];
                int readDataLength = 0;
                byte[] totalResponseData = new byte[Shared.PacketLength];

                while (stream.DataAvailable)
                {
                    byte[] buffer = new byte[1];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if(bytesRead == 0)
                    {
                        break;
                    }
                    totalResponseData[totalBytesRead++] = buffer[0];
                    if (totalBytesRead < Shared.PacketTypeLength + Shared.KeyLength + 1)
                    {
                        continue;
                    }
                    if (totalBytesRead < Shared.HeaderLength)
                    {
                        dataLengthBytes[readDataLength++] = buffer[0];
                        continue;
                    }
                    if (totalBytesRead == Shared.HeaderLength + BitConverter.ToInt32(dataLengthBytes))
                    {
                        byte[] responseData = new byte[totalBytesRead];
                        for(int i = 0; i < totalBytesRead; i++) 
                        {
                            responseData[i] = totalResponseData[i];
                        }
                        Packet packet = new Packet(responseData);
                        var zx = EncodingManager.GetText(packet.Data);
                        new Thread(() => _handler.Invoke(client, packet)).Start();
                        totalBytesRead = 0;
                        dataLengthBytes = new byte[Shared.DataLength];
                        readDataLength = 0;
                        totalResponseData = new byte[Shared.PacketLength];
                    }
                }

            }
        }).Start();
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Stop();
    }

}
