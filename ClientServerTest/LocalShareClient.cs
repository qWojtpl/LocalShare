using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace ClientServerTest;

// First 1 byte is packet type
// Next 32 bytes are key
// Next 3 bytes are number of packet
// Next 476 bytes are file bytes
public class LocalShareClient : IDisposable
{

    private readonly UdpClient _listener;
    private readonly UdpClient _claimClient;
    private bool _disposed = false;
    public int Port { get; }

    private string? key;
    private FileStream? writer;
    private string? fileName;
    private string? tempFileName;
    private int fileSize;
    private int actualSize;
    private long lastIdentifier;

    public LocalShareClient(int port = 2780)
    {
        Port = port;
        _listener = new UdpClient(port);
        _claimClient = new UdpClient();
        _claimClient.EnableBroadcast = true;
    }

    private void Init(string? key)
    {
        this.key = key;
        if(this.writer != null)
        {
            this.writer.Close();
        }
        this.writer = null;
        this.fileName = null;
        this.tempFileName = null;
        this.fileSize = -1;
        this.actualSize = 0;
        this.lastIdentifier = -1;
    }

    public async Task Start()
    {
        while (!_disposed)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);
            byte[] responseData = _listener.Receive(ref remoteEP);

            Task.Run(() => HandlePacket(responseData));

        }
    }

    private void HandlePacket(byte[] responseData)
    {
        if(responseData.Length < Shared.HeaderLength)
        {
            return;
        }

        Packet packet = new Packet(responseData);
            
        string key = packet.Key;
        if(this.key != null)
        {
            if(!this.key.Equals(key))
            {
                return;
            }
        } else
        {
            Init(key);
        }

        PacketType packetType = packet.Type;

        if(PacketType.FileName.Equals(packetType))
        {
            HandleFileNamePacket(packet);
        }
        else if(PacketType.FileSize.Equals(packetType))
        {
            HandleFileSizePacket(packet);
        } else if(PacketType.Byte.Equals(packetType))
        {
            HandleBytePacket(packet);
        }
    }

    private void HandleFileNamePacket(Packet packet)
    {
        this.fileName = GetText(packet.Data); 
        CreateFileWithDirectory(fileName);
    }

    private void HandleFileSizePacket(Packet packet)
    {
        this.fileSize = int.Parse(GetText(packet.Data));
    }

    private void HandleBytePacket(Packet packet)
    {
        long identifier = packet.Identifier;
        if(lastIdentifier + 1 != identifier)
        {
            return;
        }
        this.actualSize += packet.Data.Length;
        if (this.writer == null)
        {
            if(this.fileName == null)
            {
                Console.WriteLine("Not found filename, creating custom name...");
                tempFileName = "./files/unknown-" + DateTime.Now.ToString()
                    .Replace(" ", "-")
                    .Replace(":", "-")
                    .Replace(".", "-");
                CreateFileWithDirectory(tempFileName);
                this.writer = File.OpenWrite(tempFileName);
            }
            else
            {
                Console.WriteLine("Found filename, creating new file...");
                this.writer = File.OpenWrite("./files/" + this.fileName);
            }
        }
        writer.Write(packet.Data, 0, packet.Data.Length);
        if (fileSize != -1 && writer != null)
        {
            if(actualSize == fileSize)
            {
                if(this.fileName != null && tempFileName != null)
                {
                    if(!this.fileName.Equals(writer.Name))
                    {
                        Console.WriteLine("Renaming file...");
                        File.Move("./files/" + tempFileName, "./files/" + this.fileName);
                    }
                }
                Init(null);
                return;
            }
            else if(actualSize >= fileSize)
            {
                throw new Exception("Sent file is too large!");
            }
        }
        this.lastIdentifier = identifier;
        RequestPacket(key, lastIdentifier + 1);
    }

    private void RequestPacket(string key, long identifier)
    {
        if(identifier <= lastIdentifier)
        {
            return;
        }
        byte[] requestPacket = new byte[Shared.KeyLength + Shared.PacketIdentifierLength];
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] identifierBytes = BitConverter.GetBytes(identifier);
        for(int i = 0; i < Shared.KeyLength; i++)
        {
            requestPacket[i] = keyBytes[i];
        }
        for(int i = 0; i < Shared.PacketIdentifierLength; i++)
        {
            requestPacket[i + Shared.KeyLength] = identifierBytes[i];
        }
        Console.WriteLine("Requesting " + identifier + " from " + IPAddress.Broadcast + ":" + (Port + 1));
        _claimClient.SendAsync(requestPacket, requestPacket.Length, new IPEndPoint(IPAddress.Broadcast, Port + 1));
        Thread.Sleep(350);
        if(lastIdentifier <= identifier)
        {
            RequestPacket(key, identifier);
        }
    }

    private void CreateFileWithDirectory(string fileName)
    {
        Directory.CreateDirectory("./files/");
        File.Create("./files/" + fileName).Close();
    }

    private string GetText(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Close();
        _claimClient.Close();
        if(writer != null) {
            writer.Close();
        }   
    }

}
