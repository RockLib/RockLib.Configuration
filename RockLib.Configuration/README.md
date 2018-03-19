# RockLib.Configuration [![Build status](https://ci.appveyor.com/api/projects/status/0qxs1k1bw36cn8ly?svg=true)](https://ci.appveyor.com/project/bfriesen/rocklib-configuration)

Defines a static `Config` class as a general replacement for the old .NET Framework `ConfigurationManager` class.

### Overview

The old .NET Framework `ConfigurationManager` class was very useful for libraries to use as a default source of per-application settings. For example, a class could define two constructors: one that defines all the settings for the class, and one parameterless constructor that reads the settings from configuration. But since `ConfigurationManager` no longer exists, this pattern becomes impossible. The `Config` class makes it possible again.

### Library Usage

Libraries are expected to access the `Config.Root` and `Config.AppSettings` properties in order to retrieve their settings.

```c#
IConfigurationSection fooSection = Config.Root.GetSection("foo");
string bar = Config.AppSettings["bar"];
```

### Application Usage

Applications are expected to provide an instance - the "Root" - of the `IConfiguration` interface to the `Config` class. This can be done explicitly or implicitly.

To explicitly set the configuration root, call the `SetRoot` method. ASP.NET Core Applications should call this method in their `Startup` constructor.

```c#
public Startup(IConfiguration configuration)
{
    Configuration = configuration;
    Config.SetRoot(Configuration);
}
```

If the configuration root is not explicitly set, it will load configuration settings, in order, from:

1) If the application is a .NET Framework app, from `ConfigurationManager.AppSettings`;
2) A `'appsettings.json'` file, relative to the current working directory;
3) A `'appsettings.{environment}.json file'`, relative to the corrent working directory, where `environment` is the value of the `ASPNETCORE_ENVIRONMENT` or `ROCKLIB_ENVIRONMENT` environment variable;
4) Environment variables.

**Note that ASP.NET Core applications do not automatically load settings from `'appsettings.json'` - the configuration root must be set explicitly as described above.**
