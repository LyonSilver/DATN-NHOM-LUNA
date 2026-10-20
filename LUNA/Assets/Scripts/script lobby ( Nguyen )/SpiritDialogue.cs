using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SpiritDialogue : MonoBehaviour
{
    public GameObject spirit;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button confirmButton;
    public CanvasGroup panelGroup;
    public CanvasGroup blockGroup; // Panel trong suốt chặn click

    public float typeSpeed = 0.03f;

    private string[] lines = {
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
    private bool isTyping;

    void Start()
    {
        if (PlayerPrefs.GetInt("DialogueDone", 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        // Bật chặn click
        if (blockGroup != null)
        {
            blockGroup.gameObject.SetActive(true);
            blockGroup.blocksRaycasts = true;
            blockGroup.alpha = 0f;
        }

        spirit.SetActive(true);
        dialoguePanel.SetActive(true);
        confirmButton.onClick.AddListener(NextLine);
        confirmButton.gameObject.SetActive(false);
        ShowLine();
    }

    void ShowLine()
    {
        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TypeLine(lines[index]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        confirmButton.gameObject.SetActive(false);
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        isTyping = false;
        confirmButton.gameObject.SetActive(true);
    }

    public void NextLine()
    {
        if (isTyping) return;
        index++;
        ShowLine();
    }

    public void ResetDialogue()
    {
        PlayerPrefs.DeleteKey("DialogueDone");
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void EndDialogue()
    {
        PlayerPrefs.SetInt("DialogueDone", 1);
        PlayerPrefs.Save();

        if (blockGroup != null)
            blockGroup.gameObject.SetActive(false);

        spirit.SetActive(false);
        dialoguePanel.SetActive(false);
    }
}