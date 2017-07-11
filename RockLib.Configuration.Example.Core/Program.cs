using System;
using RockLib.Configuration;

namespace RockLib.Configuration.Example.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Configuration Manager Exmple Harness");

            var value = ConfigurationManager.AppSettings[""];

            Console.ReadLine();
        }
    }
}