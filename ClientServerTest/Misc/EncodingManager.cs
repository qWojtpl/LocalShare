using System.Text;

namespace ClientServerTest.Misc;

public static class EncodingManager
{
    public static string GetText(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }
    public static byte[] GetBytes(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

}
