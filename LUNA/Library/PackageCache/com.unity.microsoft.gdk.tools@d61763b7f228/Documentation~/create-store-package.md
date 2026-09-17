# Package creation settings and encryption methods

The GDK Tools package in Unity supports the creation of Microsoft Store packages.

## GDK settings for package creation

> [!NOTE]
> The following settings for package creation are available only for the Windows platform.

To enable package creation:

1. Navigate to the **Microsoft GDK** section of the **Project Settings** window.
2. Select **Create Microsoft Store Package** for the active GDK settings.

Package creation occurs as a post-build step using the external tool [MakePkg.exe](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/packaging/deployment/makepkg). This tool executes with the required command line to generate a packaged build with encryption based on the chosen settings.

By default, the package is encrypted using a Random Key (`/l`). To use the Stable Key (`/lk`) encryption method, you must create and configure a Stable Key. For more information, refer to [Stable Key configuration](#stable-key-configuration).

> [!Note]
> The **Create Microsoft Store Package** option might not cover all scenarios. Depending on your app requirements, you might need to run this process outside of Unity. For more information, refer to the Microsoft documentation.

### Sideload Package setting

You can use the **Sideload Package** setting to test your package. This setting is used in combination with **Create Microsoft Store Package**. When you enable **Sideload Package**, the Test signing (`/lt`) encryption method will be forced.

> [!Important]
> The Test signing encryption method is not suitable for submission to the Microsoft Store.

### Automatic Submission Validator update

When the tracked GDK edition is **October 2025 (gdk-2510 / edition 251000)** or later, the `/updatesubval` flag is automatically passed to `makepkg pack`. Before creating the package, MakePkg contacts Microsoft's servers to check for a newer `SubmissionValidator.dll`. If a newer version is found, it is downloaded into the build output directory and used for that build's validation step. The GDK installation is not modified.

If the machine is offline, MakePkg falls back to the existing `SubmissionValidator.dll` that was copied from the GDK installation directory.

For more information, refer to:
* [Microsoft GDK - Submission Validator](https://learn.microsoft.com/en-us/gaming/gdk/docs/features/common/packaging/subval/submissionvalidator?view=gdk-2510)

## Stable Key configuration

Unlike the Test Signing (`/lt`) and Random Key (`/l`) encryption modes, the Stable Key (`/lk`) requires additional setup described as follows:

### Generate the Key file

This is a one-time process to create a **Local Escrowed Key Blob (LEKB)** which stores the stable content key used to encrypt the package. You can create this key in two ways:

1. Externally using the MakePkg tool
2. Within the Unity Editor from the **GDK: Package Stable Key Config** window (menu: **GDK** -> **PC** -> **Package Stable Key Config**)

> [!Warning]
> The contents of the LEKB file are only lightly obfuscated. It is your responsibility to ensure the security of your content packages by securing access to the LEKB file. Anyone possessing the LEKB file and the content package file output by MakePkg can decrypt its contents. Store it in a secure location with need-to-know access. Ideally, only official build machines/accounts should have access to it.

### Configure the Stable Key file path

After you create a Stable Key file, you must configure its path. There are two methods:

1. **Environment Variable**: Set the `UNITY_GDK_MAKEPKG_EKBFILE_PC` environment variable to the absolute file path of the LEKB key file.

2. **In-Editor Configuration**: Use the **GDK: Package Stable Key Config** window to select or clear the path of an LEKB file for the current Editor session.

When a Stable Key file path is configured and **Create Microsoft Store Package** is selected, the Stable Key (`/lk`) encryption method will be used automatically.

## GDK: Package Stable Key Config

In the **GDK: Package Stable Key Config** window, you can select a LEKB file path, clear the current file path and generate a new key, overwriting the content of the currently selected key file.

> [!Note]
> For security reasons, LEKB file path changes in this window last only for the current session of the Editor. If you require the file path to persist, configure the environment variable.

When you open this window for the first time in an Editor Session, the selected Stable Key file path will either be empty or display the value of the environment variable, if configured. Changes made to the path will persist until the Editor shuts down.

To generate a new key, set the LEKB file path and select **Generate Key**.

> [!Warning]
> This will prompt for confirmation and then overwrite any pre-existing key at the configured location.

To clear the key file path and revert to Random Key (`/l`), select **Clear Key**.

## Additional resources

* [Microsoft GDK - Title packaging, content updates, and streaming-installation testing](https://learn.microsoft.com/en-gb/gaming/gdk/_content/gc/packaging/title-packaging-streaming-install-testing)