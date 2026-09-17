# Issues and limitations

The following is a list of known issues and limitations with the Microsoft GDK Tools package.

## Issues

* `Create Microsoft Store Package` and `Sideloadable Package` are not supported when deploying via Xbox PC Toolbox in Unity 6000.5 or newer.
* When using GDK editions older than 250400 you must manually create or copy a valid `MicrosoftGame.config` file alongside the installed Unity.exe in order to use the GDK in `Play Mode`.
* The Unity Editor might require a restart to recognize changes to the `MicrosoftGame.config` file when used in combination with the built in editor player.
* When creating a package build, you must manually increment the version number in the **Identity** element of the `MicrosoftGame.config` file. Make sure the version number follows Microsoft's guidelines on [Package Version Numbering](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/package-version-numbering?pivots=store-installer-msix).
* `Build for GDK` automatically adds/removes the `MICROSOFT_GDK_SUPPORT` pre-processor to the list of player defines, however it can not account for the player defines being manually changed outside the scope of the tools package.
* The Microsoft GDK Tools package currently has no direct support for Unity `Build Profiles`