using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RockLib.Configuration;
using System;
using RockLib.Configuration.ObjectFactory;

namespace Example.FileConfig.DotNetCore
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
                var foo = Config.Root.GetSection("Foo").Create<FooSection>();
                var foo2 = Config.Root.GetSection("Foo").Create<FooSection>();

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