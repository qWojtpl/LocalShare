namespace LocalShareCommunication;

internal class Program
{
    public static void Main(string[] args)
    {
        new LocalShareServer().Start();
        //new LocalShareClient().Start();
        Task.Run(() =>
        {
            while(true)
            {
                Thread.Sleep(1);
            }
        }).Wait();
    }
}
