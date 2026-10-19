using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasePopup : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] private GameObject modalOverlay;

    [Header("Tieu de")]
    [SerializeField] private GameObject successTitleImage;
    [SerializeField] private GameObject failureTitleImage;

    [Header("Noi dung")]
    [SerializeField] private GameObject successContentPanel;
    [SerializeField] private GameObject failureContentPanel;

    [Header("Thong tin the bai")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescriptionText;
    [SerializeField] private TMP_Text purchasedCountText;
    [SerializeField] private TMP_Text ownedCountText;

    [Header("Thong bao that bai")]
    [SerializeField] private TMP_Text failureMessageText;

    // Hien popup mua thanh cong
    public void ShowSuccess(
        string cardName,
        string cardDescription,
        Sprite cardSprite,
        int purchasedCount,
        int purchaseLimit,
        int ownedCount)
    {
        // Hien lop nen toi
        modalOverlay.SetActive(true);

        // Chon tieu de thanh cong
        successTitleImage.SetActive(true);
        failureTitleImage.SetActive(false);

        // Chi hien khung thong tin the
        successContentPanel.SetActive(true);
        failureContentPanel.SetActive(false);

        // Cap nhat thong tin the
        cardNameText.text = cardName;
        cardDescriptionText.text = cardDescription;

        cardImage.sprite = cardSprite;
        cardImage.enabled = cardSprite != null;

        purchasedCountText.text =
            "GIỚI HẠN MUA CARD: " +
            purchasedCount + "/" + purchaseLimit;

        ownedCountText.text =
            "ĐÃ SỞ HỮU: " + ownedCount;

        // Dam bao popup duoc bat
        gameObject.SetActive(true);
    }

    // Hien popup mua that bai
    public void ShowFailure(string message)
    {
        modalOverlay.SetActive(true);

        // Chon tieu de that bai
        successTitleImage.SetActive(false);
        failureTitleImage.SetActive(true);

        // Chi hien loi giai thich
        successContentPanel.SetActive(false);
        failureContentPanel.SetActive(true);

        failureMessageText.text = message;

        gameObject.SetActive(true);
    }

    // Dong popup va mo khoa giao dien Shop
    public void Close()
    {
        modalOverlay.SetActive(false);
        gameObject.SetActive(false);
    }
}