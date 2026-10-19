
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // Player
    public string playerId = "";
    public string chapter = "Chapter I";
    public int playerLevel = 1;
    public int experience = 0;

    // Resources
    public int gold = 0;

    // Cards
    public List<string> ownedCardIds = new List<string>();
    public List<string> deckCardIds = new List<string>();

    // Progress
    public string currentStageId = "";
    public string lastMatchResult = "";
    public int matchesPlayed = 0;
    public int matchesWon = 0;
    public int matchesLost = 0;

    // Save information
    public float playTimeSeconds = 0f;
    public string lastSavedTime = "";
    public string slotType = "Manual";
}
