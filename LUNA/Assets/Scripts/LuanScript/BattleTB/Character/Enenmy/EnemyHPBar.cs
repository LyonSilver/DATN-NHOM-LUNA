using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    [Header("UI")]
    [SerializeField] private Image hpImage;

    [Header("HP Sprites")]
    [SerializeField] private Sprite fullHP;
    [SerializeField] private Sprite highHP;
    [SerializeField] private Sprite midHP;
    [SerializeField] private Sprite lowHP;
    [SerializeField] private Sprite emptyHP;

    public void SetHP(int hp)
    {
        currentHP = Mathf.Clamp(hp, 0, maxHP);
        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        float hpPercent = (float)currentHP / maxHP;

        if (hpPercent > 0.8f)
        {
            hpImage.sprite = fullHP;
        }
        else if (hpPercent > 0.6f)
        {
            hpImage.sprite = highHP;
        }
        else if (hpPercent > 0.4f)
        {
            hpImage.sprite = midHP;
        }
        else if (hpPercent > 0.1f)
        {
            hpImage.sprite = lowHP;
        }
        else
        {
            hpImage.sprite = emptyHP;
        }
    }
}