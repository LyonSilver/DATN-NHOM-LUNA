using UnityEngine;

[CreateAssetMenu(fileName = "Skin_", menuName = "Game/Skin Data")]
public class SkinData : ScriptableObject
{
    public GameObject auraPrefab;          // Particle hoặc mesh aura
    public Color rarityColor = Color.white;
    public string skinId;
    public string displayName;
    public Sprite icon;
    public GameObject visualPrefab;          // Model + Animator (hoặc chỉ mesh)
    public Material[] overrideMaterials;     // Nếu muốn đổi material
    public ParticleSystem equipVFX;
    public AudioClip equipSFX;
    public SkinRarity rarity = SkinRarity.Common;
    public bool isVIPExclusive;
    public int unlockPrice;                  // Coin / Gem / Level...
}

public enum SkinRarity { Common, Rare, Epic, Legendary, VIP }