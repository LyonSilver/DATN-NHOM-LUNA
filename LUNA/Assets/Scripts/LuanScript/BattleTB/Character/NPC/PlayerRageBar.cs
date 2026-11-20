
using UnityEngine;
using UnityEngine.UI;

public class PlayerRageBar : MonoBehaviour
{
    [Header("Rage UI")]
    [SerializeField] private Image rageImage;

    [Header("Rage Manager")]
    [SerializeField] private RageManager rageManager;

    [Header("Rage Sprites")]
    [Tooltip("Sắp xếp từ hết Rage đến đầy Rage")]
    [SerializeField] private Sprite[] rageSprites;

    private int currentRage = -1;
    private int maxRage = -1;

    // =====================================
    // AWAKE
    // =====================================

    private void Awake()
    {
        if (rageImage == null)
        {
            rageImage = GetComponent<Image>();
        }
    }

    // =====================================
    // UPDATE
    // =====================================

    private void LateUpdate()
    {
        if (rageManager == null)
            return;

        SetRage(
            rageManager.CurrentRage,
            rageManager.MaxRage
        );
    }

    // =====================================
    // SET RAGE
    // =====================================

    public void SetRage(int rage, int max)
    {
        max = Mathf.Max(1, max);
        rage = Mathf.Clamp(rage, 0, max);

        // Tránh cập nhật UI không cần thiết.
        if (currentRage == rage &&
            maxRage == max)
        {
            return;
        }

        currentRage = rage;
        maxRage = max;

        UpdateRageUI();
    }

    // =====================================
    // UPDATE RAGE UI
    // =====================================

    private void UpdateRageUI()
    {
        if (rageImage == null)
        {
            Debug.LogWarning(
                "PlayerRageBar: Chưa gán Rage Image!"
            );
            return;
        }

        if (rageSprites == null ||
            rageSprites.Length == 0)
        {
            Debug.LogWarning(
                "PlayerRageBar: Chưa gán Rage Sprites!"
            );
            return;
        }

        float ragePercent =
            (float)currentRage / maxRage;

        // Quy đổi % Rage thành vị trí Sprite.
        int spriteIndex = Mathf.RoundToInt(
            ragePercent * (rageSprites.Length - 1)
        );

        spriteIndex = Mathf.Clamp(
            spriteIndex,
            0,
            rageSprites.Length - 1
        );

        Sprite selectedSprite =
            rageSprites[spriteIndex];

        if (selectedSprite != null)
        {
            rageImage.type = Image.Type.Simple;

            // Không sử dụng Fill Amount.
            rageImage.fillAmount = 1f;

            rageImage.sprite = selectedSprite;
        }
    }

    // =====================================
    // GETTERS
    // =====================================

    public int GetCurrentRage()
    {
        return Mathf.Max(0, currentRage);
    }

    public int GetMaxRage()
    {
        return Mathf.Max(1, maxRage);
    }
}
