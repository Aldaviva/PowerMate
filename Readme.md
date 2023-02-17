PowerMate
===

[![Nuget](https://img.shields.io/nuget/v/PowerMate?logo=nuget)](https://www.nuget.org/packages/PowerMate/) [![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aldaviva/PowerMate/dotnetpackage.yml?branch=master&logo=github)](https://github.com/Aldaviva/PowerMate/actions/workflows/dotnetpackage.yml) [![Testspace](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:PowerMate/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/204917) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/PowerMate?logo=coveralls)](https://coveralls.io/github/Aldaviva/PowerMate?branch=master)

*Receive events from a Griffin PowerMate device over USB*

<p><details>
    <summary><strong>Table of Contents</strong></summary>

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="false" levels="1,2,3,4" bullets="1.,-,-,-" -->

1. [Quick Start](#quick-start)
1. [Prerequisites](#prerequisites)
1. [Installation](#installation)
1. [Connections](#connections)
1. [Events](#events)
    - [`OnInput`](#oninput)
        - [`PowerMateEvent` fields](#powermateevent-fields)
1. [Limitations](#limitations)
1. [Acknowledgements](#acknowledgements)

<!-- /MarkdownTOC -->
</details>
</p>

![Griffin PowerMate](https://raw.githubusercontent.com/Aldaviva/PowerMate/master/.github/images/readme-header.jpg)

## Quick Start
```ps1
dotnet install PowerMate
```

```cs
using PowerMate;

using IPowerMateClient powerMate = new PowerMateClient();

powerMate.OnInput += (sender, powerMateEvent) => {
    switch (powerMateEvent) {
        case { IsPressed: true, IsRotationClockwise: null }:
            Console.WriteLine("PowerMate was pressed");
            break;
        case { IsPressed: false, IsRotationClockwise: true }:
            Console.WriteLine("PowerMate was rotated clockwise");
            break;
        case { IsPressed: false, IsRotationClockwise: false }:
            Console.WriteLine("PowerMate was rotated counterclockwise");
            break;
        default:
            break;
    }
};
```

## Prerequisites

- A [Griffin PowerMate](https://en.wikipedia.org/wiki/Griffin_PowerMate)
    - The NA16029 USB version is supported
    - The Bluetooth version is not supported
- Any Microsoft .NET runtime that supports [.NET Standard 2.0 or later](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#net-standard-versions)
    - [.NET 5.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Core 2.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Framework 4.6.1 or later](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
- A supported operating system
    - Windows 11 (verified on 22H2)
    - Windows 10 (verified on 22H2)
    - MacOS is currently unverified
    - Linux is currently unverified

## Installation

You can install this library into your project from [NuGet Gallery](https://www.nuget.org/packages/PowerMate):
- `dotnet add package PowerMate`
- `Install-Package PowerMate`
- Go to Project › Manage NuGet Packages in Visual Studio and search for `PowerMate`

## Connections

This library will automatically try to connect to the first detected PowerMate device that is plugged into your computer. If no device is connected, it will wait until one appears and then connect automatically. If a device disconnects, this library will reconnect automatically when it reappears.

If multiple PowerMate devices are present simultaneously, this library will pick one of them arbitrarily and use it until it disconnects.

## Events

### `OnInput`

Fired whenever the PowerMate knob is moved.

#### `PowerMateEvent` fields

|Field name|Type|Example values|Description|
|-|-|-|-|
|`IsPressed`|`bool`|`true` `false`|`true` if the knob is being held down, or `false` if it is up. Pressing and releasing the knob will generate two events, with `true` and `false` in order. Can also be `true` while the knob is being rotated.|
|`IsRotationClockwise`|`bool?`|`true` `false` `null`|`true` if the knob is being rotated clockwise when viewed from above, `false` if it is being rotated counterclockwise, or `null` if it is not being rotated.|
|`RotationDistance`|`uint`|`0` `1` `2`|How far, in arbitrary angular units, the knob was rotated since the last update. When you rotate the knob slowly, you will receive multiple events, each with this set to `1`. As you rotate it faster, updates are batched and this increases to `2` or more. The highest value I have seen is `8`. This is always non-negative, regardless of the rotation direction; use `IsRotationClockwise` to determine the direction. If the knob is pressed without being rotated, this is `0`.|

## Limitations
- This library currently does not allow you to change the LED brightness.
- This library does not connect to Bluetooth PowerMates.

## Acknowledgements
- [**Luke Ma**](https://twitter.com/lukesma) for giving me a PowerMate as a gift for Christmas in 2013