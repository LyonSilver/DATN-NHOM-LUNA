# Manage sideloaded GDK applications

The **GDK Sideloaded Packages** window provides a user interface within the Unity Editor to manage sideloaded Microsoft GDK applications on the local development machine. This avoids the need to use the `wdapp.exe` command-line tool for common tasks such as launching, terminating, and uninstalling test builds.

To open the **GDK Sideloaded Packages** window, navigate to the Unity Editor main menu and select **GDK** &gt; **PC** &gt; **Sideloaded Packages**.

The **GDK Sideloaded Packages** window provides the following capabilities:

* Lists all GDK applications currently installed on the PC.
* Displays key information for each application, including its logo, install path, Content ID, and Package Full Name.
* Provides buttons to launch, terminate, and uninstall applications directly from the Editor.
* Includes a refresh button to update the list with the latest information.

## Window Layout

Each installed application appears in its own section displaying the store logo and the display name of the application.

|**Field/Button**|**Description**|
|:---|:---|
|**Install Path**| The directory where the application is installed.|
|**Content ID** |The unique identifier for the application's content.|
|**Package Full Name**| The unique name for the installed package.|
|**AUMID**| The Application User Model ID used to launch the application. |
|**Refresh**| Updates the list of applications to reflect any changes to the environment.|
|**Show in Explorer**| Opens the directory where the application is installed. |
|**Launch**| Starts the application.|
|**Terminate**| Forcibly closes the application if it is running.|
|**Uninstall**| Removes the application from the PC. A confirmation dialog displays before you proceed.|

**Note:** You can select and copy the values of **Install Path**, **Content ID**, **Package Full Name**, and **AUMID** fields.

## Prerequisites

This tool relies on the `wdapp.exe` command-line utility, which is part of the Microsoft GDK. Ensure that the GDK is correctly installed on your development machine.

The window queries applications based on the currently active sandbox on the PC. If an application does not appear, ensure you have switched to the correct sandbox using the **GDK** &gt; **PC** &gt; **Switch Sandbox** window.