﻿namespace ClientServerTest;

internal class Program
{
    public static void Main(string[] args)
    {
        Task.Run(async () => await new LocalShareServer().Start()).Wait();
        //Task.Run(async () => await new LocalShareClient().Start()).Wait();
    }
}
