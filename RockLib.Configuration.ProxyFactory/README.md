# RockLib.Configuration.ProxyFactory

A factory that creates instances of property-only interfaces, defined at run-time, and populated with values defined in an instance of `IConfiguration`.

> [!WARNING]  
> The 4.0.0 release of this library will be the final version with upgrades and changes. Bug fixes will continue to be released as needed. We strongly encourage developers to use standard .NET configuration libraries directly like `Microsoft.Extensions.Configuration` in place of `RockLib.Configuration`.

## Supported Targets

This library supports the following targets:
  - .NET 6
  - .NET Core 3.1
  - .NET Framework 4.8

## Overview

Let's say we have a class, `Foo`, that has various settings that it requires.

```csharp
public class Foo
{
    private readonly IFooSettings _settings;
    public Foo(IFooSettings settings) => _settings = settings;
    // TODO: Define methods that use the settings
}

public interface IFooSettings
{
    bool IsBar { get; }
    int BazCount { get; }
    string QuxCapacitor { get; }
}
```

When unit testing our `Foo` class, it is easy to use any mocking framework to define each of the settings as needed for each test. But when our class is being used in production, we'll want these values to come from configuration. In order to do this, we could use the `Create` extension method from `RockLib.Configuration.ObjectFactory` or the `Get` extension method from `Microsoft.Extensions.Configuration.Binder`. But those extension methods require a concrete target, so we would need to first define an implementation class for the settings interface. That implementation class is low-value, copy/paste code that we would like to avoid having to write.

The `CreateProxy` extension method fills the gap by creating a concrete implementation type at run-time - a "proxy" class. It then uses that proxy class as the target type for the `Create` extension method from `RockLib.Configuration.ObjectFactory` and returns the result of that call.

```csharp
IConfiguration configuration; // TODO: get from somewhere
IFooSettings settings = configuration.CreateProxy<IFooSettings>();
Foo foo = new Foo(settings);
```
