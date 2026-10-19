using UnityEngine;
using TMPro;
using System.Collections;

public class GuidePanel : MonoBehaviour
{
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;
    public float displayTime = 6f;
    public float fadeSpeed = 8f;

    private string[] messages = {
        "Click the gate to enter battle",
        "Drag and drop cards to fight",
        "Get stronger cards at the Card Shop",
        "Complete quests to earn rewards",
        "Open Card Packs for new cards",
        "Check Mail to receive gifts",
        "Go to Settings to adjust audio",
        "Press the Home button for quick menu"
    };

    private int index;
    private Coroutine routine;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    void OnEnable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (index < messages.Length)
        {
            text.text = messages[index];
            yield return Fade(1f);
            yield return new WaitForSecondsRealtime(displayTime);
            yield return Fade(0f);
            index++;
        }
        gameObject.SetActive(false);
    }

    IEnumerator Fade(float target)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, target))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }
    }
}