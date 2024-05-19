using LocalShareCommunication.UdpService;
using System.Net;
using System.Net.Sockets;

namespace LocalShareCommunication.Packets;

public class PacketSender : IDisposable
{

    private List<TcpClient> _tcpClients = new();
    private UdpCallbacker _udpCallbacker;
    public int Port { get; }

    public PacketSender(UdpCallbacker udpCallbacker, int port)
    {
        Port = port;
        _udpCallbacker = udpCallbacker;
        udpCallbacker.SetAction(AddClient);
    }

    private void AddClient(IPAddress address)
    {
        foreach(TcpClient c in _tcpClients)
        {
            if(((IPEndPoint) c.Client.RemoteEndPoint).Address.Equals(address))
            {
                continue;
            }
        }
        TcpClient client = new TcpClient();
        client.Connect(new IPEndPoint(address, Port));
        Console.WriteLine("Adding client: " + address + ":" + Port);
        _tcpClients.Add(client);
    }

    public void SendData(PacketType packetType, string key, byte[] data)
    {
        SendPacket(new Packet(packetType, key, data));
    }

    public void SendData(TcpClient client, PacketType packetType, string key, byte[] data)
    {
        SendPacket(client, new Packet(packetType, key, data).Create());
    }

    public void SendPacket(Packet packet)
    {
        byte[] packetBytes = packet.Create();
        foreach (TcpClient client in _tcpClients)
        {
            client.GetStream().Write(packetBytes, 0, packetBytes.Length);
        }
    }

    public void SendPacket(TcpClient client, byte[] packetBytes)
    {
        foreach (TcpClient c in _tcpClients)
        {
            string[] split = ((IPEndPoint)c.Client.RemoteEndPoint).Address.ToString().Split(":");
            string ip = split[split.Length - 1];
            string targetIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            if (targetIP.Equals(ip))
            {
                c.GetStream().Write(packetBytes, 0, packetBytes.Length);
                return;
            }
        }
    }

    public void Dispose()
    {
        foreach(TcpClient client in _tcpClients) 
        {
            client.Close();
        }
    }

}