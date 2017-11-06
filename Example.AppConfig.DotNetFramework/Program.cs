using System;
using RockLib.Configuration;

namespace Example.AppConfig.DotNetFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("App.Config File Example Harness");

            var key100 = Config.AppSettings["Key100"];

            Console.WriteLine($"Key100: {key100}");

            Console.ReadLine();
        }
    }
}
