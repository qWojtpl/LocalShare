using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTest;

public class Shared
{

    public static int KeyLength { get; } = 32;
    public static int PacketTypeLength { get; } = 1;
    public static int PacketNumberLength { get; } = 3;
    public static int MaxDataSize { get; } = 476;
    public static int HeaderLength { get => KeyLength + PacketTypeLength + PacketNumberLength; }

}
