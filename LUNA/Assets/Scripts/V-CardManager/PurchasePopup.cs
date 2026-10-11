using TMPro;
using UnityEngine;

public class PurchasePopup : MonoBehaviour
{
    [Header("Popup Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    [Header("Background Overlay")]
    [SerializeField] private GameObject modalOverlay;

    public void Show(string title, string message)
    {
        // Cap nhat noi dung thong bao
        titleText.text = title;
        messageText.text = message;

        // Bat nen toi truoc
        modalOverlay.SetActive(true);

        // Bat popup
        gameObject.SetActive(true);
    }

    public void Close()
    {
        // Tat nen toi
        modalOverlay.SetActive(false);

        // Tat popup
        gameObject.SetActive(false);
    }
}