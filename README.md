# RegScope

Windows Registry Search and Cleanup Tool

![RegScope Main Window](RegScope/Docs/Images/app.png)

RegScope is a Windows desktop application for searching, reviewing, and safely removing Windows Registry keys and values across multiple registry hives.

Built with WPF on .NET 9 using the MVVM pattern and packaged with WiX Toolset.

## Features

* Search registry key names.
* Search registry value names.
* Search registry value data.
* Search within a specific registry hive or across all supported hives.
* Scan both 64-bit and 32-bit registry views on 64-bit Windows.
* Display grouped search results for easier navigation.
* Review matching keys and values with their registry paths, value types, and data.
* Select and delete individual registry keys or values.
* Bulk delete selected registry items.
* Confirmation dialogs before deletion.
* Application logging using Serilog.
* Automatic MSI installer generation with WiX Toolset.

## Technology Stack

* .NET 9
* WPF
* MVVM
* CommunityToolkit.Mvvm
* Microsoft.Win32.Registry
* Serilog
* xUnit.net v3
* WiX Toolset 6

## Requirements

### Development

* Windows 10 / Windows 11
* .NET 9 SDK
* Visual Studio 2022 (optional for command-line builds)

### Runtime

* Windows 10 / Windows 11
* .NET 9 Desktop Runtime (x64 for the MSI installer)
* Administrator privileges may be required to access protected registry locations

## Build

Restore dependencies:

```powershell
dotnet restore RegScope.sln
```

Build the solution:

```powershell
dotnet build RegScope.sln -c Release -p:Platform=x64
```

Build only the desktop application:

```powershell
dotnet build RegScope\RegScope.csproj -c Release
```

## Publish

Create a publish output for deployment:

```powershell
dotnet publish RegScope\RegScope.csproj `
    -c Release `
    -r win-x64 `
    --self-contained false
```

## Build MSI Installer

The installer project is located in:

```text
RegScope.Installer
```

Build the MSI installer:

```powershell
dotnet build RegScope.Installer\RegScope.Installer.wixproj -c Release -p:Platform=x64
```

The generated MSI package will be available in:

```text
RegScope.Installer\bin\Release\x64\
```

If Visual Studio was already open after installer project changes, reload the solution before rebuilding.

## Usage

1. Enter the search text.
2. Select a registry hive or choose **All Registry**.
3. Click **Search**.
4. Review the grouped results.
5. Select the registry items you want to remove.
6. Click **Delete Selected**.
7. Confirm the deletion operation.

## Administrator Rights

Some registry locations require elevated permissions.

When accessing or removing protected registry entries, run RegScope as Administrator.

## Logging

Application logs are stored in a `Logs` directory next to the application when that location is writable. Otherwise, they are stored in:

```text
%LOCALAPPDATA%\RegScope\Logs
```

## Project Structure

```text
RegScope/                    WPF application and view models
RegScope.Services/           Registry search and delete services
RegScope.Presentation/       UI styles, resources, and controls
RegScope.Logger/             Application logging
RegScope.Tests/              Automated tests
RegScope.Installer/          WiX MSI installer project
```

## Warning

Deleting registry data can break applications or Windows configuration.

Always create a backup, restore point, or registry export before removing registry entries.
