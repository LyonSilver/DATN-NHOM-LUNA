using UnityEngine;
using TMPro;
using System.Collections;

public class SaveNotificationUI : MonoBehaviour
{
    public static SaveNotificationUI Instance { get; private set; }

    [Header("UI")]
    public TMP_Text notificationText;
    public GameObject notificationPanel;

    [Header("Settings")]
    public float showDuration = 2.5f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    public void Show(string message)
    {
        if (notificationText == null)
        {
            Debug.Log(message);
            return;
        }

        notificationText.text = message;

        if (notificationPanel != null)
            notificationPanel.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(showDuration);

        if (notificationPanel != null)
            notificationPanel.SetActive(false);
        else if (notificationText != null)
            notificationText.text = "";

        hideCoroutine = null;
    }

    // English helpers
    public void ShowSavedSlot1() => Show("Game Saved - Slot 1");
    public void ShowSavedSlot2() => Show("Game Saved - Slot 2");
    public void ShowSavedAuto()  => Show("Game Saved - Auto Save");
}