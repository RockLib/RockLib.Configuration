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
                var applicationId = ConfigurationManager.AppSettings["ApplicationId"];
                string defaultConnectionString = ConfigurationManager.ConfigurationRoot.GetConnectionString("Default");
                var foo = ConfigurationManager.ConfigurationRoot.GetSection("Foo").Get<FooSection>();
                var foo2 = ConfigurationManager.ConfigurationRoot.GetSection("Foo").Get<FooSection>();

                Console.WriteLine($"applicationId: {applicationId}");
                Console.WriteLine($"defaultConnectionString: {defaultConnectionString}");
                Console.WriteLine($"foo: {JsonConvert.SerializeObject(foo)}");
                Console.WriteLine($"foo is same instance as foo2: {ReferenceEquals(foo, foo2)}");

                var notFound = ConfigurationManager.AppSettings["notFound"];
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