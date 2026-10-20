using UnityEngine;
using UnityEngine.UI;

public class DialoguePanelSetup : MonoBehaviour
{
    public Image panelImage;
    public float corner = 0.26f;
    public float glow = 0.18f;
    public Color panelColor = new Color(0.2f, 0.1f, 0.35f, 0.7f);

    void Start()
    {
        if (panelImage == null) return;

        panelImage.color = panelColor;

        // Nếu dùng Material SoftPanel
        if (panelImage.material != null)
        {
            panelImage.material.SetFloat("_Corner", corner);
            panelImage.material.SetFloat("_Glow", glow);
        }
    }
}