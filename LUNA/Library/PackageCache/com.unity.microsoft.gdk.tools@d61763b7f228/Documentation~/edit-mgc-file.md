---
uid: edit-mgc-file
---

# Edit the Microsoft Game configuration file

You can edit your Microsoft Game Configuration file in the following ways:

* Use the asset Inspector window to quickly access and update the most common elements of the MGC. Editing using the Inspector window is the recommended method.
* Use the official MicrosoftGame.Config Editor installed with the GDK. When you double click a MGC, it automatically opens in the MicrosoftGame.Config Editor.
* Edit with a text editor. You can locate the MGC file using the **Show in Explorer** context menu.
* Use C# scripts to modify the Microsoft Game Configuration XML.

> [!IMPORTANT]
> Caution is advised when manually editing the MGC file as this can result in the creation of invalid XML.

## Use the MicrosoftGame.Config Inspector

When you select a Microsoft Game Configuration asset, the Inspector window displays the most commonly used elements, providing access to related functionality, the error log. and XML preview.

<table>
  <thead>
    <tr>
      <th><strong>Property</strong></th>
      <th><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><strong>Open</strong></td>
      <td>Open the Microsoft Game Configuration file in the <a href="#usemgceditor">MicrosoftGame.Config Editor</a>.</td>
    </tr>
    <tr>
      <td><strong>Title Id</strong></td>
      <td>Specify a unique identifier for your title to Xbox live services and is generated from the Partner Center.</td>
    </tr>
    <tr>
      <td><strong>SCID</strong></td>
      <td>Specify the service configuration ID for your title to be used in Save Game Storage. For more information, refer to <a href="https://developer.microsoft.com/en-us/games/xbox/docs/gdk/game-saves">Game saves</a>.</td>
    </tr>
    <tr>
      <td><strong>MSA App Id</strong></td>
      <td>Specify a unique identifier for your title generated from the Partner Center. It's a 16 character string of integers.</td>
    </tr>
    <tr>
      <td><strong>Name</strong></td>
      <td>Specify the name of your title generated from the Partner Center. This property accepts ASCII values that are 3 to 50 characters long.</td>
    </tr>
    <tr>
      <td><strong>Publisher</strong></td>
      <td>Specify the name of your title's publisher generated from the Partner Center. This property accepts string values that conform to the LDAPv3 Distinguished Names section in the <a href=https://www.ietf.org/rfc/rfc2253.txt>RFC 2253</a> standard.</td>
    </tr>
    <tr>
      <td><strong>Version</strong></td>
      <td>Specify the version of your title. Before submitting a new package, you must manually increment the version, otherwise the submission will fail. Additional packages for subsequent console generations must have package major versions higher than the existing package versions. Version numbers must follow Microsoft's guidelines on <a href=https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/package-version-numbering?pivots=store-installer-msix> Package version numbering</a>.</td>
    </tr>
    <tr>
      <td><strong>Store Id</strong></td>
      <td>Specify a unique identifier for the store which contains 12 characters long alphanumeric values.</td>
    </tr>
    <tr>
      <td><strong>Shell Visuals Mode</strong></td>
      <td>Expand this section to display localization settings for shell visuals.<br /><br />
      <ul>
        <li><strong>Language Pair Resources</strong>: Select the supported languages to configure for your title.</li>
        <li><strong>Shell Visuals Resource path</strong>: Specify the path to the Assets folder that contains Shell Visuals resources required for localization and when creating the <code>Resources.pri</code> file. By default, this is the path to the folder in which the MGC asset exists.</li>
        <li><strong>Built Resources Pri path</strong>: Specifies path to the Assets folder that stores the built <code>Resources.pri</code> file after Build Shell Visual Resources successfully executes <code>Makepri.exe</code>. By default this is the path to the folder in which the MGC asset exists.</li>
        <li><strong>Generate Shell Visuals Resources</strong>: Generates Shell Visual Resources to reflect the selected language locale pairs. For more information, refer to <a href="ShellVisuals.html">Localize your application</a>. Once resources are created, this option will then display as <strong>Update Shell Visuals Resources</strong>. <br /><strong>Note</strong>: This option is available only when all changes have been applied.</li>
        <li><strong>Build Shell Visuals Resources (Resource.Pri)</strong>: Runs the MakePri.exe command line tool and attempts to build the <code>Resource.Pri file</code>. For more information, refer to <a href="ShellVisuals.html#build-shell-visuals-resources">Build Shell Visuals resources</a>. <br /><strong>Note</strong>: This option is available only when all changes have been applied.</li>
      </ul>
    </td>
    </tr>
  <tr>
	  <td><strong>Sandboxes</strong></td>
	  <td>Specifies a list of Sandbox Id entries used for development purposes. Obtained by associating the Microsoft Game Configuration file with the Microsoft Store in the MicrosoftGame.Config Editor. For more information, refer to <a href="mgc-sandboxes.md">Set up sandboxes</a>.</td>
	</tr>
    <tr>
      <td><strong>Content Id Override</strong></td>
      <td>Specifies an override for the ContentID of your title. This field is auto populated with a default value of SHA-256 hash generated using the PackageFamilyName of your title.</td>
    </tr>
    <tr>
      <td><strong>Ekbid Override</strong></td>
      <td>Specify an override for the EKBID of your title. Once entered, select <strong>Apply</strong> to save the override.</td>
    </tr>
    <tr>
      <td><strong>Log</strong></td>
      <td>Displays errors, warnings and information relating to recent asset changes.</td>
    </tr>
    <tr>
      <td><strong>Preview</strong></td>
      <td>Displays the MGC file in XML format for preview.</td>
    </tr>
  </tbody>
</table>

<a name="usemgceditor"></a>

## Use the MicrosoftGame.Config Editor

You can open a Microsoft Game Config asset in the official MicrosoftGame.Config Editor using the following methods:
* Double click on the MGC asset.
* Select the asset in the Project window. Right click the asset to open the context menu and select **GDK** > **Open with MicrosoftGame.config Editor**.
* Select the asset from the Inspector window using the **Open** button.
* From **Project Settings** >  **Microsoft GDK**, use the **Open .MGC with Game Config Editor** button. A configured GDK Settings file and a Microsoft Game Config asset must be present for this option to work.

> [!NOTE]
> The MicrosoftGame.config Editor expects a specific layout when using **Shell Visuals**. It's recommended to use the same folder that contains the Microsoft Game Configuration file.

<a name="usecsharpscript"></a>

## Use C# script

You can use a C# script to directly modify the XML of the MGC, which is useful when automating your build process. The following code example demonstrates how to change the Shell Visuals settings.

```CSharp
    // Get the Game Config associated with the active build target
    // that is configured in the currently active GDK Settings asset.
    var gameConfigAsset = GdkSettings.GetActiveGameConfig(EditorUserBuildSettings.activeBuildTarget);

    // Using the asset path, read in as XML using the helper method
    var path = AssetDatabase.GetAssetPath(gameConfigAsset);
    var xml = GameConfigAsset.LoadAsXmlGameConfig(path);

    // Make changes ...
    xml.ShellVisuals.DefaultDisplayName   = defaultDisplayName;
    xml.ShellVisuals.PublisherDisplayName = PublisherDisplayName;
    xml.ShellVisuals.Description          = Description;
    xml.ShellVisuals.StoreLogo            = StoreLogo;
    xml.ShellVisuals.Square44x44Logo      = Square44x44Logo;
    xml.ShellVisuals.Square150x150Logo    = Square150x150Logo;

    // Write back out, using a helper method, overwriting the original
    GameConfigAsset.SaveXmlGameConfig(path, xml);

    // A refresh is required as the asset has changed on disc
    AssetDatabase.Refresh();
```

## Additional resources
* [Using the MicrosoftGame.config Editor](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/system/overviews/microsoft-game-config/microsoftgameconfig-editor)
