using TMPro;
using UnityEngine;
using UnityEngine.UI;
//sửa code
using System.Collections;

public class GachaUIManager : MonoBehaviour
{
    [Header("Gacha System")]
    [SerializeField] private GachaManager gachaManager;
    [SerializeField] private CurrencyManager currencyManager;

    [Header("Button Quay Thuong")]
    [SerializeField] private Button rollButton;
    [SerializeField] private CanvasGroup rollButtonGroup;

    [Header("Button Upgrade")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private CanvasGroup upgradeButtonGroup;

    [Header("Currency UI - Khong bat buoc")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text gemText;

    [Header("Gacha Popup")]
    [SerializeField] private GameObject gachaPopupPanel;
    [SerializeField] private GameObject modalOverlayImage;
    [SerializeField] private Button okButton;

    [Header("Reward UI - Cac object ban da tao")]
    [SerializeField] private Image rewardIconImage;
    [SerializeField] private TMP_Text rewardNameText;
    [SerializeField] private TMP_Text rewardAmountText;
    [SerializeField] private TMP_Text compensationText;

    [Header("Reward Sprites")]
    [SerializeField] private Sprite coinSprite;
    [SerializeField] private Sprite gemSprite;

    //sửa code 
    [Header("Gacha Animation")]
    [SerializeField] private RectTransform storePanel;
    [SerializeField] private RectTransform zoomTarget;
    [SerializeField] private GameObject gachaOpenVFX;

    [SerializeField] private float zoomScale = 1.6f;
    [SerializeField] private float zoomDuration = 0.5f;
    [SerializeField] private float effectDuration = 2.5f;

    private Vector3 originalScale;
    private Vector2 originalPosition;
    private bool isRolling = false;

    //--//

    private bool isPopupOpen = false;

    private void Awake()
    {
        // An popup khi bat dau.
        if (gachaPopupPanel != null)
            gachaPopupPanel.SetActive(false);

        if (modalOverlayImage != null)
            modalOverlayImage.SetActive(false);

        // An dong thong bao bu Coin ban dau.
        if (compensationText != null)
            compensationText.gameObject.SetActive(false);

        // Tu dong ket noi cac button.
        if (rollButton != null)
            rollButton.onClick.AddListener(OnRollButtonClicked);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        if (okButton != null)
            okButton.onClick.AddListener(OnOKButtonClicked);

        //sửa code
        // Lưu trạng thái ban đầu của giao diện
        if (storePanel != null)
        {
            originalScale = storePanel.localScale;
            originalPosition = storePanel.anchoredPosition;
        }

        // Tắt VFX khi bắt đầu
        if (gachaOpenVFX != null)
        {
            gachaOpenVFX.SetActive(false);
        }
        //--//
    }

    private void OnEnable()
    {
        if (currencyManager != null)
            currencyManager.OnCurrencyChanged += RefreshCurrencyUI;
    }

    private void Start()
    {
        RefreshCurrencyUI();
        RefreshButtonStates();
    }

    private void OnDisable()
    {
        if (currencyManager != null)
            currencyManager.OnCurrencyChanged -= RefreshCurrencyUI;
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
        // Cap nhat trang thai nut neu tien thay doi
        // truc tiep trong Inspector khi dang Play.
        RefreshButtonStates();
    }

    private void RefreshCurrencyUI()
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

        // Khoa hai nut khi popup dang mo.
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

    // public void OnRollButtonClicked()
    // {
    //     if (gachaManager == null || isPopupOpen)
    //         return;

    //     GachaResult result = gachaManager.TryRoll();

    //     if (result == GachaResult.Success)
    //     {
    //         ShowReward(gachaManager.LastReward);
    //     }
    //     else
    //     {
    //         Debug.Log("Gacha quay không thành công: " + result);
    //     }

    //     RefreshCurrencyUI();
    // }

    //sửa code
    public void OnRollButtonClicked()
    {
        if (gachaManager == null || isPopupOpen || isRolling)
            return;

        GachaResult result = gachaManager.TryRoll();

        if (result == GachaResult.Success)
        {
            isRolling = true;
            isPopupOpen = true;

            RefreshCurrencyUI();

            StartCoroutine(PlayGachaAnimation());
        }
        else
        {
            Debug.Log("Gacha quay khong thanh cong: " + result);
        }
    }
    //--//
    public void OnUpgradeButtonClicked()
    {
        if (gachaManager == null || isPopupOpen)
            return;

        GachaResult result = gachaManager.UpgradeRareRate();

        switch (result)
        {
            case GachaResult.Success:
                ShowMessage(
                    "Nâng cấp thành công!\nTỉ lệ thẻ hiếm đã tăng."
                );
                break;

            case GachaResult.NotEnoughGem:
                ShowMessage(
                    "Nâng cấp thất bại!\nKhông đủ Gem."
                );
                break;

            case GachaResult.UpgradeAlreadyPurchased:
                ShowMessage(
                    "Bạn đã nâng cấp tỉ lệ thẻ hiếm rồi!"
                );
                break;

            case GachaResult.InvalidConfiguration:
                ShowMessage(
                    "Nâng cấp thất bại!\nCấu hình Gacha không hợp lệ."
                );
                break;

            default:
                ShowMessage("Nâng cấp thất bại!");
                break;
        }

        RefreshCurrencyUI();
    }

    private void ShowReward(GachaReward reward)
    {
        if (reward == null)
            return;

        // Hien lai cac thanh phan phan thuong
        if (rewardIconImage != null)
            rewardIconImage.gameObject.SetActive(true);

        if (rewardNameText != null)
            rewardNameText.gameObject.SetActive(true);

        if (rewardAmountText != null)
            rewardAmountText.gameObject.SetActive(true);

        // An thong bao cu
        if (compensationText != null)
        {
            compensationText.text = "";
            compensationText.gameObject.SetActive(false);
        }

        if (rewardIconImage != null)
        {
            rewardIconImage.sprite = null;
            rewardIconImage.enabled = true;

            if (reward.rewardType == GachaRewardType.Card)
            {
                if (reward.card != null)
                    rewardIconImage.sprite = reward.card.cardSprite;
            }
            else if (reward.rewardType == GachaRewardType.Coin)
            {
                rewardIconImage.sprite = coinSprite;
            }
            else if (reward.rewardType == GachaRewardType.Gem)
            {
                rewardIconImage.sprite = gemSprite;
            }

            if (rewardIconImage.sprite == null)
                rewardIconImage.enabled = false;
        }

        if (rewardNameText != null)
        {
            if (reward.rewardType == GachaRewardType.Card)
            {
                rewardNameText.text = reward.card != null
                    ? reward.card.cardName
                    : "Lá bài";
            }
            else if (reward.rewardType == GachaRewardType.Coin)
            {
                rewardNameText.text = "Coin";
            }
            else
            {
                rewardNameText.text = "Gem";
            }
        }

        if (rewardAmountText != null)
        {
            if (reward.rewardType == GachaRewardType.Card)
                rewardAmountText.text = "x" + reward.amount;
            else
                rewardAmountText.text = "+" + reward.amount;
        }

        // Chi hien thong bao neu the quay duoc da full.
        if (reward.wasCompensation && compensationText != null)
        {
            string cardName = reward.replacedCard != null
                ? reward.replacedCard.cardName
                : "Lá bài";

            compensationText.text =
                "Đã full lượt sở hữu của " + cardName +
                ". Nhận bù +" + reward.amount + " Coin.";

            compensationText.gameObject.SetActive(true);
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

        RefreshCurrencyUI();
    }

    private void ShowMessage(string message)
    {
        // An thong tin phan thuong cua lan quay truoc
        if (rewardIconImage != null)
        {
            rewardIconImage.sprite = null;
            rewardIconImage.enabled = false;
            rewardIconImage.gameObject.SetActive(false);
        }

        if (rewardNameText != null)
            rewardNameText.gameObject.SetActive(false);

        if (rewardAmountText != null)
            rewardAmountText.gameObject.SetActive(false);

        // Dung compesationText de hien thi thong bao
        if (compensationText != null)
        {
            compensationText.text = message;
            compensationText.gameObject.SetActive(true);
        }

        // Mo popup
        isPopupOpen = true;

        if (modalOverlayImage != null)
            modalOverlayImage.SetActive(true);

        if (gachaPopupPanel != null)
            gachaPopupPanel.SetActive(true);

        RefreshButtonStates();
    }

    //sửa code
    private IEnumerator PlayGachaAnimation()
    {
        // Lưu lại trạng thái ban đầu trước mỗi lần quay
        if (storePanel != null)
        {
            originalScale = storePanel.localScale;
            originalPosition = storePanel.anchoredPosition;
        }

        // Bật hiệu ứng cổng không gian
        if (gachaOpenVFX != null)
        {
            gachaOpenVFX.SetActive(true);

            ParticleSystem[] particles =
                gachaOpenVFX.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particle in particles)
            {
                particle.Clear();
                particle.Play(true);
            }
        }

        // Zoom về phía cổng không gian
        if (storePanel != null && zoomTarget != null)
        {
            yield return StartCoroutine(
                ZoomStoreToTarget(zoomScale, zoomDuration)
            );
        }

        // Giữ hiệu ứng trong thời gian đã thiết lập
        yield return new WaitForSeconds(effectDuration);

        // Trả giao diện về trạng thái ban đầu
        yield return StartCoroutine(RestoreStoreAnimated());

        // Tắt VFX
        if (gachaOpenVFX != null)
        {
            gachaOpenVFX.SetActive(false);
        }

        // Chỉ mở popup sau khi animation kết thúc
        isRolling = false;
        ShowReward(gachaManager.LastReward);
    }

    private IEnumerator ZoomStoreToTarget(float targetScale, float duration)
    {
        if (storePanel == null || zoomTarget == null)
            yield break;

        RectTransform parentRect = storePanel.parent as RectTransform;

        if (parentRect == null)
            yield break;

        // Lấy tọa độ tâm cổng trong hệ tọa độ của StorePanel
        Vector3 targetLocal =
            storePanel.InverseTransformPoint(zoomTarget.position);

        Vector2 startPosition = storePanel.anchoredPosition;
        Vector3 startScale = storePanel.localScale;

        // Với parent có tâm tại (0, 0), đưa tâm cổng về tâm màn hình
        Vector2 targetPosition = new Vector2(
            -targetLocal.x * targetScale,
            -targetLocal.y * targetScale
        );

        Vector3 endScale = originalScale * targetScale;

        float timer = 0f;
        duration = Mathf.Max(duration, 0.01f);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);

            // Làm chuyển động mượt hơn
            t = t * t * (3f - 2f * t);

            storePanel.localScale =
                Vector3.Lerp(startScale, endScale, t);

            storePanel.anchoredPosition =
                Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        storePanel.localScale = endScale;
        storePanel.anchoredPosition = targetPosition;
    }

    private IEnumerator RestoreStoreAnimated()
    {
        if (storePanel == null)
            yield break;

        Vector3 startScale = storePanel.localScale;
        Vector2 startPosition = storePanel.anchoredPosition;

        float duration = Mathf.Max(zoomDuration, 0.01f);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = t * t * (3f - 2f * t);

            storePanel.localScale =
                Vector3.Lerp(startScale, originalScale, t);

            storePanel.anchoredPosition =
                Vector2.Lerp(startPosition, originalPosition, t);

            yield return null;
        }

        storePanel.localScale = originalScale;
        storePanel.anchoredPosition = originalPosition;
    }
    //--//
}