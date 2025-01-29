# RockLib.Configuration.ObjectFactory

An alternative to `Microsoft.Extensions.Configuration.Binder` that supports non-default constructors and other features commonly found in JSON and XML serializers.

> [!WARNING]  
> The 4.0.0 release of this library will be the final version with upgrades and changes. Bug fixes will continue to be released as needed. We strongly encourage developers to use standard .NET configuration libraries directly like `Microsoft.Extensions.Configuration` in place of `RockLib.Configuration`.

## Table Of Contents

- [Overview](#overview)
- [Non-default constructors](#non-default-constructors)
- [Type-specified values](#type-specified-values)
- [Lists](#lists)
- [Dictionaries](#dictionaries)
- [Default Types](#default-types)
- [Value Converters](#value-converters)

## Overview

The `RockLib.Configuration.ObjectFactory` package defines a `Create` extension method that behaves very similar to the `Get` extension method from the `Microsoft.Extensions.Configuration.Binder` package. Both extension methods extend the `IConfiguration` interface and create an object of a specified type with values contained in the `IConfiguration` intstance. The `Create` extension method also has additional features, as described in later sections.

For example, an `IConfiguration` created with this json file:

```json
{
    "bar": 123,
    "baz": {
        "qux": "abc",
        "corge": true
    }
}
```

can be mapped to an instance of the `Foo` class:

```csharp
public class Foo
{
    public int Bar { get; set; }
    public Baz Baz { get; set; }
}

public class Baz
{
    public string Qux { get; set; }
    public bool Corge { get; set; }
}
```

by making this call:

```csharp
IConfiguration configuration; // TODO: load from json
Foo foo = configuration.Create<Foo>();

// Non-generic overload is also available.
Foo foo2 = (Foo)configuration.Create(typeof(Foo));
```

## Non-default constructors

The `Create` extension method supports mapping to types without default constructors. It does so by matching config setting names to constructor parameter names. For example:

```json
{
    "bar": 123,
    "baz": {
        "qux": "abc",
        "corge": true
    }
}
```

can be mapped to an instance of the `Foo` class:

```csharp
public class Foo
{
    public Foo(int bar, Baz baz)
    {
        Bar = bar;
        Baz = baz;
    }

    public int Bar { get; }
    public Baz Baz { get; }
}

public class Baz
{
    public Baz(string qux, bool corge)
    {
        Qux = qux;
        Corge = corge;
    }

    public string Qux { get; }
    public bool Corge { get; }
}
```

Note that a public property corresponding to the constructor parameter does not have to exist in order for the mapping to occur. The same configuration can be mapped to this version of `Foo` as well:

```csharp
public class Foo
{
    public Foo(int bar, Baz baz)
    {
    }
}

public class Baz
{
    public Baz(string qux, bool corge)
    {
    }
}
```

## Type-specified values

Sometimes, it is necessary to specify a derived type for a value in the configuration. For example, given the following types defined in assembly `MyAssembly`:

```csharp
namespace MyNamespace
{
    public class Foo
    {
        public int Bar { get; set; }
        public Baz Baz { get; set; }
    }

    public class Baz
    {
        public string Qux { get; set; }
    }

    public class BazDerived : Baz
    {
        public bool Corge { get; set; }
    }
}
```

a instance of `Foo` can be created with a `Baz` property with a value of type `BazDerived` by specifying a `type/value` pair where the `type` is a *[assembly qualified name](https://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname.aspx)*:

```json
{
    "bar": 123,
    "baz": {
        "type": "MyNamespace.BazDerived, MyAssembly",
        "value": {
            "qux": "abc",
            "corge": true
        }
    }
}
```

Being able to specify a type means that interfaces and abstract classes are supported.

```csharp
public class Foo
{
    public IBar Bar { get; set; }
    public BazBase Baz { get; set; }
}

public interface IBar
{
}

public class Bar : IBar
{
    public int Qux { get; set; }
}

public abstract class BazBase
{
}

public class Baz : BazBase
{
    public bool Corge { get; set; }
}
```

is mappable from:

```json
{
    "bar": {
        "type": "MyNamespace.Bar, MyAssembly",
        "value": {
            "qux": "abc"
        }
    },
    "baz": {
        "type": "MyNamespace.Baz, MyAssembly",
        "value": {
            "corge": true
        }
    }
}
```

## Lists

RockLib.Configuration.ObjectFactory currently supports the following list types:

- Any single-dimension array
- `List<T>`
- `IList<T>`
- `ICollection<T>`
- `IEnumerable<T>`
- `IReadOnlyCollection<T>`
- `IReadOnlyList<T>`

If a list property is readonly, one of the following conditions must be met:

- The property type is one of the following:
  - `List<T>`
  - `IList<T>`
  - `ICollectionList<T>`
- The property type implements `IList` and has a single `Add` method that has one parameter with a type other than `object`.

If a class has a list-type property, but the application only wants to define one item, the configuration can be flattened. For the `Foo` class:

```csharp
public class Foo
{
    public IEnumerable<Bar> Bars { get; set; }
}

public class Bar
{
    public string Baz { get; set; }
    public int Qux { get; set; }
}
```

instead of a configuration like this:

```json
{
    "bars": [
        {
            "baz": "abc",
            "qux": 123
        }
    ]
}
```

can be rewritten like this:

```json
{
    "bars": {
        "baz": "abc",
        "qux": 123
    }
}
```

## Dictionaries

RockLib.Configuration.ObjectFactory currently supports the following dictionary types - note that the key of each dictionary type must be `string`:

- `Dictionary<string, TValue>`
- `IDictionary<string, TValue>`
- `IReadOnlyDictionary<string, TValue>`

If a dictionary property is readonly, it must be one of the following dictionary types:
- `Dictionary<string, TValue>`
- `IDictionary<string, TValue>`

Dictionaries take the form of regular objects in configuration. For example, in the following Foo class:

```csharp
public class Foo
{
    public Foo(IReadOnlyDictionary<string, int> bar)
    {
        Bar = bar;
    }

    public IReadOnlyDictionary<string, int> Bar { get; }
}
```

A configuration might look like this:

```json
{
    "bar": {
        "baz": 123,
        "qux": 456
    }
}
```

## Default Types

Given a type `Foo` that has an abstract property `Bar`:

```csharp
public class Foo
{
    public IBar Bar { get; set; }
}

public interface IBar
{
}

public class DefaultBar : IBar
{
    public int Baz { get; set; }
}
```

We would like to be able to define the `DefaultBar` type as the default type - if the type is not otherwise specified, create an instance of `DefaultBar`. There are two ways of specifying the default type: by property or by type. When specified by property, we want to say: "when setting the `Foo.Bar` property, if the configuration is *not* type-specified, set the property to an instance of `DefaultBar`." When specified by type we want to say: "whenever creating an instance of `IBar`, if the configuration is *not* type-specified, create an instance of `DefaultBar`.

To programmatically set `DefaultBar` as the default type for the `Foo.Bar` property, call the `Create` extension method as follows:

```csharp
DefaultTypes defaultTypes =
    new DefaultTypes().Add(typeof(Foo), nameof(Foo.Bar), typeof(DefaultBar));

Foo foo = configuration.Create<Foo>(defaultTypes: defaultTypes);
```

To programmatically set `DefaultBar` as the default type for the `IBar`, call the `Create` extension method as follows:

```csharp
DefaultTypes defaultTypes =
    new DefaultTypes().Add(typeof(IBar), typeof(DefaultBar));

var foo = configuration.Create<Foo>(defaultTypes: defaultTypes);
```

Default types can also be specified via attributes, so that the `defaultTypes` parameter in the `Create` extension method can be omitted.

```csharp
public class Foo
{
    [DefaultType(typeof(DefaultBar))]
    public IBar Bar { get; set; }

    public IBaz Baz { get; set; }
}

public interface IBar
{
}

public class DefaultBar : IBar
{
    public string Qux { get; set; }
}

[DefaultType(typeof(DefaultBaz))]
public interface IBaz
{
}

public class Baz : IBaz
{
    public bool Corge { get; set; }
}
```

## Value Converters

RockLib converts most configuration string values to the target type by using the `TypeConverter` obtained by calling `TypeDescriptor.GetConverter(targetType)`. In addition, there is support for target types `Encoding` and `Type`. If value conversions need to be supported for additional types, value converters can be registered. These value converters are functions that have a single string parameter and return the target type. Similar to default types, they can be registered by property or by type.

This example defines a class that has a property of type `System.Numerics.BigInteger`, which does not have a `TypeConverter` defined.

```csharp
public class Foo
{
    public BigInteger Bar { get; set; }
}
```

The `BigInteger.Parse` method meets the criteria for a convert function - it has an overload with one string parameter and returns our target type. To do this programmatically for all `BigInteger` values:

```csharp
ValueConverters valueConverters =
    new ValueConverters().Add(typeof(BigInteger), BigInteger.Parse);

Foo foo = configuration.Create<Foo>(valueConverters: valueConverters);
```

We could also target just the `Foo.Bar` property:

```csharp
ValueConverters valueConverters =
    new ValueConverters().Add(typeof(Foo), nameof(Foo.Bar), BigInteger.Parse);

Foo foo = config.Create<Foo>(valueConverters: valueConverters);
```

There are attributes for value converters, as with default types. In this case, the value of the attribute should be the name of a convert method. The method *must* be static, but can be either public or private.

```csharp
public class Foo
{
    public Bar Bar { get; set; }

    [ConvertMethod(nameof(ParseBaz))]
    public Baz Baz { get; set; }

    private static Baz ParseBaz(string value) => new Baz(bool.Parse(value));
}

[ConvertMethod(nameof(Parse))]
public struct Bar
{
    public Bar(int qux) => Qux = qux;

    public int Qux { get; }

    public static Bar Parse(string value) => new Bar(int.Parse(value));
}

public struct Baz
{
    public Baz(bool corge) => Corge = corge;

    public bool Corge { get; }
}
```
