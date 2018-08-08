# RockLib.Configuration [![Build status](https://ci.appveyor.com/api/projects/status/0qxs1k1bw36cn8ly?svg=true)](https://ci.appveyor.com/project/bfriesen/rocklib-configuration)

Defines a static `Config` class as a general replacement for the old .NET Framework `ConfigurationManager` class.

## Overview

The old .NET Framework `ConfigurationManager` class was very useful for libraries to use as a default source of per-application settings. For example, a class could define two constructors: one that defines all the settings for the class, and one parameterless constructor that reads the settings from configuration. But since `ConfigurationManager` no longer exists, this pattern becomes impossible. The `Config` class makes it possible again.

## Library Usage

Libraries are expected to access the `Config.Root` and `Config.AppSettings` properties in order to retrieve their settings.

```c#
IConfigurationSection fooSection = Config.Root.GetSection("foo");
string bar = Config.AppSettings["bar"];
```

## Application Usage

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
3
1) If the application is a .NET Framework app, from `ConfigurationManager` (see [.NET Framework Application Usage](#net-framework-application-usage) for details);
2) A `'appsettings.json'` file, relative to the current working directory;
3) A `'appsettings.{environment}.json file'`, relative to the corrent working directory, where `environment` is the value of the `ASPNETCORE_ENVIRONMENT` or `ROCKLIB_ENVIRONMENT` environment variable;
4) Environment variables.

**Note that ASP.NET Core applications do not automatically load settings from `'appsettings.json'` - the configuration root must be set explicitly as described above.**

## .NET Framework Application Usage

Starting in RockLib.Configuration version 2.1.0, .NET Framework applications can configure their application completely through their app.config/web.config, without any additional setup.

When adding a RockLib.Configuration section to an app.config/web.config, it must be registered in the `<configSections>` section, then defined in the config body. This template shows how (also note that the rest of the examples below redefine the same `<my_section>` custom section):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="my_section" type="RockLib.Configuration.RockLibConfigurationSection, RockLib.Configuration" />
  </configSections>

  <my_section>
    <!-- TODO: Add custom element(s) -->
  </my_section>
</configuration>
```

---

In order to determine what should go inside of the custom section, it is helpful to compare to a JSON configuration. Here is a relatively simple example:

```json
{
    "my_section": {
        "foo": {
            "bar": "abc",
            "baz": true
        },
        "qux": 123
    }
}
```

This is an equivalent custom section.

```xml
<my_section>
    <value qux="123">
        <foo bar="abc" baz="true" />
    </value>
</my_section>
```

Note the `<value>` element. The name of any such top-level elements - ones directly under the custom section element - don't actually matter. This custom section is equivalent.

```xml
<my_section>
    <item qux="123">
        <foo bar="abc" baz="true" />
    </item>
</my_section>
```

---

In this example, the JSON config contains an array.

```json
{
    "my_section": {
        "foo": [
            {
                "bar": "abc",
                "baz": true
            },
            {
                "bar": "xyz",
                "baz": false
            }
        ],
        "qux": 123
    }
}
```

To replicate the same structure in our custom section, multiple elements with the name of the JSON field are created.

```xml
<my_section>
    <value qux="123">
        <foo bar="abc" baz="true" />
        <foo bar="xyz" baz="false" />
    </value>
</my_section>
```

---

Each of the custom section examples above has used attributes to contain values, but elements are also valid (but more verbose, so not preferred).

```xml
<my_section>
    <item qux="123">
        <foo bar="abc" baz="true" />
    </item>
</my_section>
```

That example uses attributes. The following equivalent example uses elements.

```xml
<my_section>
    <item>
        <foo>
            <bar>abc</bar>
            <baz>true</baz>
        </foo>
        <qux>123</qux>
    </item>
</my_section>
```
