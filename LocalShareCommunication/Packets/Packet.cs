using LocalShareCommunication.Misc;

namespace LocalShareCommunication.Packets;

public class Packet
{

    public PacketType Type { get => _type; }
    private PacketType _type;
    public string Key { get => _key; }
    private string _key;
    public int DataLength { get => _dataLength; }
    private int _dataLength;
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
        InitDataLength(responseData);
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
        _dataLength = data.Length;
        _data = data;
    }

    public byte[] Create()
    {
        byte[] newData = new byte[_data.Length + Shared.HeaderLength];
        newData[0] = (byte) _type;
        byte[] keyBytes = EncodingManager.GetBytes(_key);
        for (int i = 0; i < Shared.KeyLength; i++)
        {
            newData[i + Shared.PacketTypeLength] = keyBytes[i];
        }
        byte[] dataLengthBytes = BitConverter.GetBytes(_dataLength);
        for (int i = 0; i < Shared.DataLength; i++)
        {
            newData[i + Shared.PacketTypeLength + Shared.KeyLength] = dataLengthBytes[i];
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

    private void InitDataLength(byte[] responseData)
    {
        byte[] dataLengthBytes = new byte[Shared.DataLength];
        for (int i = 0; i < Shared.DataLength; i++)
        {
            dataLengthBytes[i] = responseData[i + Shared.PacketTypeLength + Shared.KeyLength];
        }
        _dataLength = BitConverter.ToInt32(dataLengthBytes);
    }

    private void InitData(byte[] responseData)
    {
        _data = new byte[_dataLength];
        for (int i = 0; i < _dataLength; i++)
        {
            _data[i] = responseData[Shared.HeaderLength + i];
        }
    }

}
