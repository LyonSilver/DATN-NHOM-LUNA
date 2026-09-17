# Create and publish an app

Create an app and use the Microsoft GDK Tools workflow to publish it in the Microsoft Store.

## Create an app on Partner Center
To publish your app on Microsoft Store, you must perform the following steps. For details on each step, refer to the Microsoft documentation.
1. [Reserve you app’s name](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/reserve-your-apps-name?pivots=store-installer-msix)
2. [Create an app submission](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/create-app-submission?pivots=store-installer-msix)


## Publish your app packages
Understand how to use the GDK Tools package to create a packaged Windows app that is suitable for submission to Microsoft Partner Center.

**Note:** Make sure you have installed the latest versions of Microsoft GDK API package and Microsoft GDK Tools package from the Package Manager.

1. Create your app within Unity. For more information, refer to the [GDK project workflow](https://docs.unity3d.com/Packages/com.unity.microsoft.gdk.tools@1.0/manual/gdk-project-workflow.html).
2. Build **Shell Visual resources** that are required to submit your app. For more information, refer to [localize your application](https://docs.unity3d.com/Packages/com.unity.microsoft.gdk.tools@1.0/manual/ShellVisuals.html).
3. Configure the following settings in **Microsoft GDK** section for Windows platform (menu: **Edit > Project Settings > Microsoft GDK**). For more information on these settings, refer to the [GDK settings reference](https://docs.unity3d.com/Packages/com.unity.microsoft.gdk.tools@1.0/manual/gdk-settings-reference.html).
    * Enable **Build for Microsoft GDK**.
    * Set the **Shell Visuals Mode** to either **Run Make Pri and Copy** or **Copy Only**.
    * Enable **Create Microsoft Store Package**.
4. In Unity’s main menu, go to **File > Build Settings** and select the **Windows** platform.
5. Select **Build** to create your application build in the target folder. <br>**Note**: For Unity Editor version 6000.0, **Build Settings** are included within the [Build Profiles](https://docs.unity3d.com/6000.0/Documentation/Manual/BuildSettings.html) window.
6. Upload your app packages. Refer to Microsoft documentation on [Upload app packages](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/upload-app-packages?pivots=store-installer-msix). The following files in the build folder are required during the upload process. <br/>**Note**: When submitting your Windows app to the Microsoft Store, you can use `MakeAppx.exe` tool from the Windows SDK to create `.msix` or `.appx` files. For more information, refer to Microsoft documentation on [Create an MSIX package or bundle with MakeAppx.exe](https://learn.microsoft.com/en-us/windows/msix/package/create-app-package-with-makeappx-tool).

    |**Filename**|**Description**|
    |---|---|
    |**EKB**|Select the latest EKB from the **MSIXVC** folder.|
    |**Subval log**|Select the `.xml` file that starts with `Validator_`.|
    |**Symbol file**|Select the provided `.zip` in the folder.|
    |**Disc layout**|Select the `.xml` file that starts with `layout_`.|

## Additional resources
* [Publish your app in the Microsoft Store](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/overview?pivots=store-installer-msix)
* [Build Settings](https://docs.unity3d.com/2022.3/Documentation/Manual/BuildSettings.html)



