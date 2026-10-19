using UnityEngine;

public class GachaTestController : MonoBehaviour
{
    [Header("Gacha Reference")]
    [SerializeField] private GachaManager gachaManager;

    [Header("Currency Reference")]
    [SerializeField] private CurrencyManager currencyManager;

    private void Update()
    {
        // R: Quay Gacha
        if (Input.GetKeyDown(KeyCode.R))
        {
            TestRoll();
        }

        // U: Nang cap ti le the hiem
        if (Input.GetKeyDown(KeyCode.U))
        {
            TestUpgrade();
        }

        // I: Kiem tra so du
        if (Input.GetKeyDown(KeyCode.I))
        {
            PrintStatus();
        }
    }

    public void TestRoll()
    {
        if (gachaManager == null ||
            currencyManager == null)
        {
            Debug.LogError(
                "GachaTestController: Chua gan reference!"
            );
            return;
        }

        int moneyBefore = currencyManager.GetMoney();
        int gemBefore = currencyManager.GetEnergy();

        Debug.Log("===== BAT DAU QUAY GACHA =====");

        GachaResult result = gachaManager.TryRoll();

        Debug.Log("Ket qua xu ly: " + result);

        if (result == GachaResult.Success)
        {
            GachaReward reward = gachaManager.LastReward;

            if (reward != null)
            {
                if (reward.rewardType == GachaRewardType.Card)
                {
                    Debug.Log(
                        "Nhan the: " + reward.card.cardName +
                        " | Rarity: " + reward.card.rarity
                    );
                }
                else
                {
                    Debug.Log(
                        "Nhan tai nguyen: " +
                        reward.rewardType +
                        " | So luong: " + reward.amount
                    );
                }
            }
        }

        Debug.Log(
            "Coin: " + moneyBefore +
            " -> " + currencyManager.GetMoney()
        );

        Debug.Log(
            "Gem: " + gemBefore +
            " -> " + currencyManager.GetEnergy()
        );

        Debug.Log(
            "Ti le hien tai: Common " +
            gachaManager.GetCommonRate() +
            "% | Expanded " +
            gachaManager.GetExpandedRate() +
            "% | Rare " +
            gachaManager.GetRareRate() + "%"
        );

        Debug.Log("===== KET THUC QUAY GACHA =====");
    }

    public void TestUpgrade()
    {
        if (gachaManager == null ||
            currencyManager == null)
        {
            Debug.LogError(
                "GachaTestController: Chua gan reference!"
            );
            return;
        }

        int gemBefore = currencyManager.GetEnergy();

        Debug.Log("===== BAT DAU NANG CAP =====");

        GachaResult result =
            gachaManager.UpgradeRareRate();

        Debug.Log("Ket qua nang cap: " + result);

        Debug.Log(
            "Gem: " + gemBefore +
            " -> " + currencyManager.GetEnergy()
        );

        Debug.Log(
            "Ti le hien tai: Common " +
            gachaManager.GetCommonRate() +
            "% | Expanded " +
            gachaManager.GetExpandedRate() +
            "% | Rare " +
            gachaManager.GetRareRate() + "%"
        );

        Debug.Log("===== KET THUC NANG CAP =====");
    }

    public void PrintStatus()
    {
        if (gachaManager == null ||
            currencyManager == null)
        {
            Debug.LogError(
                "GachaTestController: Chua gan reference!"
            );
            return;
        }

        Debug.Log("===== GACHA STATUS =====");
        Debug.Log("Coin: " + currencyManager.GetMoney());
        Debug.Log("Gem: " + currencyManager.GetEnergy());

        Debug.Log(
            "Da nang cap: " + gachaManager.IsUpgraded()
        );

        Debug.Log(
            "Ti le: " +
            gachaManager.GetCommonRate() + "% / " +
            gachaManager.GetExpandedRate() + "% / " +
            gachaManager.GetRareRate() + "%"
        );
    }
}