using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }

    public static event Action<UnlockType, string> OnItemUnlocked;

    private HashSet<string> unlockedKeySet = new HashSet<string>();
    private string saveFilePath;
    private readonly byte[] encryptionKey = Encoding.UTF8.GetBytes("UnlockSystemKey!");
    private readonly byte[] encryptionIV = Encoding.UTF8.GetBytes("UnlockSystemInit");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "unlocks.dat");
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string FormatKey(UnlockType type, string id)
    {
        if (string.IsNullOrEmpty(id)) return string.Empty;
        return type.ToString() + "_" + id.Trim();
    }

    public bool IsUnlocked(UnlockType type, string id)
    {
        string key = FormatKey(type, id);
        if (string.IsNullOrEmpty(key)) return false;
        return unlockedKeySet.Contains(key);
    }

    public bool Unlock(UnlockType type, string id)
    {
        string key = FormatKey(type, id);
        if (string.IsNullOrEmpty(key)) return false;
        if (unlockedKeySet.Contains(key)) return false;

        unlockedKeySet.Add(key);
        SaveData();
        OnItemUnlocked?.Invoke(type, id);
        return true;
    }

    public bool CheckAndUnlockByLevel(string id, int currentLevel, int requiredLevel)
    {
        if (currentLevel >= requiredLevel)
        {
            return Unlock(UnlockType.LevelRequirement, id);
        }
        return false;
    }

    public void ResetAllData()
    {
        unlockedKeySet.Clear();
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
        SaveData();
    }

    private void SaveData()
    {
        try
        {
            UnlockData data = new UnlockData();
            data.unlockedKeys.AddRange(unlockedKeySet);

            string json = JsonUtility.ToJson(data);
            byte[] encryptedBytes = EncryptString(json);

            string tempPath = saveFilePath + ".tmp";
            File.WriteAllBytes(tempPath, encryptedBytes);

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }
            File.Move(tempPath, saveFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Save Error: " + ex.Message);
        }
    }

    private void LoadData()
    {
        unlockedKeySet.Clear();
        if (!File.Exists(saveFilePath)) return;

        try
        {
            byte[] encryptedBytes = File.ReadAllBytes(saveFilePath);
            string json = DecryptBytes(encryptedBytes);
            UnlockData data = JsonUtility.FromJson<UnlockData>(json);

            if (data != null && data.unlockedKeys != null)
            {
                foreach (string key in data.unlockedKeys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        unlockedKeySet.Add(key);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Corrupted save data, clearing: " + ex.Message);
            ResetAllData();
        }
    }

    private byte[] EncryptString(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = encryptionKey;
            aes.IV = encryptionIV;
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return ms.ToArray();
                }
            }
        }
    }

    private string DecryptBytes(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = encryptionKey;
            aes.IV = encryptionIV;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}