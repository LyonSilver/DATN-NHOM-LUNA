# Samples overview

The Microsoft GDK API package contains a set of samples to help you explore the Xbox Services such as Achievements, Cloud Saves and Leaderboards. A sample contains C# scripts, scenes, a Microsoft Game Configuration file, and a GDK Settings file.

The samples can be found using the following steps:

1. Create a new Unity Project.
1. In the main menu, go to **Window** > **Package Manager**.
1. In the packages list view, select the **Microsoft GDK API** package.
1. In the [Package details](https://docs.unity3d.com/Manual/upm-ui-details.html) view, select **Samples**.
1. Import the sample you want to explore.

## Provided samples

|**Sample**|**Description**|
|---|---|
|[**Achievements**](Samples/Achievements.md)|Demonstrates basic interaction with [Title-Managed Achievements](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/live/features/player-data/achievements/title-managed/live-achievements-tm-overview). <br /><br />Use it to: <ul><li>Query a single achievement to unlock it.</li><li>Update the status of the achievement.</li></ul>|
|[**Cloud saves**](Samples/CloudSaves.md)|Demonstrates the use of `XGameSaveAPIs` to implement cloud saves. Use it to save, upload, and retrieve cloud save data .
|[**Game Save**](Samples/GameSave.md)|Demonstrates the use of `XGameSaveAPIs` and full-sync or sync-on-demand mode. <br /><br />Use it to: <ul><li>Load, save, and delete game save data.</li><li>Display the state of all the containers and blobs used in a game in the debug output area.</li></ul>|
|[**In Game Store**](Samples/InGameStore.md)|Demonstrates an in-game store implementation to present specific items for the users to purchase using the XStore API.|
|[**Leaderboards**](Samples/Leaderboards.md)|Demonstrates the use of Leaderboards in a title using Player Stats and Player Stat Rules. Use it to rank game participants based on their position in the game in relation to their peers.|
|[**Sign-in**](Samples/Sign-in.md)|Demonstrates a scenario where a user signs-in when the game scene starts.|
|[**Social Manager**](Samples/SocialManager.md)|Demonstrates the use of Social Manager to view information on friends in several states.|
|[**Speech Synthesizer**](Samples/SpeechSynthesizer.md)|Demonstrates the use of the Speech Synthesizer feature. Use it to set up a speech synthesize engine to convert text messages into audio streams.|
|[**Users**](Samples/Users.md)|Demonstrates user identity using the XUser Game Core API. Use it to query the gamertag and check the status of the user sign-in.|

## Additional resources

* [Build an application with the Microsoft GDK API package](build-with-gdk-api.md)
