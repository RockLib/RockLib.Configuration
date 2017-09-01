using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;

namespace RockLib.Configuration.Example.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Configuration Manager Example Harness");

            try
            {
                var key1 = Config.AppSettings["Key1"];
                string defaultConnectionString = Config.Root.GetConnectionString("Default");
                var foo = Config.Root.GetSection("Foo").Get<FooSection>();
                var foo2 = Config.Root.GetSection("Foo").Get<FooSection>();

                Console.WriteLine($"key1: {key1}");
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