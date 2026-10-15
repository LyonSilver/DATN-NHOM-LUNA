using UnityEngine;

public class AutoSaveManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 phút lưu 1 lần

    [Header("References")]
    [SerializeField] private SaveLoadManager saveLoadManager;

    private float timer;

    private void Start()
    {
        if (saveLoadManager == null)
        {
            saveLoadManager = FindFirstObjectByType<SaveLoadManager>();
        }
    }

    private void Update()
    {
        if (!enableAutoSave || saveLoadManager == null) return;

        timer += Time.deltaTime;
        if (timer >= autoSaveInterval)
        {
            TriggerAutoSave();
            timer = 0f;
        }
    }

    /// <summary>
    /// Gọi hàm này khi cần Auto Save ngay lập tức
    /// </summary>
    public void TriggerAutoSave()
    {
        if (saveLoadManager != null)
        {
            // Tạo đối tượng SaveData để truyền vào hàm SaveGame (truyền Slot 0 cho AutoSave)
            SaveData dataToSave = new SaveData(); 
            saveLoadManager.SaveGame(dataToSave, 0);

            Debug.Log("<color=cyan>[AutoSaveManager]</color> Đã tự động lưu game vào Slot 0!");
        }
        else
        {
            Debug.LogWarning("[AutoSaveManager] Chưa tìm thấy SaveLoadManager trong Scene!");
        }
    }
}