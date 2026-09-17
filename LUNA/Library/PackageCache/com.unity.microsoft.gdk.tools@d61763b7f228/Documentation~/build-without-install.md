# Using the Microsoft Game Development Kit without installation

In general it is recommended to install the Microsoft Game Development Kit (GDK) locally using the approptiate installer for the edition that you require. However, it is possible to work with an extracted version of the Microsoft Game Development Kit (GDK). This approach is referred to a Build Without Install (BWOI) and is intended for advanced users.

You will need to create an extracted version of the Microsoft Game Development Kit (GDK) and customize the environment of the machine being used so that it can reference the extracted Microsoft Game Development Kit (GDK).

For the most up-to-date details on how to set-up Build Without Install (BWOI), see the Microsoft documentation.

## High level steps

At a high level in order to set-up Build Without Install (BWOI) you will need to carry out the following steps:

1. Get an extracted version of the Micrsoft Game Develoment Kit (GDK). You may need to manually produce this following the Microsoft Guidance.
2. Configure your machines environment to reference the extracted Microsoft Game Development Kit (GDK). This involves setting some environment variables and paths, for example:

Environment variables required for GDK editions older than October 2025:
```Shell
SET GRDKEDITION=241000
SET "GameDK=C:\ExtractedGdk\Microsoft GDK\"
SET "GameDKLatest=%GameDK%%GRDKEDITION%\"
SET "GamingGRDKBuild=%GameDKLatest%GRDK\"
```

Environment variables required for GDK editions equal to and newer than October 2025:
```Shell
SET GRDKEDITION=251000
SET "GameDK=C:\ExtractedGdk\Microsoft GDK\"
SET "GameDKLatest=%GameDK%%GXDKEDITION%\"
SET "GameDKCoreBuild=%GameDKLatest%\"
SET "GameDKXboxBuild=%GameDKLatest%\"
```

3. Start the Unity editor in the correct environment ensuring it has access to the environment variables and paths configured above.

## Additional resources

* [Using the Microsoft Game Development Kit without installation](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/tools-console/usingwithoutinstall/gc-usingwithoutinstall-toc)