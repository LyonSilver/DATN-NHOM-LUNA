# Localize your application

The Microsoft Game Configuration file allows you to define the provided localization support of your title

You can use the **Shell Visuals** section of the Microsoft Game Config asset Inspector window to:

* configure supported languages
* generate placeholder resources
* generate the localization configuration files
* build a `Resource.Pri` file using the `MakePri.exe` command line tool

Shell Visuals resources consist of language locale pairs and a selection of .png images, such as a title splash screen or logo. Each title must include Shell Visuals resources and a `Resources.Pri` file for the Microsoft Game Store localization to work.

## Build Shell Visuals resources

To build the Shell Visuals resources, use the following steps:

1. Create a folder to store your Microsoft Game Config and Shell Visual resources.
1. Create a new Microsoft Game Config file in this folder. Right click in the folder and select **Create** > **GDK** > **Microsoft Game Config**.
    * Although not mandatory, it’s recommended that you create your Microsoft Game Config asset inside the same folder that contains the Shell Visuals resources. This is the same default layout as used by the **MicrosoftGame.Config Editor** and reduces the chances of errors in that tool or with `MakePri.exe`.
1. Select the Microsoft Game Config asset. In the **Inspector** window, expand the **Shell Visuals** section.
1. Select the language pairs you want to support.
    * You can add more language pairs later, but you must [update the resources](#update-resources). Removing languages doesn’t automatically delete resources. It must be done manually to avoid the accidental deletion of files.
1. Verify that the **Shell Visuals Resource Path** and **Built Resources Pri Path** are correct. The default path is the folder in which the MGC asset is created. These paths must be set before you can build the Shell Visuals `Resources.pri` file.
1. Apply any changes made.
1. Select **Generate Shell Visuals Resources**.
<br/><strong>Note</strong>: The button is labeled **Generate Shell Visuals Resources** when you create resources for the first time and as **Update Shell Visuals Resources** when the resources already exist. You can select this button only after you apply all changes.
1. For each missing shell resource, a dialog displays asking for permission to generate new placeholder image. Select **yes** to continue. The Microsoft Game Configuration file will be updated accordingly.
    * If you have selected language pairs, an additional dialog box appears, offering you to import the default images for all newly selected languages. Select **yes** to continue.
1. Placeholder resources are generated in the folder specified by **Shell Visuals Resource Path**.
1. Select **Build Shell Visuals Resources (Resources.Pri)**. A `Resources.Pri` file is created in the folder specified by **Built Resources Pri Path**.
1. Update the placeholder images and resources files per your requirements. Don’t change the file names unless you intend to maintain the `MakePri.exe` configuration files yourself.

When you update Shell Visuals Resources, existing image files aren't affected or deleted. This is intended to avoid accidental loss of existing image resources as a result of changing the Language Pairs. However, the MakePri configuration files are updated to reflect the current configuration.

Any changes to **Language Pair Resources** are reflected in the `MicrosoftGame.Config` only when selecting **Apply**. However, changes aren't automatically applied to associated Shell Visual resources in the Assets folder. You must manually select **Generate Shell Visuals Resources** or **Update Shell Visuals Resources** to apply these changes.

### Update resources

Resources are files that are generated when the `Generate Shell Visuals Resources` button is used. They may also be files which you have manually added. Shell visuals resource files support the localization of your application. These files typically include the following types: 

* `.resw` files: These are XML-based Windows resource files similar to a [`.resx`](https://learn.microsoft.com/en-us/dotnet/core/extensions/create-resource-files#resources-in-resx-files) file. Multiple `.resw` files are added to support different languages and regions. Each `.resw` file stores text strings as key-value pairs for a specific language. Each key represents a resource name and the value is the localized string for a specific language. 
*  Image files in `.png` format

When you add new language locale pairs after building the Shell Visual resources, you must update the corresponding resource file to match the locale. You can update the `.resw` files in Visual Studio. To do that, follow these steps:

1. Open your Unity project in Visual Studio.
1. In the **Solution Explorer** window, navigate to the language-specific folder, such as `en-US`, `fr-FR`.
1. Locate and double-click the `.resw` file you want to modify.
1. Make the changes and save the file.

Alternatively, you can edit the `.resw` files using a text editor that supports XML formatting.

Updated `.resw` files are compiled into `Resources.pri` file during the build process for runtime access.

## Additional resources

* [MicrosoftGame.config Shell Visuals Element](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/reference/system/microsoftgameconfig/elements/microsoftgameconfig-element-shellvisuals)
* [MicrosoftGame.config localization](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/system/overviews/microsoft-game-config/microsoftgameconfig-localization)
* [MakePri.exe command-line options](https://learn.microsoft.com/en-us/windows/uwp/app-resources/makepri-exe-command-options)
