namespace LocalShareCommunication;

internal class Program
{
    public static void Main(string[] args)
    {
/*        LocalShareServer server = new LocalShareServer();
        server.Start();
        //server.SendFile(@"");
        new LocalShareClient().Start();*/
        Task.Run(() =>
        {
            while(true)
            {
                Thread.Sleep(1);
            }
        }).Wait();
    }
}
