# RockLib.Configuration.ObjectFactory Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".

## 1.6.8 - 2021-05-06

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates System.Reflection.Emit package to latest version.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Configuration.ObjectFactory. What follows below are the original release notes.

----

## 1.6.7

Relaxes restrictions around types that implement IEnumerable. Only implementations of IEnumerable from the System.Collections namespace are disallowed.

## 1.6.6

Adds net5.0 target.

## 1.6.5

Fixes bug when a constructor has an optional nullable enum parameter with default value of null and the parameter was not supplied.

## 1.6.4

Adds support for optional nullable enumerations in contructors.

## 1.6.3

Adds icon to project and nuget package.

## 1.6.2

Updates to align with Nuget conventions.

## 1.6.1

Adds the [ConfigSection] attribute, for use by configuration editing tools.

## 1.6.0

Adds the [AlternateName] attribute, allowing users to specify one or more alternate names for a constructor parameter / writable property.

## 1.5.0

Adds support for members of type `Func<T>`.

## 1.4.0

Adds support for arbitrary DI containers, such as Ninject or Unity.

## 1.3.1

Improves the debugging experience of config reloading proxy objects.

## 1.3.0

Adds the ability to handle identifiers with different casing strategies - PascalCase, camelCase, snake_case, and kebab-case.

## 1.2.3

Fixes two bugs related to members of type `object`:

- If the target type is `object`, and it is being created by a configuration that has a value, then just use the configuration value directly. Previously, a conversion error would occur.
- If the target type is `object`, and it has a default type of string-to-anything dictionary, and if the configuration has an "object" shape, then create the string dictionary from configuration. Previously, an nonsensical error would occur stating that it couldn't convert the dictionary type to the dictionary type.

## 1.2.2

Improvements around when not to reload:

- A reloading proxy doesn't reload if its configuration section hasn't changed.
- If a section has `reloadOnChange` explicitly configured to be `false`, then the `CreateReloadingProxy` extension method returns a regular object, not a config reloading proxy object.
- A small expansion of the definition of what a "type-specified object" is to allow a `reloadOnChange` of `false`.

## 1.2.1

Fixes a constructor selection bug.

## 1.2.0

Adds the ability to create a "config reloading proxy" - an object that reloads itself when its configuration changes.

## 1.1.4

Adds support for IReadOnlyDictionary.

## 1.1.3

Fixes a constructor selection bug.

## 1.1.2

Fixes bug where top-level object wouldn't get its default type applied.

## 1.1.1

This version enables read-only properties with a type that implements non-generic IList and does not have a public parameterless constructor.

## 1.1.0

Adds support for non-generic lists.

## 1.0.0

Initial release.
