using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string SavePath = Application.persistentDataPath + "/savegame.dat";
    private static readonly string EncryptionKey = "GameSecretKey";

    // 1. Lưu file + Mã hóa
    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        byte[] encryptedData = EncryptDecrypt(json);
        File.WriteAllBytes(SavePath, encryptedData);
    }

    // 2. Đọc file + Giải mã
    public static SaveData Load()
    {
        if (!File.Exists(SavePath)) return new SaveData();

        try
        {
            byte[] encryptedData = File.ReadAllBytes(SavePath);
            string json = DecryptToString(encryptedData);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception)
        {
            return new SaveData(); // Trả về data mới nếu file bị lỗi/can thiệp
        }
    }

    // 3. Tự động lưu ở các mốc sự kiện
    public static void AutoSave(SaveData currentData, string triggerReason)
    {
        Save(currentData);
        Debug.Log($"[AutoSave] Đã lưu tiến trình khi: {triggerReason}");
    }

    private static byte[] EncryptDecrypt(string text)
    {
        byte[] result = Encoding.UTF8.GetBytes(text);
        for (int i = 0; i < result.Length; i++)
            result[i] = (byte)(result[i] ^ EncryptionKey[i % EncryptionKey.Length]);
        return result;
    }

    private static string DecryptToString(byte[] data)
    {
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
            result[i] = (byte)(data[i] ^ EncryptionKey[i % EncryptionKey.Length]);
        return Encoding.UTF8.GetString(result);
    }
}