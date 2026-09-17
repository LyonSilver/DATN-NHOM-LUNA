# Set up sandboxes

In the Microsoft Partner Center, you can [create a number of sandboxes](https://learn.microsoft.com/en-us/gaming/game-publishing/concepts/sandboxes?source=docs#create-and-manage-sandboxes) for development purposes. The sandboxes are uniquely identified by the [Sandbox Ids](https://learn.microsoft.com/en-gb/gaming/gdk/docs/services/fundamentals/sandboxes/live-setting-up-sandboxes#sandbox-ids) and are stored in the Microsoft Game Configuration file (MGC).

After you create the sandboxes in the Partner Center, you must configure them by linking the MGC file to the Microsoft Store. This ensures that the MGC file contains the corresponding Sandbox Ids for your project.

To set up the sandboxes, use the following steps:

1. Open the MicrosoftGame.Config Editor to retrieve the sandboxes created in the Microsoft Partner Center. You can open the Editor by double clicking the MGC asset. For more information, refer to [Edit the Microsoft Game Configuration file](edit-mgc-file.md).
1. In the Editor, select **Associate with Microsoft Store**.

The **Sandboxes** section on the MGC file gets populated with the Sandbox Ids, which you can view in the Inspector window. 

If other values within the MGC file appear correct but the **Sandboxes** list is either empty or incorrect, you can manually edit the list of sandbox Ids as a fallback approach.

> [!NOTE]
> If you intend to publish your game, it is highly recommended that you obtain the Sandbox Ids via the MicrosoftGame.Config Editor rather than manually adding them via the Inspector window. This is to ensure the values within your MGC file match those found in the Microsoft Partner Center.

## Additional resources

* [Setting up Sandboxes (Microsoft Game Development Kit Documentation)](https://learn.microsoft.com/en-gb/gaming/gdk/docs/services/fundamentals/sandboxes/live-setting-up-sandboxes)
* [Xbox services Sandboxes overview](https://learn.microsoft.com/en-gb/gaming/gdk/docs/services/fundamentals/sandboxes/live-setup-sandbox)
