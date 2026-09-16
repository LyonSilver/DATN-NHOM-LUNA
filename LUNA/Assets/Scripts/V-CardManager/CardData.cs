using UnityEngine;

public enum CardType
{
    Attack,
    Guard,
    Effect,
    Support,
    Chaos
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Information")]
    public string cardID;
    public string cardName;

    [Header("Card Cost")]
    public int cost;

    [Header("Card Type")]
    public CardType cardType;

    [Header("Card Value")]
    public int baseValue;

    [Header("Card Description")]
    [TextArea(2, 5)]
    public string description;

    [Header("Card Visual")]
    public Sprite cardSprite;
}