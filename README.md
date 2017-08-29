# RockLib Configuration
All RockLib packages will depend on the RockLib.Configuration package to provide its configuration features.

## Table Of Contents
* [Steps to Success](#steps-to-success)
* [Nuget Packages](#packages)
* RockLib.config.json
  * [Setup App Config](#setup-app-config)
* [How to use](#how-to-use)
* [Example Application](#example-application)

## Steps to Success
In order go get the RockLib.Configuration provider to work inside your application you will need to complete the following steps

1. Install all the required [NuGet](#packages) packages.
2. Create and configure the RockLib.config.json file (this is the application's configuration file)

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

## Setup App Config
You will need to have an RockLib.config.json file associated with your application.

### Copy Config file to Output Directory
When you add the RockLib.config.json file to your project you will want to make sure the file is set to always Copy to Output Directory.  To configure this follow the steps below

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

## How To Use

### Accessing App Settings
In order to access app settings you can use the .AppSettings propety and provide it the desired key.

```
var key1Value = Config.AppSettings["Key1"];
```

### Accessing Connection Strings
In order to access connection strings you can use the .GetConnectionString() method and provide it a name of the connection string.

```
var defaultConnectionString = Config.Root.GetConnectionString("Default");
```

### Accessing Custom Sections
If you would like to create a custom section, this is very straight forward to do.  You will first need to create a custom section in your RockLib.config.json file as below.

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