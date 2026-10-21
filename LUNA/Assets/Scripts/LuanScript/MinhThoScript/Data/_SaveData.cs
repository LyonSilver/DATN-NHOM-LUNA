using System;
using System.Collections.Generic;

[Serializable]
public class StageProgress
{
    public string stageId;
    public bool cleared;
    public int bestStars;   // 0 đến 3
    public int highScore;
}

[Serializable]
public class _SaveData
{
    public int playerLevel = 1;
    public int exp = 0;
    public int coin = 0;
    public string pendingReveal = "";
    public List<StageProgress> stages = new List<StageProgress>();

    // Đọc tiến trình của một Stage. Chưa có thì trả về bản trống (Chưa lưu vào danh sách).
    public StageProgress Get(string stageId)
    {
        foreach (StageProgress p in stages)
            if (p.stageId == stageId) return p;
        return new StageProgress { stageId = stageId };
    }

    // Lấy tiến trình, chưa có thì tạo mới và thêm vào danh sách.
    public StageProgress GetOrCreate(string stageId)
    {
        foreach (StageProgress p in stages)
            if (p.stageId == stageId) return p;
        StageProgress created = new StageProgress { stageId = stageId };
        stages.Add(created);
        return created;
    }
}