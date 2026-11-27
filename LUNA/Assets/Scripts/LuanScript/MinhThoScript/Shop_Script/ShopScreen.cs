using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopScreen : MonoBehaviour
{
    [Header("Dữ liệu")]
    public CardDatabase cardDatabase;

    [Header("Logic có sẵn trong scene")]
    public StoreManager storeManager;
    public CurrencyManager currencyManager;
    public CardInventory cardInventory;

    [Header("Danh sách thẻ (cuộn ngang)")]
    public ShopCardItem itemPrefab;
    public RectTransform content;
    public ScrollRect scrollRect;

    [Header("Nút lọc hệ (bên trái)")]
    public Button metalButton;
    public Button woodButton;
    public Button waterButton;
    public Button fireButton;
    public Button earthButton;

    [Header("Nút lọc mức độ")]
    public Button allTierButton;
    public Button normalButton;
    public Button highButton;
    public Button extremeButton;

    [Header("Tiền tệ - Header UI")]
    [SerializeField] private TMP_Text coinValueText;
    [SerializeField] private TMP_Text gemValueText;

    [Header("Màu nút")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color32(255, 215, 120, 255);

    [Header("Mặc định khi mở Shop")]
    public CardElement startElement = CardElement.Metal;

    private static readonly CardType[] TypeOrder =
    {
        CardType.Attack,
        CardType.Defense,
        CardType.Poison,
        CardType.Heal,
        CardType.Focus,
        CardType.Chaos
    };

    private CardElement currentElement;
    private CardTier currentTier = CardTier.None;
    private bool started;

    // =====================================================
    // INITIALIZATION
    // =====================================================

    private void Start()
    {
        Bind(metalButton, () => SelectElement(CardElement.Metal));
        Bind(woodButton, () => SelectElement(CardElement.Wood));
        Bind(waterButton, () => SelectElement(CardElement.Water));
        Bind(fireButton, () => SelectElement(CardElement.Fire));
        Bind(earthButton, () => SelectElement(CardElement.Earth));

        Bind(allTierButton, () => SelectTier(CardTier.None));
        Bind(normalButton, () => SelectTier(CardTier.Normal));
        Bind(highButton, () => SelectTier(CardTier.High));
        Bind(extremeButton, () => SelectTier(CardTier.Extreme));

        currentElement = startElement == CardElement.None
            ? CardElement.Metal
            : startElement;

        started = true;

        ValidateReferences();
        RefreshButtonColors();
        UpdateCurrency();
        Rebuild();
    }

    private void OnEnable()
    {
        if (!started)
            return;

        UpdateCurrency();
        RefreshItems();
    }

    private void ValidateReferences()
    {
        if (currencyManager == null)
            Debug.LogError("ShopScreen: Chưa gán CurrencyManager.", this);

        if (storeManager == null)
Debug.LogError("ShopScreen: Chưa gán StoreManager.", this);

        if (cardInventory == null)
            Debug.LogError("ShopScreen: Chưa gán CardInventory.", this);

        if (coinValueText == null)
            Debug.LogError("ShopScreen: Chưa gán Coin Value Text.", this);

        if (gemValueText == null)
            Debug.LogError("ShopScreen: Chưa gán Gem Value Text.", this);
    }

    private void Bind(Button button, UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    // =====================================================
    // FILTER
    // =====================================================

    public void SelectElement(CardElement element)
    {
        if (element == currentElement)
            return;

        currentElement = element;

        RefreshButtonColors();
        Rebuild();
    }

    public void SelectTier(CardTier tier)
    {
        if (tier == currentTier)
            return;

        currentTier = tier;

        RefreshButtonColors();
        Rebuild();
    }

    private void RefreshButtonColors()
    {
        SetButtonColor(metalButton, currentElement == CardElement.Metal);
        SetButtonColor(woodButton, currentElement == CardElement.Wood);
        SetButtonColor(waterButton, currentElement == CardElement.Water);
        SetButtonColor(fireButton, currentElement == CardElement.Fire);
        SetButtonColor(earthButton, currentElement == CardElement.Earth);

        SetButtonColor(allTierButton, currentTier == CardTier.None);
        SetButtonColor(normalButton, currentTier == CardTier.Normal);
        SetButtonColor(highButton, currentTier == CardTier.High);
        SetButtonColor(extremeButton, currentTier == CardTier.Extreme);
    }

    private void SetButtonColor(Button button, bool selected)
    {
        if (button == null || button.targetGraphic == null)
            return;

        button.targetGraphic.color =
            selected ? selectedColor : normalColor;
    }

    // =====================================================
    // CARD LIST
    // =====================================================

    private void Rebuild()
    {
        if (cardDatabase == null ||
            itemPrefab == null ||
            content == null)
        {
            Debug.LogError(
                "ShopScreen: Thiếu CardDatabase, ItemPrefab hoặc Content.",
                this
            );

            return;
        }

        ClearContent();

        List<CardData> list = new List<CardData>();

        foreach (CardData card in cardDatabase.GetStoreCards())
        {
            if (card == null || card.tier == CardTier.None)
                continue;

            if (card.element != currentElement)
                continue;

            if (currentTier != CardTier.None &&
                card.tier != currentTier)
                continue;
list.Add(card);
        }

        list.Sort(CompareCards);

        foreach (CardData card in list)
        {
            ShopCardItem item = Instantiate(itemPrefab, content);
            item.Setup(card, this, GetOwned(card));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        if (scrollRect != null)
            scrollRect.horizontalNormalizedPosition = 0f;

        Debug.Log(
            "ShopScreen: Hệ " +
            ElementRules.GetNameVN(currentElement) +
            " | Tier " +
            (currentTier == CardTier.None ? "All" : currentTier.ToString()) +
            " | " + list.Count + " Cards."
        );
    }

    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;

            child.SetActive(false);
            Destroy(child);
        }
    }

    private int CompareCards(CardData a, CardData b)
    {
        int byTier = ((int)a.tier).CompareTo((int)b.tier);

        if (byTier != 0)
            return byTier;

        return TypeIndex(a.cardType).CompareTo(TypeIndex(b.cardType));
    }

    private int TypeIndex(CardType type)
    {
        int index = System.Array.IndexOf(TypeOrder, type);

        return index < 0 ? 99 : index;
    }

    private int GetOwned(CardData card)
    {
        return cardInventory != null
            ? cardInventory.GetCardQuantity(card)
            : 0;
    }

    private void RefreshItems()
    {
        if (content == null)
            return;

        foreach (Transform child in content)
        {
            ShopCardItem item = child.GetComponent<ShopCardItem>();

            if (item != null && item.gameObject.activeSelf)
                item.Refresh(GetOwned(item.Card));
        }
    }

    // =====================================================
    // CURRENCY UI
    // =====================================================

    public void UpdateCurrency()
    {
        if (currencyManager == null)
            return;

        int currentCoin = currencyManager.GetMoney();
        int currentGem = currencyManager.GetGem();

        if (coinValueText != null)
            coinValueText.text = currentCoin.ToString("N0");

        if (gemValueText != null)
            gemValueText.text = currentGem.ToString("N0");
    }

    // =====================================================
    // BUY CARD
    // =====================================================

    public void OnBuyRequested(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("ShopScreen: Card không hợp lệ.");
            return;
        }

        if (storeManager == null)
        {
            Debug.LogError("ShopScreen: Chưa gán StoreManager.");
            return;
        }

        PurchaseResult result = storeManager.BuyCard(card);

        if (result == PurchaseResult.Success)
        {
Debug.Log(
                "MUA THÀNH CÔNG: " + card.cardName +
                " | -" + card.moneyCost + " Coins" +
                " | -" + card.gemCost + " Gems"
            );
        }
        else
        {
            Debug.LogWarning(
                "MUA THẤT BẠI: " + card.cardName +
                " | Kết quả: " + result
            );
        }

        // Đọc lại số dư thực tế từ CurrencyManager.
        UpdateCurrency();

        // Cập nhật giới hạn sở hữu của các Card đang hiển thị.
        RefreshItems();
    }
}