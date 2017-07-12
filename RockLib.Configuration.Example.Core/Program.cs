using Newtonsoft.Json;
using System;
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
                string applicationId = ConfigurationManager.AppSettings["ApplicationId"];
                string defaultConnectionString = ConfigurationManager.ConnectionStrings["Default"];
                FooSection foo = (FooSection)ConfigurationManager.GetSection("Foo");

                Console.WriteLine($"applicationId: {applicationId}");
                Console.WriteLine($"defaultConnectionString: {defaultConnectionString}");
                Console.WriteLine($"foo: {JsonConvert.SerializeObject(foo)}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }

        class FooSection
        {
            public int Bar { get; set; }
            public string Baz { get; set; }
            public bool Qux { get; set; }
        }
    }
}