
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [Header("Player HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    [Header("HP Image")]
    [SerializeField] private Image hpImage;

    [Header("HP Sprites")]
    [SerializeField] private Sprite fullHP;
    [SerializeField] private Sprite highHP;
    [SerializeField] private Sprite midHP;
    [SerializeField] private Sprite lowHP;
    [SerializeField] private Sprite emptyHP;

    private void Start()
    {
        UpdateHPBar();
    }

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

        // Chỉ đổi Sprite, không cắt Image.
        hpImage.type = Image.Type.Simple;
        hpImage.fillAmount = 1f;

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
