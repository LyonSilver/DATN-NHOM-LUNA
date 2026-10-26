using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance;

    public List<SkinData> allSkins;
    public PlayerSkinController playerSkin;

    private HashSet<string> unlockedSkins = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        LoadUnlocked();
    }

    public bool IsUnlocked(SkinData skin) => 
        unlockedSkins.Contains(skin.skinId) || skin.rarity == SkinRarity.Common;

    public void UnlockSkin(string skinId)
    {
        unlockedSkins.Add(skinId);
        SaveUnlocked();
    }

    public void EquipSkin(SkinData skin)
    {
        if (!IsUnlocked(skin)) return;
        playerSkin.ChangeSkin(skin, true);
        PlayerPrefs.SetString("EquippedSkin", skin.skinId);
    }

    public void OnSkinChanged(SkinData skin)
    {
        // Hook thêm: save, analytics, update UI...
    }

    void SaveUnlocked()
    {
        string data = string.Join(",", unlockedSkins);
        PlayerPrefs.SetString("UnlockedSkins", data);
    }

    void LoadUnlocked()
    {
        string data = PlayerPrefs.GetString("UnlockedSkins", "");
        if (!string.IsNullOrEmpty(data))
            foreach (var id in data.Split(','))
                unlockedSkins.Add(id);
    }
}