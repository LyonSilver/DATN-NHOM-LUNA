# Microsoft Game Configuration file

The Microsoft Game Configuration (MGC) file is a manifest file that must be provided with any game distributed through the Microsoft Store. It stores game-specific configuration in XML format and must be named `MicrosoftGame.config` at runtime. This file is used for the following purposes:

* Packaging of your game for submission and distribution through Microsoft Store
* Registering game specific information during the game development phase before the project build is packaged for distribution.

The Microsoft GDK Tools package treats Microsoft Game Configuration(MGC) files as assets for easier management when they have the .mgc file extension.

|**Topic**|**Description**|
|---|---|
|[**Create or import MGC assets**](create-mgc-asset.md)|Create or import a Microsoft Game Configuration file (MGC) asset.|
|[**Edit your Microsoft Game Configuration**](edit-mgc-file.md)|Edit your Microsoft Game Configuration as per your specific requirement.|
|[**Localize your application**](ShellVisuals.md)|Use the Shell Visuals element of the MGC file to configure the representation of your product in the Microsoft Store. |
|[**Set up sandboxes**](mgc-sandboxes.md)|Set up the sandboxes required for development by associating the Microsoft Game Configuration file (MGC) with the Microsoft Store. |

## Additional resources

* [Test your project in Play mode](editor-play-mode.md)
* [MicrosoftGame.config overview](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/system/overviews/microsoft-game-config/microsoftgameconfig-overview)
