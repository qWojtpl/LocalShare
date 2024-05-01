using System.Net;
using System.Net.Sockets;

namespace ClientServerTest.Packets;

public class PacketSender : IDisposable
{

    private UdpClient _udpClient;
    public int Port { get; }

    public PacketSender(int port)
    {
        _udpClient = new UdpClient();
        _udpClient.EnableBroadcast = true;
        Port = port;
    }

    public void SendData(PacketType packetType, string key, long identifier, byte[] data)
    {
        SendPacket(new Packet(packetType, key, identifier, data));
    }

    public void SendPacket(Packet packet)
    {
        byte[] packetBytes = packet.Create();
        _udpClient.SendAsync(packetBytes, packetBytes.Length, new IPEndPoint(IPAddress.Broadcast, Port));
    }

    public void Dispose()
    {
        _udpClient.Close();
    }

}