using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int playerLevel = 1;
    public int currentEXP = 0;
    public int coin = 0;
    public List<int> unlockedStageIDs = new List<int>();
    public List<string> unlockedCardIDs = new List<string>();
    public List<string> deckList = new List<string>();
    public int highScore = 0;
}