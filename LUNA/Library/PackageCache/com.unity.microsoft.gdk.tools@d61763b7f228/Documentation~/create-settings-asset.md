# Create and activate a GDK Settings asset

The Microsoft GDK Tools package doesn't automatically create a GDK Settings asset. You must either create or import one. The GDK Settings asset must be activated and configured before you build your project. If the GDK Settings asset isn’t active, the GDK Tools package won’t be able to perform any additional build steps required.

## Create a GDK Settings asset

You can create a GDK Settings asset in two ways:

* In the main menu, go to **Assets** > **Create** > **GDK** > **GDK Settings**.
* In the main menu, go to **Edit** > **Project Settings** > **Microsoft GDK** and select **Create GDK Settings Asset**. This option is visible only if a GDK Settings asset doesn’t already exist in your project.

## Activate a GDK Settings asset

A GDK Settings asset will need to be created and activated in order for your GDK project to work as expected. Most projects will only ever need a single GDK Settings asset. However, you can have multiple GDK Settings assets in your project but only one can be active at build time.

You can activate the GDK Settings asset in either of the following ways:

* From the **Assets** folder in the **Project** window, double click the GDK Settings asset that you have created. A dialog prompt will appear to confirm activation.
* In the **Project** window, right click the GDK Settings asset to open a context menu. Select **GDK** > **Make Gdk Settings Active**.
* In the asset Inspector window, select **Activate GDK Settings**. This option is visible only if the selected asset isn't already active.
* In **Project Settings** > **Microsoft GDK**, set the required GDK Settings asset as active in the **Active GDK Settings**.

## Additional resources

* [GDK Settings reference](gdk-settings-reference.md)