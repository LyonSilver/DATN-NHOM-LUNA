# Test your project in Play mode

The Unity Editor's Play mode allows you to test your project and get a realistic preview of how your application is likely to behave.

Both the Microsoft Gaming Runtime Services and Microsoft Xbox Services are available in Play mode. This allows you to experiment with features, such as login, achievements, and cloud save without leaving the Unity Editor.

However, Play mode support varies with the Microsoft GDK edition, Microsoft GDK API and Tools packages versions. It is always recommended to work with the latest releases where possible. 

## Testing in Play mode with GDK 250400 and later 

Starting with Microsoft GDK April 2025 (250400), you no longer need to manually copy your project's `MicrosoftGame.config` file to the same location as `Unity.exe`. If an active GDK Settings asset is correctly configured with a Microsoft Game Config asset, Unity will automatically locate and use the configuration file from its current location upon entering the Play mode.

### Requirements

- Microsoft GDK API and Tools Packages version 1.4.0 or later
- Microsoft GDK April 2025 or later
- Unity Editor version 2022.3 or later
- An active GDK Settings asset with a Microsoft Game Config asset configured for the Windows platform

### Limitations

Modifying the Microsoft Game Config file while the Editor is in Play mode can cause initialization failure with `E_GAMERUNTIME_OPTIONS_MISMATCH (0x89240109)` if an active Gaming Runtime Services component wasn’t properly closed in the previous iteration. If this error occurs, restart the Unity Editor to release the resources.

Always exit Play mode before modifying the Game Config file to prevent `E_GAMERUNTIME_OPTIONS_MISMATCH (0x89240109)` errors.

### Code guidelines

To reduce the chances of `E_GAMERUNTIME_OPTIONS_MISMATCH` errors, follow these recommended guidelines:

- Ensure all handles are closed before terminating the game or when exiting Play mode.
- Ensure queues are closed by force-terminating all pending callbacks, as demonstrated in the following example:

    ```C#
    bool canBreak = false;
    int hr = XTaskQueueTerminate(queue, false, IntPtr.Zero, (IntPtr context) =>
    {
        canBreak = true;
    }, out XTaskQueueTerminateCallbackHandle terminateHandle);

    while (!canBreak)
    {
        XTaskQueueDispatch(queue, XTaskQueuePort.Completion, 0);
        Thread.Yield();
    }

    terminateHandle.Dispose();
    queue.Close();
    ```

- Call `XblCleanup` before `CloseDefaultXStaskQueue` because it uses the default queue to close the Xbox Services library, as demonstrated in the following example:

    ```C#
    Debug.Log("Uninitializing Xbox Live API.");
    SDK.XBL.XblCleanup(null);

    SDK.CloseDefaultXTaskQueue();

    Debug.Log("Uninitializing XGame Runtime Library.");
    SDK.XGameRuntimeUninitialize();
    ```

## Testing in Play mode with GDK editions prior to 250400

For Microsoft GDK editions prior to 250400, you can still test your project using the Unity Editor's built-in Play mode. However, this process requires additional manual steps as follows:

1. [Create a valid MicrosoftGame.config](#valid-microsoft-game-config).
1. Manually copy the previously created config file to the same location as the executing process (**Unity.exe**).
1. [Correct initialization and uninitialization of the Microsoft GDK](#microsoft-gdk-initialization). Failure to initialize and uninitialize when entering and exiting Play mode results in locked or leaked resources.

<a name="valid-microsoft-game-config"></a>

### Create a valid MicrosoftGame.config

Before calling `SDK.XGameRuntimeInitialize()`, a valid **MicrosoftGame.config** file must exist alongside the executing process. In the case of the Unity Editor and its Play mode, this means the same location as `Unity.exe`.

To achieve this, use the following steps:

1. Locate the MGC Asset you want to use.
1. Go to **Assets** > **Show In Explorer**.
1. Copy and paste the `.mgc` file to the location containing `Unity.exe`.
1. Rename the file to `MicrosoftGame.config`.

> [!NOTE]
> Any changes made to the `MicrosoftGame.config` file after calling `SDK.XGameRuntimeInitialize()` won't take effect until you restart the Editor. This is a known issue.

<a name="microsoft-gdk-initialization"></a>

### Correct initialization and uninitialization of the Microsoft GDK

The Microsoft GDK Tools package doesn't automatically initialize or uninitialize the API. Ensure your code handles this by calling corresponding initialization and uninitialization methods when entering and exiting the Play mode. Failure to do so might lead to unexpected results or errors whilst testing your project.

For example, each call to `SDK.XGameRuntimeInitialize()` must match with a subsequent call to `SDK.XGameRuntimeUninitialize()`.

Neither the Unity Editor nor the Microsoft GDK Tools package can automatically clean up unmatched API calls.