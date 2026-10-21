// using UnityEngine;

// public enum CardType1
// {
//     Attack,
//     Guard,
//     Effect,
//     Support,
//     Chaos
// }

// public enum CardRarity
// {
//     Common,
//     Rare,
//     Epic,
//     Legendary
// }

// public enum PurchaseType
// {
//     Money,
//     Energy,
//     MoneyAndEnergy
// }

// [CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card Data")]
// public class CardData1 : ScriptableObject
// {
//     [Header("Card Information")]
//     public string cardID;
//     public string cardName;

//     public CardType cardType;
//     public CardRarity rarity;

//     [TextArea(2, 5)]
//     public string description;

//     [Header("Card Value")]
//     public int baseValue;

//     [Header("Purchase")]
//     public PurchaseType purchaseType;

//     public int moneyCost;
//     public int energyCost;

//     [Header("Inventory")]
//     public int maxQuantity;

//     [Header("Visual")]
//     public Sprite cardSprite;
// }