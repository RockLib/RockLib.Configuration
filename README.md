This repository contains the source code for three nuget packages:

### RockLib.Configuration [![Build status](https://ci.appveyor.com/api/projects/status/0qxs1k1bw36cn8ly?svg=true)](https://ci.appveyor.com/project/bfriesen/rocklib-configuration)
  
  Defines a static `Config` class as a general replacement for the old .NET Framework `ConfigurationManager` class.

### RockLib.Configuration.ObjectFactory [![Build status](https://ci.appveyor.com/api/projects/status/ox9velgud5ljj8d0?svg=true)](https://ci.appveyor.com/project/bfriesen/rocklib-configuration-owv0n)

An alternative to `Microsoft.Extensions.Configuration.Binder` that supports readonly properties and other features commonly found in JSON and XML serializers.

### RockLib.Configuration.ProxyFactory [![Build status](https://ci.appveyor.com/api/projects/status/ogf4axfokbklh638?svg=true)](https://ci.appveyor.com/project/bfriesen/rocklib-configuration-fe27m)

A factory that creates instances of property-only interfaces, defined at run-time, and populated with values defined in an instance of `IConfiguration`.

All RockLib packages will depend on the RockLib.Configuration package to provide its configuration features.

## Table Of Contents
* [Steps to Success](#steps-to-success)
* [Nuget Packages](#packages)
* [Configuration Setup](#configuration-setup)
  * [RockLib Configuration Setup](#rockLib-configuration-setup)
  * [Application Configuration Setup](#application-configuration-setup)
  * [Environment Variables Setup](#environment-variables-setup)
* [How to use](#how-to-use)
  * [Accessing App Settings](#accessing-app-settings)
  * [Accessing Connection Strings](#accessing-connection-strings)
  * [Accessing Custom Sections](#accessing-custom-sections)
* [Example Application](#example-application)

## Steps to Success
In order go get the RockLib.Configuration provider to work inside your application you will need to complete the following steps

1. Install all the required [NuGet](#packages) packages.
2. Create and configure the `rocklib.config.json` file (this is the application's configuration file)

## Packages 

### Required Packages
1. RockLib.Configuration

### Install Nuget via UI
If you want to install this package via the NuGet UI, this can be done as well. 

If you are unsure how to use the UI to reference the package source checkout out these [docs](https://docs.microsoft.com/en-us/nuget/tools/package-manager-ui#package-sources).

### Install Nuget via Command Line

How to install from the package manager console:

```
PM> Install-Package RockLib.Configuration
```

## Configuration Setup
By default, the RockLib.Configuration library will attempt to pull values from multiple sources.  These sources include App.config/web.config files (for .NET Framework applications), the `rocklib.config.json` file, Environment variables, and application-driven key/value pairs.

he order of precedence, from lowest priority to highest, is App/Web Configuration file, `rocklib.config.json` file, Environment variables, application-provided values.  This means that if you have an app setting value in `rocklib.config.json` and a setting in an environment variable, the value from the environment variable will be the one provided by the RockLib.Configuration library.

### RockLib Configuration Setup
The `rocklib.config.json` is a Json formatted file that can contain `appSettings` section in order to store your key/value pairs.  In order for this file to be used by RockLib.Configuration you need to setup this file to be copied to your output directory.

The usage of the `rocklib.config.json` file will normally be associated with .NET Core/Standard applications when needing to access only AppSettings key/value pairs.

### Copy Config file to Output Directory
When you add the `rocklib.config.json` file to your project you will want to make sure the file is set to always Copy to Output Directory.  To configure this follow the steps below

1. Add the file to your solution
2. Right click the file -> Properties (should open up the property page for the file)
3. Set the `Copy to Output Directory` to `Copy Always`

Here is an example of the base app.config.json file
```
{
  "appSettings": {
    "Key1": "somekey,
    "Key2": "anotherkey"
  }
}

```

### Application Configuration Setup
The App.Config or Web.Config file is the standard configuration file used for .NET Framework applications.  If you only need to use the AppSettings values in your application you can ignore the need for the `rocklib.config.json` file.  The RockLib.Configuration library can pull `AppSetting` values from your App.Config or Web.Config file with no issues.

Here is an example of how to store `AppSetting` values in your app/web.config files
```
  <appSettings>
    <add key="Key1" value="Key1_Value"/>
  </appSettings>
```

### Environment Variables Setup
By default, RockLib.Configuration configures itself by reading environment variables. It does so at a higher priority than the `rocklib.config.json` file. This allows some or all of an application's settings to be set via machine, user, or process environment variables. The [Microsoft.Extensions.Configuration documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration#simple-configuration) provides some details on the formatting of environment variables names.

In order to have RockLib.Configuration pull the environment variables you will need to have the key formatted correctly.

To pull values as `AppSetting` key/values you will want to format the key as below
```
AppSettings:test_key1
```

If you want to pull environment variables and use them as custom sections, this is also allowed.  To do this, assume you have a section called foo_section.  When you create your variables you will need to prefix your keys with foo_section as below.

```
foo_section:Bar
foo_section:Baz
```


## How To Use

### Accessing App Settings
In order to access `AppSettings` you can use the .AppSettings propety and provide  the desired key.

```
var key1Value = Config.AppSettings["Key1"];
```

If the provided key is not found a KeyNotFoundException will be throw.

#### Example Configuration Files

rocklib.config.json
```
{
  "appSettings": {
    "Key1": "somekey
  }
}
```

App.Config
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="Key100" value="Key100_Value"/>
  </appSettings>
</configuration>
```

### Accessing Connection Strings
In order to access connection strings you can use the .GetConnectionString() method and provide it a name of the connection string.

```
var defaultConnectionString = Config.Root.GetConnectionString("Default");
```


rocklib.config.json
```
{
  "ConnectionStrings": {
    "Default": "schema://path"
  }
}
```

### Accessing Custom Sections
If you would like to create a custom section, this is very straight forward to do.  You will first need to create a custom section in your `rocklib.config.json` file as below.

```
"Foo": {
    "Bar": 123,
    "Baz": "abc",
    "Qux": true
}
```

You will then want to create an object model that maps to your section (via json serialization)

```
class FooSection
{
    public int Bar { get; set; }
    public string Baz { get; set; }
    public bool Qux { get; set; }
}
```

Finally you can access the section by the GetSection() method and provide it the name of the section.

```
var foo = Config.Root.GetSection("Foo").Get<FooSection>();
```

## Example Application
If you want to see how the Rocklib.Configuration works you can do this by looking *.Example application in the repository.
