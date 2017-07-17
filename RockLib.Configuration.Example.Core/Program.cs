using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;

namespace RockLib.Configuration.Example.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Configuration Manager Exmple Harness");

            try
            {
                var applicationId = Config.AppSettings["ApplicationId"];
                string defaultConnectionString = Config.Root.GetConnectionString("Default");
                var foo = Config.Root.GetSection("Foo").Get<FooSection>();
                var foo2 = Config.Root.GetSection("Foo").Get<FooSection>();

                Console.WriteLine($"applicationId: {applicationId}");
                Console.WriteLine($"defaultConnectionString: {defaultConnectionString}");
                Console.WriteLine($"foo: {JsonConvert.SerializeObject(foo)}");
                Console.WriteLine($"foo is same instance as foo2: {ReferenceEquals(foo, foo2)}");

                var notFound = Config.AppSettings["notFound"];
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