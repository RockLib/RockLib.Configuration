using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace RockLib.Configuration.Example.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Configuration Manager Exmple Harness");

            try
            {

                var value = ConfigurationManager.AppSettings["key"];
                var section = ConfigurationManager.GetSection("appSettings");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.ReadLine();
        }
    }
}