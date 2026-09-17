# Build an application with the Microsoft GDK API package

Understand how to use the GDK API package to build a project for the Windows platform.

> [!NOTE]
> The GDK API package does not create or manage the [MicrosoftGame.config](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/system/overviews/microsoft-game-config/microsoftgameconfig-overview) file. Consider using the [Microsoft GDK Tools package (com.unity.microsoft.gdk.tools)](https://docs.unity3d.com/Packages/com.unity.microsoft.gdk.tools@latest) to make best use of the GDK API package for an improved workflow.


## Set up and build a project using the GDK API package

To set up and build a project using the GDK API package, complete the following tasks:

* [Import a sample](#import-sample)
* [Include the sample scene in the build](#include-sample)
* [Add the Microsoft Game Configuration file](#add-game-config)
* [Run the project](#run-project)

<a name="import-sample"></a>

### Import a sample

The GDK API package provides some samples for you to explore the Microsoft Xbox services. To run these samples, you need the [Input System package](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest). Use the following steps to import one of the samples and install the Input System package:

1. Create a new Unity Project.
1. In the main menu, go to **Window** > **Package Manager**.
1. In the packages list view, select the **Input System** package.
1. In the Package [details view](https://docs.unity3d.com/Manual/upm-ui-details.html), select **Install**.
1. In the packages list view, select the **Microsoft GDK API** package.
1. In the Package [details view](https://docs.unity3d.com/Manual/upm-ui-details.html), select **Samples**.
1. Locate the Sign-in sample and select **Import**. Unity imports the selected package sample into `Assets/Samples/Microsoft GDK API/<package version>/Sign-in`.
1. Double click the **SignIn** scene asset to load the sample scene.

<a name="include-sample"></a>

### Include the sample scene in the build

1. In the main menu, go to **File** >  **Build Settings** and select the **Windows** platform.
    * If you're using Unity Editor version 6000.0, Build settings are included within the [Build Profiles](https://docs.unity3d.com/2023.3/Documentation/Manual/BuildSettings.html) window:
        1. In the main menu, go to **File** >  **Build Profiles**.
        1. Select **Add Build Profile** to open the Platform Browser window and select the **Windows** platform. This creates a Windows build profile.
        1. Select **Switch Profile** to make the Windows build profile active.
1. Add the sample scene by selecting **Add Open Scenes**.
1. Select **Build** to create a player in your target folder. Don’t use **Build & Run** as the `XGameRuntime` will fail to initialize due to a missing `MicrosoftGame.config`.


<a name="add-game-config"></a>

### Add the Microsoft Game Configuration file

Your project must contain a valid Microsoft game configuration file for your game to run. The GDK API package itself doesn’t provide you tools to create or maintain this file.

1. In the Project window, navigate to `Assets/Samples/Microsoft GDK API/<package version>/Sign-in/MicrosoftGameConfig`.
1. Copy the `GdkSignInSample.mgc` to the resulting build folder where the built player is located. You can locate the Microsoft Game Config file using the right-click menu option, **Show in Explorer**.
1. Rename the file copied in the earlier step to `MicrosoftGame.config`.

For more information about the MicrosoftGame.cofig file, refer to [MicrosoftGame.config](microsoft-game-dot-config).

> [!NOTE]
> For the best experience it’s recommended to use the [Microsoft GDK Tools](https://docs.unity3d.com/Packages/com.unity.microsoft.gdk.tools@latest) package. This package makes working with Microsoft Game Configuration files easier, as you don’t need to maintain this file manually during the build process.

<a name="run-project"></a>

### Run the project

At this point you can either run the built application executable directly from the output folder or within the Unity Editor using the Build And Run option with the same output location. Don’t clean the build folder on build, or the MicrosoftGame.config file will be removed.

## Build a sample using the Microsoft GDK Tools package

Using both the Microsoft GDK API and Microsoft GDK Tools packages together is the recommended approach when developing for GDK. The GDK Tools package provides support that you don't get with the standalone GDK API package.

To build and run a sample using the Microsoft GDK Tools package, use the following steps:
1. Follow the previous steps to:
    * [Import a sample with the Microsoft GDK API package](#import-sample)
    * [Include the sample scene in the build](#include-sample)
1. Install the [Microsoft GDK Tools package](https://docs.unity3d.com/Packages/com.unity.microsoft.gdk.tools@latest).
1. In the Project window, navigate to `Assets/Samples/Microsoft GDK API/<package version>/Sign-in/`.
1. Double click on the `GDKSignInSample.gdksettings.asset` to activate it.
1. In the main menu, go to **File** > **Build Settings** and select the **Windows** platform.
1. Ensure the sample scene is still selected in the **Scenes In Build** section.
1. Select **Build And Run**. The GDK Tools package will automatically create the MicrosoftGame.config based on the GDK Settings provided by `GDKSignInSample.gdksettings.asset`.

<a name="microsoft-game-dot-config"></a>

## MicrosoftGame.config

Consider the following important points relating to the `MicrosoftGame.Config` file:
* Your project must include a valid `MicrosoftGame.Config` file for the correct initialization of the GDK API at runtime.
* The `MicrosoftGame.Config` file must be included in the same folder as the executable that initializes the GDK API. The executable can be a built game executable or Unity.exe if you’re using the built-in Player of Unity Editor. In case of built game executable, make sure to save the `MicrosoftGame.Config` file in the same folder as your project build.
* The `MicrosoftGame.Config` file must be correctly configured, with valid settings for elements such as **TitleID** and **MSAAppId**. For more information, refer to [MicrosoftGame.config overview](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/system/overviews/microsoft-game-config/microsoftgameconfig-overview).
* Pass the correct **SCID** in the call to `SDK.XBL.XblInitialize()`.
* If your project doesn't include a `MicrosoftGame.Config file`, you will get the error `0x87E5001F (E_GAME_MISSING_GAME_CONFIG)`.

## Additional resources

* [MicrosoftGame.config Overview](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/system/overviews/microsoft-game-config/microsoftgameconfig-overview)
* [Samples overview](Samples.md)
