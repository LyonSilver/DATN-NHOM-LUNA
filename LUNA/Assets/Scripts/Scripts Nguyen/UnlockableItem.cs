using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockableItemComponent : MonoBehaviour
{
    [Header("Item Configuration")]
    public UnlockType unlockType;
    public string itemId;

    [Header("Optional Direct Rarity Text Reference")]
    public Text explicitRarityText;         // Kéo Text hiển thị Rare/Epic/Legendary vào đây (nếu có)
    public TMP_Text explicitTMPRarityText;  // Kéo TMP Text độ hiếm vào đây (nếu có)

    [Header("UI Visual Elements")]
    public GameObject lockedVisual;
    public GameObject unlockedVisual;
    public Button actionButton;

    private void OnEnable()
    {
        UnlockManager.OnItemUnlocked += HandleItemUnlocked;
        RefreshUI();
    }

    private void OnDisable()
    {
        UnlockManager.OnItemUnlocked -= HandleItemUnlocked;
    }

    // Kiểm tra chính xác thẻ này có phải là Legendary hay không
    private bool IsLegendaryCard()
    {
        // 1. Kiểm tra chính xác theo itemId
        if (!string.IsNullOrEmpty(itemId))
        {
            string lowerId = itemId.Trim().ToLower();
            if (lowerId.Contains("chaos") || lowerId.Contains("legendary"))
            {
                return true;
            }
        }

        // 2. Kiểm tra nếu có kéo-thả trực tiếp Text độ hiếm
        if (explicitRarityText != null && CheckStringIsLegendary(explicitRarityText.text))
        {
            return true;
        }

        if (explicitTMPRarityText != null && CheckStringIsLegendary(explicitTMPRarityText.text))
        {
            return true;
        }

        // 3. Kiểm tra Tên của GameObject thẻ bài này
        if (CheckStringIsLegendary(gameObject.name))
        {
            return true;
        }

        return false;
    }

    private bool CheckStringIsLegendary(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        string t = text.Trim().ToLower();
        return t.Contains("legendary") || t.Contains("huyền thoại") || t.Contains("huyen thoai") || t.Contains("chaos draw");
    }

    public void RefreshUI()
    {
        bool isLegendary = IsLegendaryCard();
        bool isUnlocked = false;

        if (UnlockManager.Instance != null && !string.IsNullOrEmpty(itemId))
        {
            isUnlocked = UnlockManager.Instance.IsUnlocked(unlockType, itemId);
        }

        // Cập nhật Visual nếu có kéo thả
        if (lockedVisual != null) lockedVisual.SetActive(!isUnlocked || isLegendary);
        if (unlockedVisual != null) unlockedVisual.SetActive(isUnlocked && !isLegendary);

        // Tự tìm nút Buy nếu chưa kéo thả
        if (actionButton == null)
        {
            actionButton = GetComponentInChildren<Button>();
        }

        if (actionButton != null)
        {
            if (isLegendary)
            {
                // KHÓA DUY NHẤT THẺ LEGENDARY
                actionButton.interactable = false;
            }
            else
            {
                // CÁC THẺ THƯỜNG KHÔNG BỊ KHÓA, BẤM BUY BÌNH THƯỜNG
                actionButton.interactable = true;
            }
        }
    }

    private void HandleItemUnlocked(UnlockType type, string id)
    {
        if (type == unlockType && id == itemId)
        {
            RefreshUI();
        }
    }
}