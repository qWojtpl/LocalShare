using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ClientServerTest;

// First 1 byte is packet type
// Next 32 bytes are key
// Next 3 bytes are number of packet
// Next 476 bytes are file bytes
public class LocalShareClient : IDisposable
{

    private readonly UdpClient _listener;
    private bool _disposed = false;
    public int Port { get; }

    private string? key;
    private FileStream? writer;
    private string? fileName;
    private string? tempFileName;
    private int fileSize = -1;
    private int actualSize = 0;

    public LocalShareClient(int port = 2780)
    {
        Port = port;
        _listener = new UdpClient(port);
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
        actualSize = 0;
    }

    public async Task Start()
    {
        while (!_disposed)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] responseData = _listener.Receive(ref remoteEP);
            if(responseData.Length < Shared.HeaderLength)
            {
                continue;
            }

            PacketType packetType;
            if(responseData[0] < 3)
            {
                packetType = (PacketType) responseData[0];
            } else
            {
                packetType = PacketType.Byte;
            }

            byte[] keyBytes = new byte[Shared.KeyLength];
            for(int i = 0; i < Shared.KeyLength; i++)
            {
                keyBytes[i] = responseData[i + 1];
            }
            
            string key = GetText(keyBytes);
            if(this.key != null)
            {
                if(!this.key.Equals(key))
                {
                    continue;
                } 
            } else
            {
                Init(key);
            }

            if(PacketType.FileName.Equals(packetType))
            {
                HandleFileNamePacket(responseData);
            }
            else if(PacketType.FileSize.Equals(packetType))
            {
                HandleFileSizePacket(responseData);
            } else if(PacketType.Byte.Equals(packetType))
            {
                HandleBytePacket(responseData);
            }
        }
    }

    private void HandleFileNamePacket(byte[] responseData)
    {
        this.fileName = GetTextFromResponse(responseData); 
        CreateFileWithDirectory(fileName);
    }

    private void HandleFileSizePacket(byte[] responseData)
    {
        this.fileSize = int.Parse(GetTextFromResponse(responseData));
    }

    private void HandleBytePacket(byte[] responseData)
    {
        Console.WriteLine("HandleBytePacket");
        this.actualSize += responseData.Length - Shared.HeaderLength;
        if (this.writer == null)
        {
            if(this.fileName == null)
            {
                Console.WriteLine("Not found filename, creating custom name...");
                tempFileName = "./files/unknown-" + DateTime.Now;
                CreateFileWithDirectory(tempFileName);
                this.writer = File.OpenWrite(tempFileName);
            }
            else
            {
                Console.WriteLine("Found filename, creating new file...");
                this.writer = File.OpenWrite("./files/" + this.fileName);
            }
        }
        byte[] writeBytes = GetBytesFromResponse(responseData);
        writer.Write(writeBytes, 0, writeBytes.Length);
        if(fileSize != -1 && writer != null)
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
                Console.WriteLine("Sharing end.");
                Init(null);
                return;
            }
            else if(actualSize >= fileSize)
            {
                throw new Exception("Sent file is too large!");
            }
        }
    }

    private void CreateFileWithDirectory(string fileName)
    {
        Directory.CreateDirectory("./files/");
        File.Create("./files/" + fileName).Close();
    }
    
    private byte[] GetBytesFromResponse(byte[] bytes)
    {
        byte[] data = new byte[bytes.Length - Shared.HeaderLength];

        for(int i = Shared.HeaderLength; i < bytes.Length; i++)
        {
            data[i - Shared.HeaderLength] = bytes[i];
        }

        return data;
    }

    private string GetText(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    private string GetTextFromResponse(byte[] bytes)
    {
        return GetText(GetBytesFromResponse(bytes));
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Close();
        if(writer != null) {
            writer.Close();
        }   
    }

}
