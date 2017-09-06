using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RockLib.Configuration;
using System;

namespace Example.EnvironmentVariableConfig.DotNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RockLib.Configuration Environment Variable Configuration Example");

            // Note that the current version (0.0.1-alpha02) requires that the rocklib.config.json file exists.
            // In order to successfully load, this file must a valid JSON object. A config file with an empty
            // JSON object (open & close curly braces) is the current work-around for this.

            SetEnvironmentVariables();

            var testKey1 = Config.AppSettings["test_key1"];
            var testKey2 = Config.AppSettings["test_key2"];

            var foo = Config.Root.GetSection("foo_section").Get<Foo>();

            var qux = Config.Root.GetSection("qux_section").Get<Qux>();

            Console.WriteLine($"testKey1: {testKey1}");
            Console.WriteLine($"testKey2: {testKey2}");
            Console.WriteLine($"foo: {JsonConvert.SerializeObject(foo)}");
            Console.WriteLine($"qux: {JsonConvert.SerializeObject(qux)}");

            Console.ReadLine();
        }

        private static void SetEnvironmentVariables()
        {
            // Note: these environment variables could be set up on any level (machine, user, process) with
            // the same values, and this example would still work. Setting environment variables programmatically
            // means that they are set on the process level.

            Environment.SetEnvironmentVariable("AppSettings:test_key1", "123");
            Environment.SetEnvironmentVariable("AppSettings:test_key2", "789");

            Environment.SetEnvironmentVariable("foo_section:Bar", "123.45");
            Environment.SetEnvironmentVariable("foo_section:Baz", "abc");

            Environment.SetEnvironmentVariable("qux_section:Garply", "Fred");
            Environment.SetEnvironmentVariable("qux_section:Foos:0:Bar", "1.1");
            Environment.SetEnvironmentVariable("qux_section:Foos:0:Baz", "aaa");
            Environment.SetEnvironmentVariable("qux_section:Foos:1:Bar", "2.2");
            Environment.SetEnvironmentVariable("qux_section:Foos:1:Baz", "bbb");
            Environment.SetEnvironmentVariable("qux_section:Foos:2:Bar", "3.3");
            Environment.SetEnvironmentVariable("qux_section:Foos:2:Baz", "ccc");
        }
    }

    public class Foo
    {
        public double Bar { get; set; }
        public string Baz { get; set; }
    }

    public class Qux
    {
        public Garply Garply { get; set; }

        public Foo[] Foos { get; set; }
    }

    public enum Garply
    {
        Grault,
        Thud,
        Fred
    }
}
