using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string playerId = "";
    public string slotType = "";
    public string lastSavedTime = "";

    public string chapter = "";
    public int playerLevel = 1;
    public int gold = 0;

    public List<string> ownedCardIds = new List<string>();
    public List<string> deckCardIds = new List<string>();

    public string currentStageId = "";
    public string lastMatchResult = "";
    public int matchesPlayed = 0;
    public int matchesWon = 0;
    public int matchesLost = 0;
}