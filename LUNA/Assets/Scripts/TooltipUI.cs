using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private GameObject tooltipPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Hide();
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
            transform.position = Input.mousePosition;
    }

    public void Show(string title, string content)
    {
        if (titleText) titleText.text = title;
        if (contentText) contentText.text = content;
        if (tooltipPanel) tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }
}