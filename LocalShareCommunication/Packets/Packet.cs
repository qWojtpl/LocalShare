using LocalShareCommunication.Misc;

namespace LocalShareCommunication.Packets;

public class Packet
{

    public PacketType Type { get => _type; }
    private PacketType _type;
    public string Key { get => _key; }
    private string _key;
    public byte[] Data { get => _data; }
    private byte[] _data;

    public Packet(byte[] responseData)
    {
        if (responseData.Length < Shared.HeaderLength)
        {
            throw new Exception("Packet too small.");
        }
        InitPacketType(responseData);
        InitKey(responseData);
        InitData(responseData);
    }

    public Packet(PacketType type, string key, byte[] data)
    {
        _type = type;
        _key = key;
        if(data.Length > Shared.MaxDataSize)
        {
            throw new Exception("Data too large.");
        }
        _data = data;
    }

    public byte[] Create()
    {
        byte[] newData = new byte[_data.Length + Shared.HeaderLength];
        newData[0] = (byte) _type;
        byte[] keyBytes = EncodingManager.GetBytes(_key);
        for (int i = 0; i < Shared.KeyLength; i++)
        {
            newData[i + 1] = keyBytes[i];
        }
        for (int i = 0; i < _data.Length; i++)
        {
            newData[i + Shared.HeaderLength] = _data[i];
        }
        return newData;
    }

    public override string ToString()
    {
        return EncodingManager.GetText(Create());
    }

    private void InitPacketType(byte[] responseData)
    {
        if (responseData[0] < 3)
        {
            _type = (PacketType) responseData[0];
        }
        else
        {
            _type = PacketType.Byte;
        }
    }

    private void InitKey(byte[] responseData)
    {
        byte[] keyBytes = new byte[Shared.KeyLength];
        for (int i = 0; i < Shared.KeyLength; i++)
        {
            keyBytes[i] = responseData[i + Shared.PacketTypeLength];
        }
        _key = EncodingManager.GetText(keyBytes);
    }

    private void InitData(byte[] responseData)
    {
        _data = new byte[responseData.Length - Shared.HeaderLength];

        for (int i = Shared.HeaderLength; i < responseData.Length; i++)
        {
            _data[i - Shared.HeaderLength] = responseData[i];
        }

    }

}
