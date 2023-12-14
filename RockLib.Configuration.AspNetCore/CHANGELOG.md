# RockLib.Configuration.AspNetCore Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 3.0.0-alpha.1 - Not Yet Released

#### Changed
- Removed .NET Core 3.1 as a TFM and added .NET 8. Supported targets are now .NET 4.8, .NET 6, and .NET 8.

## 2.0.0 - 2022-02-15

#### Added

- Added .editorconfig and Directory.Build.props files to ensure consistency.

#### Changed

- Supported targets: net6.0, netcoreapp3.1, and net48.
- As the package now uses nullable reference types, some method parameters now specify if they can accept nullable values.

## 1.0.5 - 2021-08-11

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Configuration to latest version, [2.5.3](https://github.com/RockLib/RockLib.Configuration/blob/main/RockLib.Configuration/CHANGELOG.md#253---2021-08-11).

## 1.0.4 - 2021-05-06

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Configuration package to latest version, which includes SourceLink.
- Updates Microsoft.AspNetCore.Hosting.Abstractions package to latest version.
- For net5.0, use framework reference instead of package reference.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Configuration.AspNetCore. What follows below are the original release notes.

----

## 1.0.3

Adds net5.0 target.

## 1.0.2

Adds icon to project and nuget package.

## 1.0.1

Updates to align with Nuget conventions.

## 1.0.0

Adds `SetConfigRoot` extension method for `IWebHostBuilder`.
