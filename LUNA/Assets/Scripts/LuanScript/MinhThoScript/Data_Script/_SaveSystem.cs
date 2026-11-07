using System.IO;
using UnityEngine;

public static class _SaveSystem
{
    const string FileName = "chaoscard_save.json";
    static _SaveData cache;

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static _SaveData Load()
    {
        if (cache != null) return cache;
        return ReloadFromDisk();
    }

    public static _SaveData ReloadFromDisk()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                cache = JsonUtility.FromJson<_SaveData>(json);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("SaveSystem: Không đọc được file save, tạo mới. " + e.Message);
        }

        if (cache == null) cache = new _SaveData();
        return cache;
    }

    public static void Save()
    {
        if (cache == null) cache = new _SaveData();
        try
        {
            string json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(FilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Lưu thất bại. " + e.Message);
        }
    }

    // Gọi khi người chơi kết thúc một trận thắng
    public static void ReportStageResult(string stageId, int stars, int score)
    {
        _SaveData data = Load();
        StageProgress p = data.GetOrCreate(stageId);

        stars = Mathf.Clamp(stars, 0, 3);
        bool firstClear = !p.cleared && stars >= 1;
        bool improved = stars > p.bestStars;

        if (stars >= 1) p.cleared = true;
        if (stars > p.bestStars) p.bestStars = stars;   // Không bao giờ giảm sao đã đạt
        if (score > p.highScore) p.highScore = score;

        // Đánh dấu để map chiếu hiệu ứng khi mở lại
        if (firstClear || improved) data.pendingReveal = stageId;

        Save();
    }

    public static void Reset()
    {
        cache = new _SaveData();
        Save();
    }
}