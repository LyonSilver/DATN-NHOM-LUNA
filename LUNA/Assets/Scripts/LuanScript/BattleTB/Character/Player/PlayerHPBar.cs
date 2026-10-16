
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [Header("Player HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    [Header("HP Image")]
    [SerializeField] private Image hpImage;

    [Header("Display Settings")]
    [SerializeField] private bool useFillAmount = true;

    [Header("HP Sprites")]
    [SerializeField] private Sprite fullHP;
    [SerializeField] private Sprite highHP;
    [SerializeField] private Sprite midHP;
    [SerializeField] private Sprite lowHP;
    [SerializeField] private Sprite emptyHP;

    // =========================================
    // START
    // =========================================

    private void Start()
    {
        UpdateHPBar();
    }

    // =========================================
    // SET HP
    // =========================================

    public void SetHP(int hp)
    {
        currentHP = Mathf.Clamp(hp, 0, maxHP);

        UpdateHPBar();
    }

    public void SetHP(int hp, int max)
    {
        maxHP = Mathf.Max(1, max);

        SetHP(hp);
    }

    // =========================================
    // UPDATE HP BAR
    // =========================================

    private void UpdateHPBar()
    {
        if (hpImage == null)
        {
            Debug.LogWarning(
                "PlayerHPBar: Chưa gán HP Image!"
            );
            return;
        }

        float hpPercent =
            (float)currentHP / maxHP;

        // =====================================
        // MODE 1: FILL AMOUNT
        // =====================================

        if (useFillAmount)
        {
            hpImage.type = Image.Type.Filled;
            hpImage.fillMethod =
                Image.FillMethod.Horizontal;

            hpImage.fillOrigin =
                (int)Image.OriginHorizontal.Left;

            hpImage.fillAmount = hpPercent;

            return;
        }

        // =====================================
        // MODE 2: SPRITE STAGES
        // =====================================

        hpImage.type = Image.Type.Simple;

        Sprite selectedSprite;

        if (hpPercent > 0.8f)
        {
            selectedSprite = fullHP;
        }
        else if (hpPercent > 0.6f)
        {
            selectedSprite = highHP;
        }
        else if (hpPercent > 0.4f)
        {
            selectedSprite = midHP;
        }
        else if (hpPercent > 0.1f)
        {
            selectedSprite = lowHP;
        }
        else
        {
            selectedSprite = emptyHP;
        }

        if (selectedSprite != null)
        {
            hpImage.sprite = selectedSprite;
        }
    }

    // =========================================
    // GETTERS
    // =========================================

    public int GetCurrentHP()
    {
        return currentHP;
    }

    public int GetMaxHP()
    {
        return maxHP;
    }

    public float GetHPPercent()
    {
        return (float)currentHP / maxHP;
    }
}
