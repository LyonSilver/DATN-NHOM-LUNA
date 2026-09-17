# Access the available GDK versions

The GDK discovery package provides a sample to get the GDK setup details.

To import the sample, use the following steps:

1. Create a new Unity Project.
1. In the main menu, go to **Window** > **Package Manager**.
1. In the packages list view, select the **Microsoft GDK Discovery** package.
1. In the [Package details](https://docs.unity3d.com/Manual/upm-ui-details.html) view, select **Samples**.
1. Select **Import**.

After you import the sample, a new menu is added to the Unity Editor main menu under **GDK** > **Discovery Samples** with the following sub-menus.

| **Menu** | **Description** |
| :--- | :--- |
| **Find Specific Edition** | Checks for a specific GDK version on your system and displays its details if available.|
| **Query Installed GDKs** | Displays a list of all the GDK versions that are currently installed on your system. |

Each menu demonstrates an aspect of the API and corresponds to a `.cs` file in the sample. The output of each menu is displayed in the Unity Editor Console window.