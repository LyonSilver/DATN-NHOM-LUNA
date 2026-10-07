using TMPro;
using UnityEngine;

public class PurchasePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    public void Show(string title, string message)
    {
        titleText.text = title;
        messageText.text = message;

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}