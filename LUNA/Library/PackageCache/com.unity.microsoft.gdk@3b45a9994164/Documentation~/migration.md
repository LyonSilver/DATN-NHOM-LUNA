# Upgrade guide

Use this guide to upgrade from the older Unity GDK package (platform specific) to the new Microsoft GDK API package. This guide covers typical migration steps along with some that may be specific to your project. When upgrading from Microsoft's legacy GDK Unity Package,  refer to: [**Migrating from the Microsoft 'GDK Unity Package'**](#legacy-gdk-unity-package)

> [!NOTE]
> It’s recommended to backup your project before attempting a migration.

<a name="migration-mechanisms"></a>

## Provided migration mechanisms

To ease the transition from legacy packages to the new Microsoft GDK API package, Unity and Microsoft provide the following migration mechanisms. These mechanisms are designed to mitigate common migration issues and help speed up the migration process:

* **MovedFromAttributes for Public API**: Any matching public API has been migrated using MovedFromAttributes. This feature handles all namespace changes.

* **API Redirections**: Where possible, API redirections have been implemented to assist in your project’s migration. The redirections automatically update your project to use the new APIs in the Microsoft GDK API package. Additional comments have been added to further guide you towards the new API.

* **Detailed Migration Guide**: This documentation serves as a comprehensive guide covering all migration details. Following this guide should lead to a successful migration in the majority of scenarios.

> [!NOTE]
> Extensive testing is recommended throughout the migration process.

<a name="legacy-gdk-unity-package"></a>

### Migrating from Microsoft's legacy GDK Unity Package

When migrating from the GDK Unity Package (PC Only) provided by Microsoft, you must change the migration path. To do this, use the following steps:
1. In the Unity main menu, go to **Edit** > **Project Settings** > **Player** and select your platform.
2. Expand **Other Settings**.
3. In the Script Compilation section, add a new Scripting Define Symbol of `LEGACY_GDK_UPGRADE_PATH`.

<a name="breaking-changes"></a>

## Breaking changes

The following section details breaking changes to be aware of when migrating to the new Microsoft GDK API package.

* [`XRegistrationToken`](#xregistrationtoken): Transitioned to a specific type for each use, now employing Dispose semantics.
* `XAppCaptureOpenScreenshotStream`: `XAppScreenshotLocalId` has been phased out. Use a proper string with this parameter.
* `XGameSaveCloseUpdateHandle`: Renamed to `XGameSaveCloseUpdate` for consistency with the API.
* `XPackageMount`: This previously deprecated method has been removed.
- The following methods parameter types have been adjusted to align with and match the API:
    * `XStoreReportConsumableFulfillmentCompleted`
    * `XStoreShowRateAndReviewUICompleted`
    * `XUserFindControllerForUserWithUiResult`
    * `XAppCaptureReadScreenshotStream`: The parameter type has been changed from `int32` to `uint32`.
    * `XGameSaveFilesGetRemainingQuota`: The output parameter type has been changed from `long` to `ulong`.
* Several delegates have had their signatures changed. For more information, refer to [Delegate signature changes](#delegate-changes).
* `HttpSecurityProtocolFlags`: Enum has been replaced by the use of `Uint32`, for example `UInt32 enabledHttpSecurityProtocolFlags;`.
* `XPackageUpdateInstallationMonitor`: The return type is changed from `bool` to `void`.
* `XUserLocalId`: No longer provides a constructor that accepts a `UInt64` type.

<a name="xregistrationtoken"></a>

### XRegistrationToken deprecation
The new Microsoft GDK API package, `XRegistrationToken` has been replaced by specific types unique to each use. Consult the following methods listed for reference:

```C#
public static Int32 XAppBroadcastRegisterIsAppBroadcastingChanged(
    XAppBroadcastMonitorCallback callback,
    out XRegistrationToken token) ->
public static Int32 XAppBroadcastRegisterIsAppBroadcastingChanged(
    XAppBroadcastMonitorCallback callback,
    out XIsAppBroadcastingChangedRegistrationToken token)

public static Int32 XAppCaptureRegisterMetadataPurged(
    XAppCaptureMetadataPurgedCallback callback,
    out XRegistrationToken token) ->
public static Int32 XAppCaptureRegisterMetadataPurged(
    XAppCaptureMetadataPurgedCallback callback,
    out XMetadataPurgedToken token)

public static bool XAppCaptureUnRegisterMetadataPurged(
    XRegistrationToken token) ->
public static bool XAppCaptureUnRegisterMetadataPurged(
    XMetadataPurgedToken token)

public static Int32 XGameInviteRegisterForEvent(
    XGameInviteEventCallback callback,
    out XRegistrationToken token) ->
public static Int32 XGameInviteRegisterForEvent(
    XGameInviteEventCallback callback,
    out XGameInviteRegistrationToken token)

public static void XGameInviteUnregisterForEvent(
    XRegistrationToken token) ->
public static bool XGameInviteUnregisterForEvent(
    XGameInviteRegistrationToken token)


public static Int32  XNetworkingRegisterPreferredLocalUdpMultiplayerPortChanged(
    XNetworkingPreferredLocalUdpMultiplayerPortChangedCallback callback,
    out XRegistrationToken token) ->
public static Int32 XNetworkingRegisterPreferredLocalUdpMultiplayerPortChanged(
    XNetworkingPreferredLocalUdpMultiplayerPortChangedCallback callback,
    out XNetworkingPreferredLocalUdpMultiplayerPortChangedCallbackToken token)

public static bool XNetworkingUnregisterPreferredLocalUdpMultiplayerPortChanged(
    XRegistrationToken token) ->
public static bool XNetworkingUnregisterPreferredLocalUdpMultiplayerPortChanged(
    XNetworkingPreferredLocalUdpMultiplayerPortChangedCallbackToken token)


public static Int32 XNetworkingRegisterConnectivityHintChanged(
    XNetworkingConnectivityHintChangedCallback callback,
    out XRegistrationToken token) ->
public static Int32 XNetworkingRegisterConnectivityHintChanged(
    XNetworkingConnectivityHintChangedCallback callback,
    out XNetworkingRegisterConnectivityHintChangedCallbackToken token)


public static bool XNetworkingUnregisterConnectivityHintChanged(
    XRegistrationToken token,
    bool wait) ->
public static bool XNetworkingUnregisterConnectivityHintChanged(
    XNetworkingRegisterConnectivityHintChangedCallbackToken token,
    bool wait)

public static Int32 XPackageRegisterPackageInstalled(
    XPackageInstalledCallback callback,
    out XRegistrationToken token) ->
public static Int32 XPackageRegisterPackageInstalled(
    XPackageInstalledCallback callback,
    out XPackageRegisterPackageInstalledToken token)


public static void XPackageUnregisterPackageInstalled(
    XRegistrationToken token) ->
public static bool XPackageUnregisterPackageInstalled(
    XPackageRegisterPackageInstalledToken token)


public static Int32 XPackageRegisterInstallationProgressChanged(
    XPackageInstallationMonitorHandle installationMonitor,
    XPackageInstallationProgressCallback callback,
    out XRegistrationToken token) ->
public static Int32 XPackageRegisterInstallationProgressChanged(
    XPackageInstallationMonitorHandle installationMonitor,
    XPackageInstallationProgressCallback callback,
    out XPackageRegisterInstallationProgressChangedToken token)

public static void XPackageUnregisterInstallationProgressChanged(
    XPackageInstallationMonitorHandle installationMonitor,
    XRegistrationToken token) ->
public static bool XPackageUnregisterInstallationProgressChanged(
    XPackageInstallationMonitorHandle installationMonitor,
    XPackageRegisterInstallationProgressChangedToken token,
    bool wait)

public static Int32 XStoreRegisterGameLicenseChanged(
    XStoreContext context,
    XStoreGameLicenseChangedCallback callback,
    out XRegistrationToken token) ->
public static Int32 XStoreRegisterGameLicenseChanged(
    XStoreContext context,
    XStoreGameLicenseChangedCallback callback,
    out GameLicenseChangedCallbackToken token)

public static void XStoreUnregisterGameLicenseChanged(
    XStoreContext context,
    XRegistrationToken token) ->
public static void XStoreUnregisterGameLicenseChanged(
    XStoreContext context,
    GameLicenseChangedCallbackToken token)

public static Int32 XStoreRegisterPackageLicenseLost(
    XStoreLicense license,
    XStorePackageLicenseLostCallback callback,
    out XRegistrationToken token) ->
public static Int32 XStoreRegisterPackageLicenseLost(
    XStoreLicense license,
    XStorePackageLicenseLostCallback callback,
    out PackageLicenseLostCallbackToken token)

public static void XStoreUnregisterPackageLicenseLost(
    XStoreLicense license,
    XRegistrationToken token) ->
public static void XStoreUnregisterPackageLicenseLost(
    XStoreLicense license,
    PackageLicenseLostCallbackToken token)


public static Int32 XUserRegisterForChangeEvent(
    XUserChangeEventCallback callback,
    out XRegistrationToken registrationToken) ->
public static int XUserRegisterForChangeEvent(
    XUserChangeEventCallback callback,
    out XUserChangeRegistrationToken token)

public static void XUserUnregisterForChangeEvent(
    XRegistrationToken registrationToken) ->
public static bool XUserUnregisterForChangeEven(
    XUserChangeRegistrationToken token,
    bool wait)
```

<a name="delegate-changes"></a>

### Delegate signature change

The following is a non-exhaustive list of delegates with signatures changes. The majority of which have `IntPtr context` added, which must be manually updated:

- `XAppBroadcastMonitorCallback`
- `XGameInviteEventCallback`
- `XGameProtocolActivationCallback`
- `XNetworkingConnectivityHintChangedCallback`
- `XPackageEnumerationCallback`
- `XStorePackageLicenseLostCallback`
- `XUserChangeEventCallback`
- `XTaskQueueCallback`

<a name="migration-example"></a>

## Upgrade workflow

To upgrade from the legacy Unity GDK package samples, complete the following tasks:

1. [Initial setup](#initial-setup)
2. [Resolve first compilation errors and fixes](#resolve-first-compliation)
3. [Resolve second compilation errors and fixes](#resolve-second-compliation)
4. [Resolve non-explicit errors](#resolve-non-explicit)

<a name="initial-setup"></a>

### Initial setup

Complete the initial project setup using the following steps:

1. Install the Unity legacy GDK package.
2. Import all the provided samples.
3. Install the new `com.unity.microsoft.gdk` package.
4. Uninstall the Unity legacy GDK package.
5. A **Script Updating Consent** prompt will appear. It’s recommended to select  **Yes, for these and other files that might be found later** to automatically update the API references in your project.

<a name="resolve-first-compliation"></a>

###  Resolve first compilation errors and fixes

**Leaderboards and User samples**: Errors and resolutions for the `UserManager.cs` file.

<table>
<thead>
  <tr>
    <th><strong>Error message</strong></th>
    <th><strong>Resolution</strong></th>
  </tr>
</thead>
<tbody>
  <tr>
    <td><code>error CS0619: 'XRegistrationToken' is obsolete: 'XRegistrationToken has been removed. If you are upgrading from a legacy package, please refer to the migration guide.</code></td>
    <td>Replace XRegistrationToken with the specific use case type - <code>XUserChangeRegistrationToken</code> in this case.</td>
  </tr>
  <tr>
    <td><code>`error CS1503: Argument 1: cannot convert from 'method group' to 'XUserChangeEventCallback'</code></td>
    <td>Delegate signature changed - you can mitigate the error by adding <code>IntPtr _</code> as the first parameter of the function.</td>
  </tr>
</tbody>
</table>

<a name="resolve-second-compliation"></a>

###  Resolve second compilation errors and fixes

**Game Save sample**: Error and resolution for the `UserManager.cs` file.

|**Error message**|**Resolution**|
|---|---|---|
|`error CS1503: Argument 1: cannot convert from 'method group' to 'XUserChangeEventCallback'`| Delegate signature changed - you can mitigate the error by adding `IntPtr _` as the first parameter of the function.|

<a name="resolve-non-explicit"></a>

### Resolve non-explicit errors

At this point, all errors should be fixed. However, hidden issues tied to behavior changes introduced in the new GDK API might still persist. Use the following to address any remaining errors:
1. [**Fix Life Cycle Management**](#fix-lifecycle)
2. [**Implement DefaultTaskQueue**](#implement-default-task)
3. [**Resolve NullReferenceException errors**](#resolve-null-reference)
4. [**Retrieve MicrosoftGameConfig Values**](#retrieve-values)

<a name="fix-lifecycle"></a>

**Fix Life Cycle Management**

The new Microsoft GDK API package follows a stricter lifecycle management. Ensure to call `SDK.XGameRuntimeUninitialize()` and `SDK.XBL.XblCleanup(null)` when terminating the session. Failure to do so might result in the following error in subsequent runs: `A call is being made after Xbox Live APIs have been initialized. (0x89235207).`

<a name="implement-default-task"></a>

**Implement DefaultTaskQueue**

The Microsoft GDK API now requires users to create and manage their task queue. To transition to the new API, you can use `SDK.CreateDefaultTaskQueue()`, `SDK.XTaskQueueDispatch(uint)`, and `SDK.CloseDefaultXTaskQueue()`.

<a name="resolve-null-reference"></a>

**Resolve NullReferenceException errors**

After completing the previous steps, attempting to launch or deploy the User scene will throw a `NullReferenceException` error in `UserSceneManager.Start()`. Use the following steps to rewrite the `TaskQueue` logic using `SDK.CreateDefaultTaskQueue()` to resolve this issue:

1. Above the creation of `DispatchGXDKTaskQueue`, add `SDK.CreateDefaultTaskQueue()` (along with error handling).
2. Call `SDK.XTaskQueueDispatch(0)` in a looping thread to pull the GDK events asynchronously. You can refer to GDKGameRuntime.cs in one of the package samples for more details.
3. Ensure that you call `SDK.CloseDefaultXTaskQueue()` on object termination.

```C#
void Awake/RuntimeInitialization()
{
    Int32 hr = Unity.XGamingRuntime.SDK.XGameRuntimeInitialize();
    if (hr == 0)
    {
        int hResult = Unity.XGamingRuntime.SDK.CreateDefaultTaskQueue();
        if (HR.FAILED(hResult))
        {
            Debug.Log($"FAILED: XTaskQueueCreate, HResult: 0x{hResult:X}");
            return;
        }

        // Dispatch a thread to pull GDK events...
    }
}
```
<a name="retrieve-values"></a>

**Retrieve MicrosoftGameConfig Values**

As the Microsoft GDK now supports the Windows platform, it’s important to have a reliable way of retrieving different Title IDs per platform.

The following are common errors you might get when trying to use Xbox Live API:
* `This app is missing a required MicrosoftGame.Config file. (0x87E5001F)`.
* `MissingMethodException:  assembly:<unknown assembly> type:<unknown type> member:(null)`

To resolve, implement a custom struct or class holding your Title IDs that you will pass to the Xbox Live API. Use the following solutions to implement this:

* Use a preprocessor to enable the ID based on the platform

  ```C#
  public static class GdkPlatformSettings
  {
  #if UNITY_GAMECORE_XBOXSERIES || UNITY_GAMECORE_XBOXONE
      public static string gameConfigTitleId = "00000000";
      public static string gameConfigScid = "00000000-0000-0000-0000-000000000000";
      public static string gameConfigSandbox = "";
  #else
      public static string gameConfigTitleId = "11111111";
      public static string gameConfigScid = "00000000-0000-0000-0000-000011111111";
      public static string gameConfigSandbox = "";
  #endif
  }
  ```

* Using the `com.unity.microsoft.gdk.tool` Configure the required Microsoft GDK assets using the following steps:

  1. Create a new `MicrosoftGameConfig` and `MicrosoftGdkSettings` from **Create** > **GDK**.
  2. Configure the `MicrosoftGameConfig.asset`.
  3. Select the `MicrosoftGdkSettings.asset` and click on **Generate GdkPlatformSettings.cs Now**.
  4. Ensure it's configured with the correct SCID for the target platform.
  5. Modify `GamingRuntimeManager.cs` to use the correct SCID: `XBL.XblInitialize(GdkPlatformSettings.gameConfigScid)`.

