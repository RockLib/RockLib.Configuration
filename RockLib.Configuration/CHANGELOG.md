# RockLib.Configuration Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 5.0.0 - 2025-01-29

#### Changed
- Updated `CompositeConfigurationSection`, `MemoryConfigurationBuilderExtensions`, and `Config` members for nullability.
- Updated Microsoft NuGet package references to latest 8.* versions.

#### Removed
- Removed support for .NET 6.

## 4.0.3 - 2024-07-12

#### Added
- Direct dependency on System.Text.Json to .NET 6 package to resolve vulnerability.
- Updated multiple dependencies in the test projects.

## 4.0.2 - 2024-10-10

#### Changed
- Re-release to pull and build with the latest System.Text.Json to fix vulnerability.

## 4.0.1 - 2024-07-12

#### Added
- Direct dependency on System.Text.Json to .NET 8 package to resolve vulnerability.

## 4.0.1-alpha.1 - 2024-07-12

#### Added
- Direct dependency on System.Text.Json to .NET 8 package to resolve vulnerability.

## 4.0.0 - 2024-02-14

#### Changed
- Finalized 4.0.0 version.

## 4.0.0-alpha.1 - 2023-12-14

#### Changed
- Removed .NET Core 3.1 as a TFM and added .NET 8. Supported targets are now .NET 4.8, .NET 6, and .NET 8.
- Removed RockLib.Immutable as a dependency, and added `Semimutable<>` and `SoftLock<>` internally.

## 3.1.0 - 2023-03-24
	
#### Added
- Added support for generic host projects to allow loading of correct environment appsettings files.

## 3.1.0-alpha.1 - 2023-03-21
	
#### Added
- Added support for generic host projects to allow loading of correct environment appsettings files.

## 3.0.0 - 2022-02-10
	
#### Added
- Added `.editorconfig` and `Directory.Build.props` files to ensure consistency.

#### Changed
- Supported targets: net6.0, netcoreapp3.1, and net48.
- As the package now uses nullable reference types, some method parameters now specify if they can accept nullable values.
- `Config.ResetRoot()` relies on an private method that no longer silently catches all exceptions.

## 2.5.3 - 2021-08-11

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Immutable to latest version, [1.0.7](https://github.com/RockLib/RockLib.Immutable/blob/main/RockLib.Immutable/CHANGELOG.md#107---2021-08-10).

## 2.5.2 - 2021-05-06

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Immutable package to latest version, which includes SourceLink.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Configuration. What follows below are the original release notes.

----

## 2.5.1

Adds net5.0 target.

## 2.5.0

Adds SetBasePath method to Config static class, allowing users to opt-in to setting the base path of the configuration builder when using the default config root.

## 2.4.4

Adds icon to project and nuget package.

## 2.4.3

Update dependency package.

## 2.4.2

Updates to align with Nuget conventions.

## 2.4.1

Fixes the list section ordering bug.

## 2.4.0

Adds extension methods to create a composite configuration section from a configuration.

## 2.3.1

Fixes a case sensitivity bug in the configuration manager provider.

## 2.3.0

- Adds a `reloadOnChange` flag to the `AddAppSettingsJson` (for all targets) and `AddConfigurationManager` (for .NET Framework targets) extension methods.
- The default value of `Config.Root` uses both extension methods and sends a value of true for each.

## 2.2.1

Fix support for web projects with Web.Config

## 2.2.0

- Adds support for RockLib.Secrets without a hard dependency
- Adds .SetConfigRoot extension for IConfigurationBuilder

## 2.1.0

For .NET Framework applications/libraries, the `AddConfigurationManager` extension method adds any `RockLibConfigurationSection` sections declared in app.config/web.config to the configuration builder. The default value of Config.Root also includes these sections.

## 2.0.0

Changes the type of Config.Root from IConfigurationRoot to IConfiguration.

## 1.0.0

Initial release.
