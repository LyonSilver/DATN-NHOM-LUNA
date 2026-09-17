# GDK project workflow

Use the following workflow to learn how to use the GDK Tools package to build a project for the Windows platform.

> [!NOTE]
> When targeting Standalone Windows, only 64-bit Windows is supported.

To set up and build a project using the GDK Tools package, complete the following tasks:
* [Create a GDK Settings asset](#gdksettingsasset)
* [Create a Microsoft game configuration file](#createmgc)
* [Configure the GDK Settings](#configuregdk)
* [Build your project](#buildproject)

<a name="gdksettingsasset"></a>

## Create a GDK Settings asset

Your project must include an activated GDK Settings asset. Use the following steps to create and activate the GDK Settings asset:

1. Create a new Unity project.
1. In the main menu, go to **Assets** > **Create** > **GDK** > **GDK Settings**. This creates a `GDKSettings.asset` file in the root of your **Assets** folder. You can rename this file if needed.
1. In the Project window, locate your newly created `GDKSettings.asset` file and double-click to activate it.

<a name="createmgc"></a>

## Create a Microsoft game configuration file

To create a Microsoft game configuration file, use the following steps:

1. In the main menu, go to **Assets** > **Create** > **GDK** > **Microsoft Game Config**.
    * This creates a basic `MicrosoftGameConfig.mgc` file in the root of your **Assets** folder.
1. Configure this asset to meet your specific requirements before your GDK project can be fully functional.

> [!NOTE]
> If you are using the [GDK API package](https://docs.unity3d.com/Packages/com.unity.microsoft.gdk@latest), you can import a .mgc asset from the samples provided within the package.

<a name="configuregdk"></a>

## Configure the GDK Settings

Configure the GDK Settings for the target platform, Windows, using the following steps:

1. In the main menu, go to **Edit** > **Project Settings** > **Microsoft GDK**.
1. Select **Add Target GDK Settings**.
1. Enable **Build for Microsoft GDK**.
1. Set **Microsoft Game Config** to the Microsoft Game configuration asset created earlier.

<a name="buildproject"></a>

## Build your project

When you build your project, the Microsoft GDK Tools package runs the additional steps required during the build process. This includes: 
* Processing shell visuals resources required to localize your game splash screen, logo, and the text relevant to the Microsoft Store.
* Managing the Microsoft Game Configuration file and packaging steps that you would otherwise need to handle manually.

For more information on configuring localization, refer to [Localize your application](ShellVisuals.md). Make sure you configure the [Microsoft Game Configuration file](mgc-file.md) correctly per your requirements before the build process. Note that the `gameConfigSandbox` field in the `GdkPlatformSettings` class is now obsolete and should be removed from your configuration.

## Additional resources
* [GDK Settings](MsGdkSettings.md)
* [Microsoft game configuration file](mgc-file.md)
* [Configure GDK Settings for target platforms](gdk-settings-reference.md#configuregdksettings)
* [Test your project in Play mode](editor-play-mode.md)