# Requirements and installation
Understand the system requirements and installation instructions to set up the Microsoft GDK API package.

## Requirements
Understand the system requirements before you install the Microsoft GDK package for Unity (com.unity.microsoft.gdk).

### Microsoft Game Devlopment Kit (GDK)
To use the Microsoft GDK API package, you must use the Microsoft Game Development Kit (GDK). There are two options:
1. Install the Microsoft Game Development Kit (GDK). This is the recommended approach, refer to [Game Development Kit (GDK) documentation](https://learn.microsoft.com/en-us/gaming/gdk/).
2. Build Without Install (BWOI). This is for advanced users, refer to [Using the Microsoft Game Development Kit without installation](build-without-install.md).

### Unity Editor requirements

It’s recommended to use the latest version of the editor stream you're currently using. This package is supported from Unity Editor version 2022.3 and newer.

> [!NOTE]
> The Microsoft GDK API package is supported only on Unity Editor for Windows.

## Install the Microsoft GDK API package

To install the Microsoft GDK API package, follow the instructions in the [Package Manager](https://docs.unity3d.com/Manual/upm-ui-install.html) documentation.

## Recommended packages

It’s recommended to use the [GDK Tools package (com.unity.microsoft.gdk.tools)](https://docs.unity3d.com/Packages/com.unity.microsoft.gdk.tools@latest) along with the Microsoft GDK API package (com.unity.microsoft.gdk). The GDK Tools package manages the additional steps needed to produce a working build. Without this package, you are responsible for ensuring everything is in place to produce a functional build.

The provided samples require the use of the [Unity Input system](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/manual/index.html) package.

## Supported features

The Microsoft GDK API package provides:

* Managed C# wrappers providing access to the native Microsoft GDK API.
* A set of samples to introduce you to different Microsoft GDK API services.

## Support and licensing

Refer to the following files included with the package:

* [LICENSE](../license/LICENSE.html)
* [Third Party Notices](../license/Third%20Party%20Notices.html)
