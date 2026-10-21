using UnityEngine;

/// <summary>
/// Gắn lên nút hoặc GameObject bất kỳ.
/// Gọi ResetAll() hoặc ResetMoney() từ OnClick để test.
/// </summary>
public class SaveReset : MonoBehaviour
{
    [Header("Keys cần xóa (để trống = xóa hết)")]
    [SerializeField] private string[] keysToDelete = { "PlayerMoney" };

    [ContextMenu("Reset All")]
    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[SaveReset] Đã xóa toàn bộ PlayerPrefs");
    }

    [ContextMenu("Reset Money")]
    public void ResetMoney()
    {
        PlayerPrefs.DeleteKey("PlayerMoney");
        PlayerPrefs.Save();
        Debug.Log("[SaveReset] Đã xóa PlayerMoney");
    }

    [ContextMenu("Reset Selected Keys")]
    public void ResetSelected()
    {
        if (keysToDelete == null || keysToDelete.Length == 0)
        {
            ResetAll();
            return;
        }

        foreach (var key in keysToDelete)
        {
            if (!string.IsNullOrEmpty(key))
                PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
        Debug.Log($"[SaveReset] Đã xóa {keysToDelete.Length} key");
    }
}