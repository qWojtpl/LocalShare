namespace LocalShareCommunication;

internal class Program
{
    public static void Main(string[] args)
    {
        LocalShareServer server = new LocalShareServer();
        server.Start();
        server.SendFile(@"D:\SteamLibrary\steamapps\common\OMSI 2\.mods\Jelcz M081MB Vero v.1.1.rar");
        new LocalShareClient().Start();
        Task.Run(() =>
        {
            while(true)
            {
                Thread.Sleep(1);
            }
        }).Wait();
    }
}
