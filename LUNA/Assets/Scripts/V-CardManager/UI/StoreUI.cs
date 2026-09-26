using TMPro;
using UnityEngine;

public class StoreUI : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private CardDatabase cardDatabase;

    [Header("Managers")]
    [SerializeField] private StoreManager storeManager;
    [SerializeField] private CurrencyManager currencyManager;

    [Header("Store")]
    [SerializeField] private Transform storePanel;
    [SerializeField] private GameObject storeCardPrefab;

    [Header("Currency UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text energyText;

    private void Start()
    {
        RefreshStore();
        UpdateCurrencyUI();
    }

    public void RefreshStore()
    {
        ClearStore();

        if (cardDatabase == null)
        {
            Debug.LogError("StoreUI: CardDatabase chưa được gán.");
            return;
        }

        for (int i = 0; i < cardDatabase.GetStoreCards().Count; i++)
        {
            CardData card =
                cardDatabase.GetStoreCards()[i];

            GameObject cardObject =
                Instantiate(storeCardPrefab, storePanel);

            StoreCardUI cardUI =
                cardObject.GetComponent<StoreCardUI>();

            cardUI.Setup(card, this);
        }
    }

    private void ClearStore()
    {
        if (storePanel == null)
        {
            return;
        }

        for (int i = storePanel.childCount - 1; i >= 0; i--)
        {
            Destroy(storePanel.GetChild(i).gameObject);
        }
    }

    public void BuyCard(CardData card)
    {
        PurchaseResult result =
            storeManager.BuyCard(card);

        switch (result)
        {
            case PurchaseResult.Success:

                Debug.Log(
                    "Mua thành công: " +
                    card.cardName
                );

                UpdateCurrencyUI();

                break;

            case PurchaseResult.InvalidCard:

                Debug.Log(
                    "Card không hợp lệ."
                );

                break;

            case PurchaseResult.InventoryFull:

                Debug.Log(
                    "Đã đạt giới hạn Card: " +
                    card.cardName
                );

                break;

            case PurchaseResult.NotEnoughMoney:

                Debug.Log(
                    "Không đủ Money."
                );

                break;

            case PurchaseResult.NotEnoughEnergy:

                Debug.Log(
                    "Không đủ Energy."
                );

                break;

            case PurchaseResult.NotEnoughMoneyAndEnergy:

                Debug.Log(
                    "Không đủ Money và Energy."
                );

                break;
        }
    }

    public void UpdateCurrencyUI()
    {
        if (currencyManager == null)
        {
            return;
        }

        moneyText.text =
            "Money: " + currencyManager.GetMoney();

        energyText.text =
            "Energy: " + currencyManager.GetEnergy();
    }
}