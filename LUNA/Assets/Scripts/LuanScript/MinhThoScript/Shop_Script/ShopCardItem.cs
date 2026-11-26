using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardItem : MonoBehaviour
{
    [Header("Thẻ")]
    public CardUI cardUI;

    [Header("Giá")]
    public GameObject coinIcon;
    public TMP_Text coinPriceText;
    public GameObject gemIcon;
    public TMP_Text gemPriceText;

    [Header("Mua")]
    public Button buyButton;
    public TMP_Text buyButtonText;
    public TMP_Text ownedText;

    public CardData Card { get; private set; }

    private ShopScreen shop;

    public void Setup(CardData card, ShopScreen owner, int owned)
    {
        Card = card;
        shop = owner;

        DisableBattleBehaviours();
        FixCardRect();

        if (cardUI != null)
        {
            cardUI.Setup(card);
        }

        ShowPrice(card);

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        Refresh(owned);
    }

    public void Refresh(int owned)
    {
        if (Card == null) return;

        int limit = Card.maxQuantity;
        bool full = owned >= limit;

        if (ownedText != null)
        {
            ownedText.text = "Owned " + owned + "/" + limit;
        }

        if (buyButton != null)
        {
            buyButton.interactable = !full;
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = full ? "MAX" : "BUY";
        }
    }

    private void ShowPrice(CardData card)
    {
        bool showCoin = card.purchaseType == PurchaseType.Money || card.purchaseType == PurchaseType.MoneyAndGem;
        bool showGem = card.purchaseType == PurchaseType.Gem || card.purchaseType == PurchaseType.MoneyAndGem;

        SetActive(coinIcon, showCoin);
        SetActive(gemIcon, showGem);

        if (coinPriceText != null)
        {
            coinPriceText.gameObject.SetActive(showCoin);
            coinPriceText.text = card.moneyCost.ToString();
        }

        if (gemPriceText != null)
        {
            gemPriceText.gameObject.SetActive(showGem);
            gemPriceText.text = card.gemCost.ToString();
        }
    }

    private static void SetActive(GameObject target, bool value)
    {
        if (target != null)
        {
            target.SetActive(value);
        }
    }

    private void OnBuyClicked()
    {
        if (shop != null && Card != null)
        {
            shop.OnBuyRequested(Card);
        }
    }

    // Lưới an toàn: tắt các script chỉ dành cho trận đấu (hover, animation, dissolve...) nếu còn trên thẻ.
    // Chỉ tắt script của project (không có namespace), giữ nguyên Image, TMP, CardUI.
    private void DisableBattleBehaviours()
    {
        foreach (MonoBehaviour behaviour in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour == null || behaviour == this || behaviour is CardUI) continue;
            if (behaviour.GetType().Namespace != null) continue;

            behaviour.enabled = false;
        }
    }

    // Một số script trận đấu đổi pivot/vị trí của thẻ lúc Awake, đặt lại để thẻ luôn nằm giữa ô.
    private void FixCardRect()
    {
        if (cardUI == null) return;

        RectTransform rect = cardUI.transform as RectTransform;
        if (rect == null) return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }
}