using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTest;

public class Packet
{
    
    public PacketType Type { get => _type; }
    private PacketType _type;
    public string Key { get => _key; }
    private string _key;
    public long Identifier { get => _identifier; }
    private long _identifier = 0;
    public byte[] Data { get => _data; }
    private byte[] _data;


    public Packet(byte[] responseData)
    {
        InitPacketType(responseData);
        InitKey(responseData);
        if(this._type.Equals(PacketType.Byte))
        {
            InitIdentifier(responseData);
        }
        InitData(responseData);
    }

    private void InitPacketType(byte[] responseData)
    {
        if(responseData[0] < 3)
        {
            this._type = (PacketType) responseData[0];
        } else 
        {
            this._type = PacketType.Byte;
        }
    }

    private void InitKey(byte[] responseData)
    {
        byte[] keyBytes = new byte[Shared.KeyLength];
        for (int i = 0; i < Shared.KeyLength; i++)
        {
            keyBytes[i] = responseData[i + Shared.PacketTypeLength];
        }
        this._key = GetText(keyBytes);
    }

    private void InitIdentifier(byte[] responseData)
    {
        byte[] identifierBytes = new byte[Shared.PacketIdentifierLength];
        for (int i = 0; i < Shared.PacketIdentifierLength; i++)
        {
            identifierBytes[i] = responseData[i + Shared.KeyLength + Shared.PacketTypeLength];
        }
        this._identifier = BitConverter.ToInt64(identifierBytes);
    }

    private void InitData(byte[] responseData)
    {
        this._data = new byte[responseData.Length - Shared.HeaderLength];

        for (int i = Shared.HeaderLength; i < responseData.Length; i++)
        {
            this._data[i - Shared.HeaderLength] = responseData[i];
        }

    }

    private string GetText(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

}
