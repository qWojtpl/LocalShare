namespace ClientServerTest;

internal class Program
{
    public static void Main(string[] args)
    {
        new LocalShareServer().Start();
        new LocalShareClient().Start();
    }
}
