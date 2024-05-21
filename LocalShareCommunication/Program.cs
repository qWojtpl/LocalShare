using LocalShareCommunication.Client;
using LocalShareCommunication.Server;

namespace LocalShareCommunication;

internal class Program
{
    public static void Main(string[] args)
    {
        LocalShareServer server = new LocalShareServer();
        server.Start();
        LocalShareClient client = new LocalShareClient();
        client.Start();
        server.SendFile(@"Heartbeat_Connection.mp4");
        Thread.Sleep(2000);
        server.SendFile(@"Heartbeat_Connection.mp4");
        Task.Run(() =>
        {
            while(true)
            {
                Thread.Sleep(1);
            }
        }).Wait();
    }
}
