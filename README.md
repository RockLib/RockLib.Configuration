This repository contains the source code for three nuget packages:

### [RockLib.Configuration](RockLib.Configuration) [![Build status](https://ci.appveyor.com/api/projects/status/0qxs1k1bw36cn8ly?svg=true)](https://ci.appveyor.com/project/bfriesen/rocklib-configuration)
  
  Defines a static `Config` class as a general replacement for the old .NET Framework `ConfigurationManager` class.

### [RockLib.Configuration.ObjectFactory](RockLib.Configuration.ObjectFactory) [![Build status](https://ci.appveyor.com/api/projects/status/ox9velgud5ljj8d0?svg=true)](https://ci.appveyor.com/project/bfriesen/rocklib-configuration-owv0n)

An alternative to `Microsoft.Extensions.Configuration.Binder` that supports non-default constructors and other features commonly found in JSON and XML serializers.

### [RockLib.Configuration.ProxyFactory](RockLib.Configuration.ProxyFactory) [![Build status](https://ci.appveyor.com/api/projects/status/kc7repd7vjplu7ls?svg=true)](https://ci.appveyor.com/project/RockLib/rocklib-configuration-u6yve)

A factory that creates instances of property-only interfaces, defined at run-time, and populated with values defined in an instance of `IConfiguration`.
