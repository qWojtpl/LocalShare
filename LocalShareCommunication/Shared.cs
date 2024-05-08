
namespace LocalShareCommunication;

public class Shared
{

    public static int KeyLength { get; } = 16;
    public static int PacketTypeLength { get; } = 1;
    public static int PacketIdentifierLength { get; } = 8;
    public static int MaxDataSize { get; } = 20000;
    public static int HeaderLength { get => KeyLength + PacketTypeLength + PacketIdentifierLength; }
    public static int PacketLength { get => HeaderLength + MaxDataSize; }
    public static int GoalChunkCount { get; } = 16;
    public static string FilesPath { get; set; } = "./files/";

}
