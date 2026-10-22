using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GachaConfig",
    menuName = "Gacha/Gacha Config"
)]
public class GachaConfig : ScriptableObject
{
    [Header("Reward Pool")]
    public List<CardData> allCards = new List<CardData>();

    [Header("Gacha Cost")]
    [Min(0)]
    public int rollCoinCost = 200;

    [Header("Normal Rates (%)")]
    [Range(0, 100)] public int commonRate = 60;
    [Range(0, 100)] public int expandedRate = 30;
    [Range(0, 100)] public int rareRate = 10;

    [Header("Upgraded Rates (%)")]
    [Range(0, 100)] public int upgradedCommonRate = 40;
    [Range(0, 100)] public int upgradedExpandedRate = 30;
    [Range(0, 100)] public int upgradedRareRate = 30;

    [Header("Coin Reward - Applies to normal and compensation rewards")]
    [Min(0)] public int minCoin = 20;
    [Range(0, 100)] public int maxCoin = 100;

    [Header("Gem Reward")]
    [Min(0)] public int minGem = 10;
    [Min(0)] public int maxGem = 50;

    [Header("Upgrade")]
    [Min(0)] public int upgradeGemCost = 10000;
}