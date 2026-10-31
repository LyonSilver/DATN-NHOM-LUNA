using UnityEngine;

public class DotView : MonoBehaviour
{
    [SerializeField] Color litColor = new Color32(0xA8, 0x55, 0xF7, 255);
    [SerializeField] Color unlitColor = new Color32(0x4A, 0x4A, 0x55, 255);
    [SerializeField] float litScale = 0.24f;
    [SerializeField] float unlitScale = 0.16f;

    SpriteRenderer rend;

    public void SetLit(bool lit)
    {
        if (rend == null) rend = GetComponent<SpriteRenderer>();
        rend.color = lit ? litColor : unlitColor;
        transform.localScale = Vector3.one * (lit ? litScale : unlitScale);
    }
}