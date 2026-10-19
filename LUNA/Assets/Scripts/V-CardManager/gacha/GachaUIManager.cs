using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUIManager : MonoBehaviour
{
    [Header("Gacha System")]
    [SerializeField] private GachaManager gachaManager;
    [SerializeField] private CurrencyManager currencyManager;

    [Header("Main Buttons")]
    [SerializeField] private Button rollButton;
    [SerializeField] private CanvasGroup rollButtonGroup;

    [SerializeField] private Button upgradeButton;
    [SerializeField] private CanvasGroup upgradeButtonGroup;

    [Header("Currency UI - Optional")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text gemText;

    [Header("Rate UI - Optional")]
    [SerializeField] private TMP_Text commonRateText;
    [SerializeField] private TMP_Text expandedRateText;
    [SerializeField] private TMP_Text rareRateText;

    [Header("Popup")]
    [SerializeField] private GameObject modalOverlayImage;
    [SerializeField] private GameObject gachaPopupPanel;
    [SerializeField] private Button okButton;

    [Header("Reward UI")]
    [SerializeField] private Image rewardIcon;
    [SerializeField] private Sprite coinSprite;
    [SerializeField] private Sprite gemSprite;

    [SerializeField] private TMP_Text rewardTitleText;
    [SerializeField] private TMP_Text rewardDescriptionText;

    [Header("Message UI - Optional")]
    [SerializeField] private TMP_Text messageText;

    private bool isPopupOpen = false;

    private void Awake()
    {
        // An popup khi bat dau game.
        if (gachaPopupPanel != null)
            gachaPopupPanel.SetActive(false);

        if (modalOverlayImage != null)
            modalOverlayImage.SetActive(false);

        if (messageText != null)
            messageText.text = "";

        // Noi su kien cho cac button.
        if (rollButton != null)
            rollButton.onClick.AddListener(OnRollButtonClicked);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        if (okButton != null)
            okButton.onClick.AddListener(OnOKButtonClicked);
    }

    private void OnEnable()
    {
        if (currencyManager != null)
            currencyManager.OnCurrencyChanged += RefreshUI;
    }

    private void Start()
    {
        RefreshUI();
        RefreshRates();
    }

    private void OnDisable()
    {
        if (currencyManager != null)
            currencyManager.OnCurrencyChanged -= RefreshUI;
    }

    private void OnDestroy()
    {
        if (rollButton != null)
            rollButton.onClick.RemoveListener(OnRollButtonClicked);

        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);

        if (okButton != null)
            okButton.onClick.RemoveListener(OnOKButtonClicked);
    }

    private void Update()
    {
        // Dong bo neu sua tien truc tiep trong Inspector khi Play.
        RefreshButtonStates();
    }

    public void RefreshUI()
    {
        if (currencyManager != null)
        {
            if (coinText != null)
                coinText.text = currencyManager.GetMoney().ToString();

            if (gemText != null)
                gemText.text = currencyManager.GetEnergy().ToString();
        }

        RefreshButtonStates();
    }

    private void RefreshButtonStates()
    {
        if (gachaManager == null || currencyManager == null)
            return;

        bool canRoll =
            currencyManager.HasEnoughMoney(
                gachaManager.GetRollCoinCost()
            );

        bool canUpgrade =
            !gachaManager.IsUpgraded() &&
            currencyManager.HasEnoughEnergy(
                gachaManager.GetUpgradeGemCost()
            );

        // Khi popup mo, khoa hai nut chinh.
        if (isPopupOpen)
        {
            canRoll = false;
            canUpgrade = false;
        }

        SetButtonState(rollButton, rollButtonGroup, canRoll);
        SetButtonState(upgradeButton, upgradeButtonGroup, canUpgrade);
    }

    private void SetButtonState(
        Button button,
        CanvasGroup group,
        bool canUse)
    {
        if (button != null)
            button.interactable = canUse;

        if (group != null)
            group.alpha = canUse ? 1f : 0.4f;
    }

    public void OnRollButtonClicked()
    {
        if (gachaManager == null)
            return;

        if (isPopupOpen)
            return;

        ClearMessage();

        GachaResult result = gachaManager.TryRoll();

        if (result == GachaResult.Success)
        {
            ShowReward(gachaManager.LastReward);
        }
        else
        {
            ShowResultMessage(result);
        }

        RefreshUI();
    }

    public void OnUpgradeButtonClicked()
    {
        if (gachaManager == null)
            return;

        if (isPopupOpen)
            return;

        ClearMessage();

        GachaResult result = gachaManager.UpgradeRareRate();

        if (result == GachaResult.Success)
        {
            ShowMessage("Nang cap thanh cong! Ti le the hiem da tang.");
            RefreshRates();
        }
        else
        {
            ShowResultMessage(result);
        }

        RefreshUI();
    }

    private void ShowReward(GachaReward reward)
    {
        if (reward == null)
        {
            ShowMessage("Khong tim thay phan thuong.");
            return;
        }

        if (rewardIcon != null)
        {
            rewardIcon.enabled = true;

            if (reward.rewardType == GachaRewardType.Card)
            {
                rewardIcon.sprite = reward.card != null
                    ? reward.card.cardSprite
                    : null;
            }
            else if (reward.rewardType == GachaRewardType.Coin)
            {
                rewardIcon.sprite = coinSprite;
            }
            else if (reward.rewardType == GachaRewardType.Gem)
            {
                rewardIcon.sprite = gemSprite;
            }

            if (rewardIcon.sprite == null)
                rewardIcon.enabled = false;
        }

        if (rewardTitleText != null)
        {
            if (reward.rewardType == GachaRewardType.Card)
                rewardTitleText.text = "Nhan duoc the bai";
            else if (reward.rewardType == GachaRewardType.Coin)
                rewardTitleText.text = reward.wasCompensation
                    ? "Nhan Coin bu"
                    : "Nhan duoc Coin";
            else
                rewardTitleText.text = "Nhan duoc Gem";
        }

        if (rewardDescriptionText != null)
        {
            if (reward.rewardType == GachaRewardType.Card &&
                reward.card != null)
            {
                rewardDescriptionText.text =
                    reward.card.cardName + "\n" +
                    reward.card.description;
            }
            else if (reward.rewardType == GachaRewardType.Coin)
            {
                string text = "+" + reward.amount + " Coin";

                if (reward.wasCompensation &&
                    reward.replacedCard != null)
                {
                    text =
                        "Da full luot so huu cua la bai: " +
                        reward.replacedCard.cardName +
                        "\nNhan Coin bu: +" + reward.amount;
                }

                rewardDescriptionText.text = text;
            }
            else
            {
                rewardDescriptionText.text =
                    "+" + reward.amount + " Gem";
            }
        }

        isPopupOpen = true;

        if (modalOverlayImage != null)
            modalOverlayImage.SetActive(true);

        if (gachaPopupPanel != null)
            gachaPopupPanel.SetActive(true);

        RefreshButtonStates();
    }

    public void OnOKButtonClicked()
    {
        if (gachaPopupPanel != null)
            gachaPopupPanel.SetActive(false);

        if (modalOverlayImage != null)
            modalOverlayImage.SetActive(false);

        isPopupOpen = false;

        ClearMessage();
        RefreshUI();
    }

    private void ShowResultMessage(GachaResult result)
    {
        switch (result)
        {
            case GachaResult.NotEnoughCoin:
                ShowMessage("Khong du Coin de quay thuong!");
                break;

            case GachaResult.NotEnoughGem:
                ShowMessage("Khong du Gem de nang cap!");
                break;

            case GachaResult.UpgradeAlreadyPurchased:
                ShowMessage("Ban da nang cap ti le roi!");
                break;

            case GachaResult.NoAvailableReward:
                ShowMessage("Khong co phan thuong kha dung!");
                break;

            case GachaResult.InvalidConfiguration:
                ShowMessage("Cau hinh gacha khong hop le. Hay kiem tra Console.");
                break;

            default:
                ShowMessage("Thao tac khong thanh cong.");
                break;
        }
    }

    private void RefreshRates()
    {
        if (gachaManager == null)
            return;

        if (commonRateText != null)
            commonRateText.text =
                "Common: " + gachaManager.GetCommonRate() + "%";

        if (expandedRateText != null)
            expandedRateText.text =
                "Expanded: " + gachaManager.GetExpandedRate() + "%";

        if (rareRateText != null)
            rareRateText.text =
                "Rare: " + gachaManager.GetRareRate() + "%";
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
        else
            Debug.Log(message);
    }

    private void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}