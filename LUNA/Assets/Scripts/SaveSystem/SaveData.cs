using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // ===== Meta =====
    public int saveVersion = 1;
    public string playerId = "";
    public string slotType = "Auto";               // Auto / Manual
    public string lastSavedTime = "";
    public string checksum = "";

    // ===== Player Progress =====
    public int playerLevel = 1;
    public int playerExp = 0;
    public int gold = 0;
    public int gems = 0;
    public int chapter = 1;
    public string currentStageId = "";
    public string lastMatchResult = "";

    // ===== Match Stats =====
    public int matchesPlayed = 0;
    public int matchesWon = 0;
    public int matchesLost = 0;

    // ===== Unlock System =====
    public List<string> unlockedCards = new List<string>();
    public List<string> unlockedStages = new List<string>();
    public List<string> unlockedBosses = new List<string>();
    public List<string> unlockedRewards = new List<string>();

    // ===== Card Collection =====
    public List<string> ownedCardIds = new List<string>();
    public List<string> deckCardIds = new List<string>();
    public List<CardInstanceData> cardInstances = new List<CardInstanceData>();

    // ===== Slot Unlock =====
    public bool isSlot2Unlocked = false;       // mặc định = đang khóa
    public int slot2UnlockCost = 500;          // giá mở khóa

    // ===== Settings =====
    public bool autoSaveEnabled = true;
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    // ===== Helpers =====
    public void EnsureLists()
    {
        unlockedCards ??= new List<string>();
        unlockedStages ??= new List<string>();
        unlockedBosses ??= new List<string>();
        unlockedRewards ??= new List<string>();
        ownedCardIds ??= new List<string>();
        deckCardIds ??= new List<string>();
        cardInstances ??= new List<CardInstanceData>();
    }

    public SaveData DeepCopy()
    {
        string json = JsonUtility.ToJson(this);
        return JsonUtility.FromJson<SaveData>(json);
    }
}

[Serializable]
public class CardInstanceData
{
    public string cardId;
    public int level = 1;
    public int exp = 0;
    public int amount = 1;
}