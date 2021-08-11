# RockLib.Configuration Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".

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
