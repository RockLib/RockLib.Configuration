This repository contains the source code for four nuget packages:

### [RockLib.Configuration](RockLib.Configuration/README.md)

  Defines a static `Config` class as a general replacement for the old .NET Framework `ConfigurationManager` class.

### [RockLib.Configuration.ObjectFactory](RockLib.Configuration.ObjectFactory/README.md)

An alternative to `Microsoft.Extensions.Configuration.Binder` that supports non-default constructors and other features commonly found in JSON and XML serializers.

### [RockLib.Configuration.ProxyFactory](RockLib.Configuration.ProxyFactory/README.md)

A factory that creates instances of property-only interfaces, defined at run-time, and populated with values defined in an instance of `IConfiguration`.

### [RockLib.Configuration.MessagingProvider](RockLib.Configuration.MessagingProvider/README.md)

A configuration provider that reloads when it receives a message containing configuration changes from a `RockLib.Messaging.IReceiver`.
