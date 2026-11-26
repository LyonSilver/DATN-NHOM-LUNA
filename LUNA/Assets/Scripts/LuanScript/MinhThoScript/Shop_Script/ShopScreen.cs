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

    [Header("Tiền tệ")]
    public TMP_Text coinValueText;
    public TMP_Text gemValueText;

    [Header("Màu nút")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color32(255, 215, 120, 255);

    [Header("Mặc định khi mở Shop")]
    public CardElement startElement = CardElement.Metal;

    private static readonly CardType[] TypeOrder =
    {
        CardType.Attack, CardType.Defense, CardType.Poison,
        CardType.Heal, CardType.Focus, CardType.Chaos
    };

    private CardElement currentElement;
    private CardTier currentTier = CardTier.None; // None = All
    private bool started;

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

        currentElement = startElement == CardElement.None ? CardElement.Metal : startElement;
        started = true;

        RefreshButtonColors();
        UpdateCurrency();
        Rebuild();
    }

    // Mở lại màn hình Shop (ví dụ quay về từ Inventory): cập nhật tiền và số lượng đang có
    private void OnEnable()
    {
        if (!started) return;

        UpdateCurrency();
        RefreshItems();
    }

    private void Bind(Button button, UnityAction action)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    // =====================================================
    // BỘ LỌC
    // =====================================================

    public void SelectElement(CardElement element)
    {
        if (element == currentElement) return;

        currentElement = element;
        RefreshButtonColors();
        Rebuild();
    }

    public void SelectTier(CardTier tier)
    {
        if (tier == currentTier) return;

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
        if (button == null || button.targetGraphic == null) return;

        button.targetGraphic.color = selected ? selectedColor : normalColor;
    }

    // =====================================================
    // DANH SÁCH THẺ
    // =====================================================

    private void Rebuild()
    {
        if (cardDatabase == null || itemPrefab == null || content == null)
        {
            Debug.LogError("ShopScreen: Thiếu Card Database, Item Prefab hoặc Content trong Inspector.");
            return;
        }

        ClearContent();

        List<CardData> list = new List<CardData>();

        foreach (CardData card in cardDatabase.GetStoreCards())
        {
            if (card == null || card.tier == CardTier.None) continue;
            if (card.element != currentElement) continue;
            if (currentTier != CardTier.None && card.tier != currentTier) continue;

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
        {
            scrollRect.horizontalNormalizedPosition = 0f;
        }

        Debug.Log("ShopScreen: Hệ " + ElementRules.GetNameVN(currentElement) +
                  " | Mức độ " + (currentTier == CardTier.None ? "All" : currentTier.ToString()) +
                  " -> " + list.Count + " thẻ.");
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
        if (byTier != 0) return byTier;

        return TypeIndex(a.cardType).CompareTo(TypeIndex(b.cardType));
    }

    private int TypeIndex(CardType type)
    {
        int index = System.Array.IndexOf(TypeOrder, type);
        return index < 0 ? 99 : index;
    }

    private int GetOwned(CardData card)
    {
        return cardInventory != null ? cardInventory.GetCardQuantity(card) : 0;
    }

    private void RefreshItems()
    {
        if (content == null) return;

        foreach (Transform child in content)
        {
            ShopCardItem item = child.GetComponent<ShopCardItem>();

            if (item != null && item.gameObject.activeSelf)
            {
                item.Refresh(GetOwned(item.Card));
            }
        }
    }

    // =====================================================
    // TIỀN TỆ VÀ MUA
    // =====================================================

    private void UpdateCurrency()
    {
        if (currencyManager == null) return;

        if (coinValueText != null) coinValueText.text = currencyManager.GetMoney().ToString();
        if (gemValueText != null) gemValueText.text = currencyManager.GetGem().ToString();
    }

    // ShopCardItem gọi khi bấm Buy. Popup thành công/thất bại làm ở phase S5, tạm thời ghi Console.
    public void OnBuyRequested(CardData card)
    {
        if (storeManager == null)
        {
            Debug.LogWarning("ShopScreen: Chưa gán Store Manager.");
            return;
        }

        PurchaseResult result = storeManager.BuyCard(card);

        Debug.Log("ShopScreen: Mua " + card.cardName + " (" + ElementRules.GetNameVN(card.element) +
                  ", " + card.tier + ") -> " + result);

        UpdateCurrency();
        RefreshItems();
    }

    // =====================================================
    // TEST
    // =====================================================

    [ContextMenu("TEST - Thêm 5000 Coins và 200 Gems")]
    private void TestAddMoney()
    {
        if (currencyManager == null) return;

        currencyManager.AddMoney(5000);
        currencyManager.AddGem(200);
        UpdateCurrency();
    }
}