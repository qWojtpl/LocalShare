
namespace LocalShareCommunication;

public class Shared
{

    public static int KeyLength { get; } = 8;
    public static int PacketTypeLength { get; } = 1;
    public static int MaxDataSize { get; set; } = 20000;
    public static int HeaderLength { get => KeyLength + PacketTypeLength; }
    public static int PacketLength { get => HeaderLength + MaxDataSize; }
    public static int UploadTimeout { get; } = 30;
    public static string FilesPath { get; set; } = "./files/";

}
