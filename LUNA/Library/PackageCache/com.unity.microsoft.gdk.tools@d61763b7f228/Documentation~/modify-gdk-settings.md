---
uid: modify-gdk-settings
---

# Modify GDK settings with C# script

You can use a C# script to modify the GDK settings, which is useful when automating your build process. The following code example demonstrates how to change the state of the **Build for Microsoft GDK** option for the selected build target on the currently active GDK settings.

```CSharp
    // Get the platform specific settings associated with the active build target
    var platformGdkSettings = GdkSettings.GetActivePlatformSettings(EditorUserBuildSettings.activeBuildTarget);
    platformGdkSettings.targetMicrosoftGdk = !platformGdkSettings.targetMicrosoftGdk;
```