
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // Thong tin tien trinh
    public string chapter = "Chapter I";
    public int playerLevel = 1;
    public int experience = 0;

    // Tai nguyen trong game
    public int gold = 0;

    // Card Collection: ID cac la bai so huu
    public List<string> ownedCardIds = new List<string>();

    // Deck: ID cac la bai trong bo bai
    public List<string> deckCardIds = new List<string>();

    // Thong tin luu game
    public float playTimeSeconds = 0f;
    public string lastSavedTime = "";

    // Danh dau loai slot
    public string slotType = "Manual";
}
