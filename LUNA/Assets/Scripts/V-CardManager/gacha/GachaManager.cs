using System.Collections.Generic;
using UnityEngine;

public enum GachaRewardType
{
    Coin,
    Gem,
    Card
}

public enum GachaResult
{
    Success,
    InvalidConfiguration,
    NotEnoughCoin,
    NotEnoughGem,
    UpgradeAlreadyPurchased,
    NoAvailableReward
}

[System.Serializable]
public class GachaReward
{
    public GachaRewardType rewardType;
    public CardData card;
    public int amount;

    // True khi thẻ quay được đã đạt giới hạn sở hữu.
    public bool wasCompensation;

    // Lưu thẻ ban đầu để popup biết thẻ nào đã bị full.
    public CardData replacedCard;
}

public class GachaManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GachaConfig config;

    [Header("Existing Systems")]
    [SerializeField] private CardInventory cardInventory;
    [SerializeField] private CurrencyManager currencyManager;

    [Header("Upgrade State")]
    [SerializeField] private bool rareRateUpgraded = false;

    public GachaReward LastReward { get; private set; }

    public bool IsUpgraded()
    {
        return rareRateUpgraded;
    }

    public int GetCommonRate()
    {
        if (config == null) return 0;

        return rareRateUpgraded
            ? config.upgradedCommonRate
            : config.commonRate;
    }

    public int GetExpandedRate()
    {
        if (config == null) return 0;

        return rareRateUpgraded
            ? config.upgradedExpandedRate
            : config.expandedRate;
    }

    public int GetRareRate()
    {
        if (config == null) return 0;

        return rareRateUpgraded
            ? config.upgradedRareRate
            : config.rareRate;
    }

    private bool IsConfigurationValid()
    {
        if (config == null ||
            cardInventory == null ||
            currencyManager == null)
        {
            Debug.LogError(
                "GachaManager: Chua gan Config, CardInventory " +
                "hoac CurrencyManager."
            );

            return false;
        }

        int totalRate =
            GetCommonRate() +
            GetExpandedRate() +
            GetRareRate();

        if (totalRate != 100)
        {
            Debug.LogError(
                "GachaManager: Tong ti le phai bang 100%."
            );

            return false;
        }

        if (config.rollCoinCost < 0 ||
            config.minCoin < 0 ||
            config.maxCoin < config.minCoin ||
            config.maxCoin > 100 ||
            config.minGem < 0 ||
            config.maxGem < config.minGem ||
            config.upgradeGemCost < 0)
        {
            Debug.LogError(
        "Gacha Config Error: " +
        "RollCost=" + config.rollCoinCost +
        ", MinCoin=" + config.minCoin +
        ", MaxCoin=" + config.maxCoin +
        ", MinGem=" + config.minGem +
        ", MaxGem=" + config.maxGem +
        ", UpgradeCost=" + config.upgradeGemCost
    );

            return false;
        }

        return true;
    }

    public GachaResult TryRoll()
    {
        LastReward = null;

        if (!IsConfigurationValid())
        {
            return GachaResult.InvalidConfiguration;
        }

        // Chua du Coin thi khong quay.
        if (!currencyManager.HasEnoughMoney(
            config.rollCoinCost))
        {
            return GachaResult.NotEnoughCoin;
        }

        // Chon phan thuong truoc, chua tru Coin.
        int randomValue = Random.Range(0, 100);

        GachaReward reward;

        if (randomValue < GetCommonRate())
        {
            reward = RollCommonReward();
        }
        else if (
            randomValue <
            GetCommonRate() + GetExpandedRate())
        {
            reward = RollExpandedReward();
        }
        else
        {
            reward = RollRareReward();
        }

        // Khong co phan thuong hop le thi khong mat Coin.
        if (reward == null)
        {
            return GachaResult.NoAvailableReward;
        }

        // Neu the da full, doi sang Coin bu.
        if (reward.rewardType == GachaRewardType.Card)
        {
            if (reward.card == null)
            {
                return GachaResult.NoAvailableReward;
            }

            if (!cardInventory.CanAddCard(reward.card))
            {
                CardData fullCard = reward.card;

                reward = CreateCurrencyReward(
                    GachaRewardType.Coin,
                    config.minCoin,
                    config.maxCoin
                );

                reward.wasCompensation = true;
                reward.replacedCard = fullCard;
            }
        }

        // Bao ve them mot lan truoc khi tru tien.
        if (!currencyManager.HasEnoughMoney(
            config.rollCoinCost))
        {
            return GachaResult.NotEnoughCoin;
        }

        // Chi tru phi khi da xac dinh duoc phan thuong.
        currencyManager.SpendMoney(config.rollCoinCost);

        // Trao phan thuong cuoi cung.
        if (reward.rewardType == GachaRewardType.Card)
        {
            if (!cardInventory.AddCard(reward.card))
            {
                // Hoan lai phi neu khong them duoc the.
                currencyManager.AddMoney(config.rollCoinCost);
                return GachaResult.NoAvailableReward;
            }
        }
        else if (reward.rewardType == GachaRewardType.Coin)
        {
            currencyManager.AddMoney(reward.amount);
        }
        else if (reward.rewardType == GachaRewardType.Gem)
        {
            currencyManager.AddEnergy(reward.amount);
        }

        LastReward = reward;

        return GachaResult.Success;
    }

    private GachaReward RollCommonReward()
    {
        List<CardData> ownedCommonCards =
            GetEligibleCards(CardRarity.Common, true);

        // 1 lua chon Coin + cac the Common da so huu.
        int optionsCount = ownedCommonCards.Count + 1;
        int index = Random.Range(0, optionsCount);

        if (index == 0)
        {
            return CreateCurrencyReward(
                GachaRewardType.Coin,
                config.minCoin,
                config.maxCoin
            );
        }

        return CreateCardReward(ownedCommonCards[index - 1]);
    }

    private GachaReward RollExpandedReward()
    {
        List<CardData> unownedCommonCards =
            GetEligibleCards(CardRarity.Common, false);

        // 1 lua chon Gem + cac the Common chua so huu.
        int optionsCount = unownedCommonCards.Count + 1;
        int index = Random.Range(0, optionsCount);

        if (index == 0)
        {
            return CreateCurrencyReward(
                GachaRewardType.Gem,
                config.minGem,
                config.maxGem
            );
        }

        return CreateCardReward(unownedCommonCards[index - 1]);
    }

    private GachaReward RollRareReward()
    {
        List<CardData> rareCards = new List<CardData>();

        if (config.allCards == null)
        {
            return null;
        }

        for (int i = 0; i < config.allCards.Count; i++)
        {
            CardData card = config.allCards[i];

            if (card == null ||
                card.rarity == CardRarity.Common ||
                card.maxQuantity <= 0)
            {
                continue;
            }

            if (!rareCards.Contains(card))
            {
                // Co chu y giu ca the da full de doi Coin bu.
                rareCards.Add(card);
            }
        }

        // Khong co the hiem trong danh sach thi nhan Gem.
        if (rareCards.Count == 0)
        {
            return CreateCurrencyReward(
                GachaRewardType.Gem,
                config.minGem,
                config.maxGem
            );
        }

        int index = Random.Range(0, rareCards.Count);

        return CreateCardReward(rareCards[index]);
    }

    private List<CardData> GetEligibleCards(
        CardRarity rarity,
        bool mustAlreadyOwn)
    {
        List<CardData> result = new List<CardData>();

        if (config.allCards == null)
        {
            return result;
        }

        for (int i = 0; i < config.allCards.Count; i++)
        {
            CardData card = config.allCards[i];

            if (card == null ||
                card.rarity != rarity ||
                card.maxQuantity <= 0)
            {
                continue;
            }

            if (result.Contains(card))
            {
                continue;
            }

            int ownedQuantity =
                cardInventory.GetCardQuantity(card);

            bool isOwned = ownedQuantity > 0;

            if (isOwned != mustAlreadyOwn)
            {
                continue;
            }

            // Khong loai bo the da full neu no da duoc so huu.
            // The full se duoc doi sang Coin trong TryRoll().
            result.Add(card);
        }

        return result;
    }

    private GachaReward CreateCurrencyReward(
        GachaRewardType type,
        int min,
        int max)
    {
        return new GachaReward
        {
            rewardType = type,
            card = null,
            amount = Random.Range(min, max + 1),
            wasCompensation = false,
            replacedCard = null
        };
    }

    private GachaReward CreateCardReward(CardData card)
    {
        if (card == null)
        {
            return null;
        }

        return new GachaReward
        {
            rewardType = GachaRewardType.Card,
            card = card,
            amount = 1,
            wasCompensation = false,
            replacedCard = null
        };
    }

    public GachaResult UpgradeRareRate()
    {
        if (!IsConfigurationValid())
        {
            return GachaResult.InvalidConfiguration;
        }

        if (rareRateUpgraded)
        {
            return GachaResult.UpgradeAlreadyPurchased;
        }

        if (!currencyManager.HasEnoughEnergy(
            config.upgradeGemCost))
        {
            return GachaResult.NotEnoughGem;
        }

        currencyManager.SpendEnergy(config.upgradeGemCost);

        rareRateUpgraded = true;

        return GachaResult.Success;
    }
}