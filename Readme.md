PowerMate
===

[![Nuget](https://img.shields.io/nuget/v/PowerMate?logo=nuget)](https://www.nuget.org/packages/PowerMate/) [![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aldaviva/PowerMate/dotnetpackage.yml?branch=master&logo=github)](https://github.com/Aldaviva/PowerMate/actions/workflows/dotnetpackage.yml) [![Testspace](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:PowerMate/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/204917) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/PowerMate?logo=coveralls)](https://coveralls.io/github/Aldaviva/PowerMate?branch=master)

*Receive events from a Griffin PowerMate device over USB*

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="false" levels="1,2,3" bullets="1.,-,-,-" -->

1. [Quick Start](#quick-start)
1. [Prerequisites](#prerequisites)
1. [Installation](#installation)
1. [Usage](#usage)
    - [Connections](#connections)
1. [Notifications](#notifications)
    - [`InputReceived` event](#inputreceived-event)
    - [`IsConnectedChanged` event](#isconnectedchanged-event)
1. [Light Control](#light-control)
    - [`LightBrightness` property](#lightbrightness-property)
    - [`LightAnimation` property](#lightanimation-property)
    - [`LightPulseSpeed` property](#lightpulsespeed-property)
1. [Demos](#demos)
    - [Simple demo](#simple-demo)
    - [Volume control](#volume-control)
1. [Limitations](#limitations)
1. [Acknowledgements](#acknowledgements)

<!-- /MarkdownTOC -->

![Griffin PowerMate](https://raw.githubusercontent.com/Aldaviva/PowerMate/master/.github/images/readme-header.jpg)

## Quick Start
```ps1
dotnet install PowerMate
```

```cs
using PowerMate;

using IPowerMateClient powerMate = new PowerMateClient();

powerMate.InputReceived += (sender, input) => {
    switch (input) {
        case { IsPressed: true, RotationDirection: RotationDirection.None }:
            Console.WriteLine("PowerMate was pressed");
            break;
        case { IsPressed: false, RotationDirection: RotationDirection.Clockwise }:
            Console.WriteLine("PowerMate was rotated clockwise");
            break;
        case { IsPressed: false, RotationDirection: RotationDirection.Counterclockwise }:
            Console.WriteLine("PowerMate was rotated counterclockwise");
            break;
        default:
            break;
    }
};
```

## Prerequisites

- A [Griffin PowerMate](https://en.wikipedia.org/wiki/Griffin_PowerMate)
    - ✅ The [USB version](https://www.amazon.com/gp/product/B003VWU2WA/ref=oh_details_o00_s00_i00?ie=UTF8&psc=1) is supported
    - ❌ The Bluetooth version is not supported
- Any Microsoft .NET runtime that supports [.NET Standard 2.0 or later](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#net-standard-versions)
    - [.NET 5.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Core 2.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet)
    - [.NET Framework 4.6.1 or later](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
- A supported operating system
    - ✅ Windows
        - verified on 10 22H2 x64
        - verified on 11 22H2 x64
    - ✅ MacOS
        - verified on 12.6 x64
    - ❌ Linux is not supported
        - on Fedora 37, Debian 11, Raspbian 11, and openSUSE 15, the PowerMate is never detected by [HIDSharp](https://www.nuget.org/packages/HidSharp/), even though it appears with `lsusb`

## Installation

You can install this library into your project from [NuGet Gallery](https://www.nuget.org/packages/PowerMate):
- `dotnet add package PowerMate`
- `Install-Package PowerMate`
- Go to Project › Manage NuGet Packages in Visual Studio and search for `PowerMate`

## Usage

1. **Construct** a new instance of the `PowerMateClient` class.

    ```cs
    using IPowerMateClient powerMate = new PowerMateClient();
    ```

    You should dispose of instances when you're done with them by calling `Dispose()`, or with a `using` statement or declaration.

1. Now you can **listen for [`InputReceived`](#inputreceived-event) events** from the client.

    ```cs
    powerMate.InputReceived += (sender, input) => Console.WriteLine($"Received PowerMate event: {input}");
    ```

### Connections

This library will automatically try to connect to one of the PowerMate devices that are plugged into your computer. If no device is connected, it will automatically wait until one appears and then connect to it. If a device disconnects, this library will reconnect automatically when one reappears.

If multiple PowerMate devices are present simultaneously, this library will pick one of them arbitrarily and use it until it disconnects.

The connection state is exposed by the **`bool IsConnected`** property, and changes to this state are emitted by the [`IsConnectedChanged`](#isconnectedchanged-event) event.

```cs
Console.WriteLine(powerMate.IsConnected 
    ? "Listening for movements from PowerMate." 
    : "Waiting for a PowerMate to be connected.");

powerMate.IsConnectedChanged += (_, isConnected) => Console.WriteLine(isConnected 
    ? "Reconnected to a PowerMate." 
    : "Disconnected from the PowerMate, attempting reconnection...");
```

## Notifications

### `InputReceived` event

Fired whenever the PowerMate knob is rotated, pressed, or released.

```cs
powerMate.InputReceived += (sender, input) => Console.WriteLine($"Received PowerMate event: {input}");
```

The event argument is a `PowerMateInput` struct with the following fields.

|Field name|Type|Example values|Description|
|-|-|-|-|
|`IsPressed`|`bool`|`true` `false`|`true` if the knob is being held down, or `false` if it is up. Pressing and releasing the knob will generate two events, with `true` and `false` in order. Will also be `true` when the knob is rotated while being held down.|
|`RotationDirection`|`enum`|`None` `Clockwise` `Counterclockwise`|The direction the knob is being rotated when viewed from above, or `None` if it is not being rotated.|
|`RotationDistance`|`uint`|`0` `1` `2`|How far, in arbitrary angular units, the knob was rotated since the last update. When you rotate the knob slowly, you will receive multiple events, each with this set to `1`. As you rotate it faster, updates are batched and this number increases to `2` or more. The highest value I have seen is `8`. This is always non-negative, regardless of the rotation direction; use `RotationDirection` to determine the direction. If the knob is pressed without being rotated, this is `0`.|

### `IsConnectedChanged` event

Fired whenever the connection state of the PowerMate changes. Not fired when constructing or disposing the `PowerMateClient` instance.

The event argument is a `bool` which is `true` when a PowerMate has reconnected, or `false` when it has disconnected.

To get the value of this state at any time, read the `IsConnected` property on the `IPowerMateClient` instance.

```cs
powerMate.IsConnectedChanged += (_, isConnected) => Console.WriteLine(isConnected 
    ? "Reconnected to a PowerMate." 
    : "Disconnected from the PowerMate, attempting reconnection...");
```

## Light Control

By default, the blue/cyan LED in the base of the PowerMate lights up when it's plugged in. You can change the intensity of the light, make it pulse at different frequencies, or turn it off entirely by setting the following properties. The values that you set are retained across reconnections.

### `LightBrightness` property

A writable `byte` property with valid values in the range [0, 255]. `0` represents off, and `255` is the brightest.

The default value is `80`, which matches the brightness the device uses when no program has instructed it to change its brightness since it has been plugged in.

Changes to `LedBrightness` will not take effect while the LED is pulsing with a [`LightAnimation`](#lightanimation-property) of `Pulsing`.

```cs
powerMate.LightBrightness = 0;
```
```cs
powerMate.LightBrightness = 255;
```

### `LightAnimation` property

A writable `enum` property that controls how the light is animated. Available values are

- **`Solid`** *(default)* — the light shines at a constant brightness, controlled by [`LightBrightness`](#lightbrightness-property)
- **`Pulsing`** — the light raises and lowers its brightness to the highest and lowest levels in a cyclical animation, with the frequency controlled by [`LightPulseSpeed`](#lightpulsespeed-property)
- **`SolidWhileAwakeAndPulsingDuringComputerStandby`** — while the computer to which the device is connected is awake, the light shines at a constant brightness (controlled by [`LightBrightness`](#lightbrightness-property)), but while the computer is asleep, the light displays the pulsing animation (with the frequency controlled by [`LightPulseSpeed`](#lightpulsespeed-property))

### `LightPulseSpeed` property

A writable `int` property with valid values in the range [0, 24]. Values outside that range are clamped. The default value is `12`.

This property controls how fast the light pulses when [`LightAnimation`](#lightanimation-property) is set to `Pulsing` or `SolidWhileAwakeAndPulsingDuringComputerStandby`. The slowest pulse speed, `0`, is about 0.03443 Hz, or a 29.04 sec period. The fastest pulse speed, `24`, is about 15.63 Hz, or a 64 ms period.

```cs
powerMate.LedPulseSpeed = 12;
```

## Demos

### Simple demo

This console program just prints out each event it receives from the PowerMate.

- Windows: [x64](https://github.com/Aldaviva/PowerMate/releases/latest/download/Demo-x64.exe), [ARM64](https://github.com/Aldaviva/PowerMate/releases/latest/download/Demo-ARM64.exe)
- MacOS: [x64](https://github.com/Aldaviva/PowerMate/releases/latest/download/Demo-x64), [ARM64](https://github.com/Aldaviva/PowerMate/releases/latest/download/Demo-ARM64)
- [Source](https://github.com/Aldaviva/PowerMate/blob/master/Demo/Demo.cs)

```text
> Demo-x64.exe
Listening for PowerMate events
Received event from PowerMate: Turning clockwise 1 increment while not pressed
Received event from PowerMate: Turning clockwise 1 increment while not pressed
Received event from PowerMate: Turning clockwise 1 increment while not pressed
^C
```

### Volume control

- Windows: [x64](https://github.com/Aldaviva/PowerMate/releases/latest/download/PowerMateVolume.exe)
- [Source](https://github.com/Aldaviva/PowerMate/blob/master/PowerMateVolume/PowerMateVolume.cs)

This background program increases and decreases the output volume of the default Windows audio output device when you turn the PowerMate knob clockwise and counterclockwise. It also toggles the output mute when you press the knob.

```cmd
PowerMateVolume.exe
```

By default, it increments and decrements the volume by 1 percentage point for each rotation event. You can customize this amount by passing a different percentage as a command-line argument, such as ½ or 2 percentage points:

```cmd
PowerMateVolume.exe 0.005
```
```cmd
PowerMateVolume.exe 0.02
```

## Limitations
- This library currently does not allow you to change the LED brightness.

## Acknowledgements
- [![Luke Ma](https://pbs.twimg.com/profile_images/599239611584380928/g7DAnpuw_normal.jpg) **Luke Ma**](https://twitter.com/lukesma) for giving me a PowerMate as a Christmas gift in 2013