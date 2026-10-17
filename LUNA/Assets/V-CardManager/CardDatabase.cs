using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Card System/Card Database")]
public class CardDatabase : ScriptableObject
{
    [Header("Cards Available In Store")]
    [SerializeField] private List<CardData> storeCards =
        new List<CardData>();

    public List<CardData> GetStoreCards()
    {
        return storeCards;
    }
}