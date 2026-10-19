
using UnityEngine;
using UnityEngine.UI;

public class PlayerManaBar : MonoBehaviour
{
    [Header("Mana UI")]
    [SerializeField] private Image manaImage;

    [Header("Turn Manager")]
    [SerializeField] private TurnManager turnManager;

    [Header("Mana Sprites")]
    [Tooltip("Sắp xếp từ hết Mana đến đầy Mana")]
    [SerializeField] private Sprite[] manaSprites;

    private int currentMana = -1;
    private int maxMana = -1;

    // =====================================
    // AWAKE
    // =====================================

    private void Awake()
    {
        if (manaImage == null)
        {
            manaImage = GetComponent<Image>();
        }
    }

    // =====================================
    // UPDATE
    // =====================================

    private void LateUpdate()
    {
        if (turnManager == null)
            return;

        // Mana sử dụng chung Energy.
        // Chỉ cập nhật Sprite khi giá trị đổi.
        SetMana(
            turnManager.CurrentEnergy,
            turnManager.MaxEnergy
        );
    }

    // =====================================
    // SET MANA
    // =====================================

    public void SetMana(int mana, int max)
    {
        max = Mathf.Max(1, max);
        mana = Mathf.Clamp(mana, 0, max);

        // Tránh cập nhật UI không cần thiết.
        if (currentMana == mana &&
            maxMana == max)
        {
            return;
        }

        currentMana = mana;
        maxMana = max;

        UpdateManaUI();
    }

    // =====================================
    // UPDATE MANA UI
    // =====================================

    private void UpdateManaUI()
    {
        if (manaImage == null)
        {
            Debug.LogWarning(
                "PlayerManaBar: Chưa gán Mana Image!"
            );
            return;
        }

        if (manaSprites == null ||
            manaSprites.Length == 0)
        {
            Debug.LogWarning(
                "PlayerManaBar: Chưa gán Mana Sprites!"
            );
            return;
        }

        float manaPercent =
            (float)currentMana / maxMana;

        // Quy đổi % Mana thành vị trí Sprite.
        int spriteIndex = Mathf.RoundToInt(
            manaPercent * (manaSprites.Length - 1)
        );

        spriteIndex = Mathf.Clamp(
            spriteIndex,
            0,
            manaSprites.Length - 1
        );

        Sprite selectedSprite =
            manaSprites[spriteIndex];

        if (selectedSprite != null)
        {
            manaImage.type = Image.Type.Simple;

            // Không sử dụng Fill Amount,
            // vì dễ làm mất cả viền Mana.
            manaImage.fillAmount = 1f;

            manaImage.sprite = selectedSprite;
        }
    }

    // =====================================
    // GETTERS
    // =====================================

    public int GetCurrentMana()
    {
        return Mathf.Max(0, currentMana);
    }

    public int GetMaxMana()
    {
        return Mathf.Max(1, maxMana);
    }
}
