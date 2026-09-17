# GDK edition selection

The Microsoft GDK API package typically supports the latest version of the `Microsoft GDK` SDK available at the time of package release. However, there are scenarios where you might need to use a different version, such as in the following cases:

- Working with older projects that require specific Microsoft GDK version
- Working on multiple projects at different stages of their release cycle
- Compatibility requirements with other middleware or tools
- Transitioning gradually to a newer Microsoft GDK version, for example, to test compatibility before fully integrating it into your project

To accommodate these needs, the Microsoft GDK API package aims to provide flexibility and allows a Unity project to use a specified version of the `Microsoft GDK` SDK based on edition number.

> [!IMPORTANT]
> Use `Microsoft GDK` SDK versions newer than the package release for evaluation and testing only. These are not recommended for product release. For more information, refer to [Limitations](#limitations).

> [!NOTE]
> Microsoft GDK edition numbers are 6 digits long in the **YYMMUU** format where: `YY - Year`, `MM - Month`, `UU - Update`. For example: `241001` indicates `October 2024 Update 1`.

## GDK development on Windows standalone platform

When using the GDK for the Windows standalone platform, the Unity project requires specific Dynamic Link Libraries (DLLs) from the `Microsoft GDK` SDK. Mananging these dependencies manually can be complex because:

- Required DLLs vary between different `Microsoft GDK` SDK versions
- Library locations and filenames might change between versions
- Libraries might be added or removed in different `Microsoft GDK` SDK releases

The Microsoft GDK API package for Unity automatically handles these dependencies, eliminating the need for manual library management.

The package automatically imports the required DLLs into your project's `Assets/Plugins/GDK` folder. These DLLs are available for use in Editor Play mode and are included in the Windows standalone builds.

## Default behavior

The default behavior occurs when:

- The `Assets/Plugins/GDK` folder does not exist
- The `GdkEdition.json` tracking file is missing

In these cases, the Microsoft GDK API package automatically imports DLLs from the latest available `Microsoft GDK` SDK edition.

> [!NOTE]
> When upgrading from an earlier version of the Microsoft GDK API package, the package re-imports the `Microsoft GDK` DLLs and creates a tracking file automatically. If you want to use an alternative edition of the `Microsoft GDK`, update the edition number in the tracking file as required.

## GDK edition tracking

The Microsoft GDK API package uses a simple JSON format file `GdkEdition.json` to keep track of the GDK edition used by your project. This tracking file is located in the `Assets/Plugins/GDK` folder of the project. The file is created automatically when the `Microsoft GDK` SDK DLLs are imported and contains the edition number from which the DLLs originated. 

The Microsoft GDK API package monitors this tracking file for changes.

- If the tracking file is **missing**, the package triggers an import from the **latest** available `Microsoft GDK` edition. 
- If the tracking file is **changed**, the package triggers an import from the **specified** `Microsoft GDK` edition.

> [!NOTE]
> The Microsoft GDK API package always verifies the expected DLLs when the Editor first loads the project and on a domain reload.

## Disable GDK edition tracking

You can disable the automatic import behavior of the Microsoft GDK API package if required. To do this, set the edition number to `-1` in the `GdkEdition.json` tracking file.

## DLL handling

The Microsoft GDK API package provides useful information and error messages in the Unity Editor's **Console** window.

After entering Editor Play mode, the DLLs are loaded and locked by the Editor. This means that the Microsoft GDK API can not automatically re-import the DLLs if needed. In this situation, you need to restart the Editor to release the lock. 

The Microsoft GDK API only manages the DLLs that are part of the `Microsoft GDK` SDK. It does not touch any additional files which might exist in the `Assets/Plugins/GDK` folder.

## Limitations

The Microsoft GDK API package is primarily tested against the latest supported `Microsoft GDK` SDK version at the time of package release. This version is considered the recommended SDK version for use with the package.

The package attempts to maintain backward compatibility with older `Microsoft GDK` SDK versions where possible. However, full functionality cannot be guaranteed for all previous versions.

While it is possible to configure the package to use newer `Microsoft GDK` SDK versions than those available at package release, this is not recommended as it might lead to:

- Unexpected behavior
- Compatibility issues
- Runtime errors
- Missing or changed functionality