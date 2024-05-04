using System.Text;

namespace LocalShareCommunication.Misc;

public static class KeyGenerator
{
    public static string GenerateKey()
    {
        StringBuilder builder = new StringBuilder();
        Random random = new Random();
        for (int i = 0; i < Shared.KeyLength; i++)
        {
            builder.Append((char) random.Next(48, 123));
        }
        return builder.ToString();
    }

}
