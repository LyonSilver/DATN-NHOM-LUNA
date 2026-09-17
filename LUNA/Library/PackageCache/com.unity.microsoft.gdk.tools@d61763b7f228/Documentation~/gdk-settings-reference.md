# GDK Settings reference

After you activate a GDK Settings asset, you can configure it in the Microsoft GDK section of the Project Settings window per your requirements. You can find all the created and imported GDK Settings assets in your project by typing `t:gdksettingsasset` in the [search bar](https://docs.unity3d.com/Manual/search-overview.html) of the Unity Project window.

Use either of the following to open the Project Settings window:

* In the Project window, right click the GDK Settings asset to open the context menu and select **GDK** > **Open Project Settings - Microsoft GDK**.
* In the asset Inspector window, select **Open Project Settings - Microsoft GDK**.

<a name="configuregdksettings"></a>

## Configure GDK Settings for target platforms

Unity supports many platforms which include desktop, web, and mobile platforms. However, not all platforms support GDK which means you must configure GDK Settings for each supported platform.

To configure GDK Settings for a target platform, select the required platform on the Microsoft GDK section of the Project Settings window. It might be necessary to select **Add Target GDK Settings** first.

The following settings are available for configuration:

### Settings for Windows

<table>
  <thead>
    <tr>
      <th><strong>Property</strong></th>
      <th><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><strong>Build for Microsoft GDK</strong></td>
      <td>Enable the additional build processing steps required for building a project using the Microsoft GDK. When enabled, the <code>MICROSOFT_GDK_SUPPORT</code> pre-processor will automatically be added to the list of Player defines and removed when disabled.<br /><br /> <strong>Note</strong>: This property will always be enabled for platforms that only support GDK.</td>
    </tr>
    <tr>
      <td><strong>Microsoft GDK Version</strong></td>
      <td>Select the Microsoft GDK edition to use in this Unity project. This setting is only available when used in combination with the Microsoft GDK API package. Applicable to Windows PC platform only.<br /><br />For more information, refer to the <a href="https://docs.unity3d.com/Packages/com.unity.microsoft.gdk@latest/index.html?subfolder=/manual/gdk-edition-selection.html">Microsoft GDK API package documentation</a>.<br /><br /> <strong>Note</strong>: The selected GDK edition is saved directly to the GDK edition tracking file used by the Microsoft GDK API package. The package uses this file when importing the Microsoft GDK DLLs. The GDK edition is not currently stored as part of the GDK Settings.</td>
    </tr>
    <tr>
      <td><strong>Microsoft Game Config</strong></td>
      <td>Specify the Microsoft Game Configuration (*.mgc) file required for the selected platform. A MicrosoftGame.config file will be generated from this asset and copied into your build output folder. <br /><br /> <strong>Note</strong>: This property is visible only when <strong>Build for Microsoft GDK</strong> is enabled.</td>
    </tr>
    <tr>
      <td><strong>Shell Visuals Mode</strong></td>
      <td>Specify how the Shell Visuals resources and resources.pri file will be managed during the build. <br /><br />
        <ul>
          <li><strong>Disabled</strong>: The build process will not attempt to build the <code>resources.pri</code> file and it will not copy any resources to the build output folder.</li>
          <li><strong>Run Make Pri and Copy</strong>: The build process will attempt to run the <code>MakePri.exe</code> command line tool to generate the <code>resources.pri</code> file. On completion, both the <code>resources.pri</code> file and image resources will be copied to the build output folder.</li>
          <li><strong>Copy Only</strong>: The build process won’t run the <code>MakePri.exe</code> command line tool and will instead copy a pre-built <code>resources.pri</code> file and additional resources.</li>
        </ul><strong>Note</strong>: This property is visible only when <strong>Build for Microsoft GDK</strong> is enabled.</td>
    </tr>
    <tr>
      <td><strong>Create Microsoft Store Package</strong></td>
      <td>Package the executable build of your project for submission.</td>
    </tr>
    <tr>
      <td><strong>Sideloadable Package</strong></td>
      <td>Generates a test signed package. The package will be automatically sideloaded into the local Microsoft Store app when using <strong>Build or Build and Run</strong>. Use the [GDK Sideloaded Packages](gdk-sideloaded-packages.md) window to interact with packages that have been sideloaded.<br /><br /> <strong>Note</strong>: This property is visible only when <strong>Create Microsoft Store Package</strong> is enabled.</td>
    </tr>
  </tbody>
</table>


### GDK Settings Script

<table>
  <thead>
    <tr>
      <th><strong>Property</strong></th>
      <th><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><strong>Regenerate On Build</strong></td>
      <td>At build time regenerate a GDK Settings script file making some GDK Settings available to the C# script. For more information, refer to the <a href="#generategdksettingsscript">Generate GDK Setting script file</a> section.</td>
    </tr>
    <tr>
      <td><strong>Use Namespace</strong></td>
      <td>Specify the namespace which will contain the generated <code>GdkPlatformSettings</code> class.</td>
    </tr>
  </tbody>
</table>

<a name="generategdksettingsscript"></a>

## Generate GDK Settings script file

You can generate a C# script to expose some GDK settings values, such as **SCID**, which are required when calling `XblInitialize()`.

The GDK Settings script file can be generated manually, by using the **Generate GDK Settings Script Now** button, or automatically at build time by selecting **Regenerate On Build**. The generated script file is saved in the same location as the currently active GDK Settings asset, and is named to reflect the asset name.

> [!NOTE]
> Using the Regenerate On Build setting will overwrite a pre-existing file.

## Additional resources

* [Create and activate a GDK Settings asset](create-settings-asset.md)