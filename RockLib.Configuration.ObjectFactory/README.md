# RockLib.Configuration.ObjectFactory [![Build status](https://ci.appveyor.com/api/projects/status/ox9velgud5ljj8d0?svg=true)](https://ci.appveyor.com/project/bfriesen/rocklib-configuration-owv0n)

An alternative to `Microsoft.Extensions.Configuration.Binder` that supports non-default constructors and other features commonly found in JSON and XML serializers.

## Table Of Contents

- [Overview](#overview)
- [Non-default constructors](#non-default-constructors)
- [Type-specified values](#type-specified-values)
- [Lists](#lists)
- [Default Types](#default-types)
- [Value Converters](#value-converters)

### Overview

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

```c#
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

```c#
IConfiguration configuration; // TODO: load from json
Foo foo = configuration.Create<Foo>();

// Non-generic overload is also available.
Foo foo2 = (Foo)configuration.Create(typeof(Foo));
```

### Non-default constructors

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

```c#
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

```c#
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

### Type-specified values

Sometimes, it is necessary to specify a derived type for a value in the configuration. For example, given the following types defined in assembly `MyAssembly`:

```c#
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

a instance of `Foo` can be created with a `Baz` property with a value of type `BazDerived` by specifying a `type/value` pair where the `type` is a *[assembly quallified name](https://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname.aspx)*:

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

```c#
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

### Lists

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

If a class has a list-type property, but the application only want to define one item, the configuration can be flattened. For the `Foo` class:

```c#
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

### Default Types

### Value Converters
