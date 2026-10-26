using UnityEngine;
using UnityEngine.UI;

public class SkinPreviewController : MonoBehaviour
{
    public Transform previewAnchor;
    public Camera previewCamera;
    public RawImage previewImage;
    public float rotateSpeed = 25f;
    public bool autoRotate = true;
    public bool is25D = true;               // bật cái này

    private GameObject currentPreview;
    private GameObject currentAura;
    private RenderTexture rt;

    void Start()
    {
        rt = new RenderTexture(512, 512, 16);
        previewCamera.targetTexture = rt;
        previewImage.texture = rt;

        if (is25D)
        {
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = 2.2f;
            previewCamera.transform.position = new Vector3(0, 1f, -10f);
            previewCamera.transform.rotation = Quaternion.identity;
        }
    }

    public void ShowSkin(SkinData skin)
    {
        if (currentPreview) Destroy(currentPreview);
        if (currentAura) Destroy(currentAura);

        if (skin.visualPrefab)
        {
            currentPreview = Instantiate(skin.visualPrefab, previewAnchor);
            currentPreview.transform.localPosition = Vector3.zero;
            currentPreview.transform.localRotation = Quaternion.identity;

            var anim = currentPreview.GetComponent<Animator>();
            if (anim) anim.Play("Idle");
        }

        if (skin.auraPrefab)
        {
            currentAura = Instantiate(skin.auraPrefab, previewAnchor);
            currentAura.transform.localPosition = Vector3.zero;
        }
    }

    void Update()
    {
        if (!autoRotate || currentPreview == null) return;

        if (is25D)
            currentPreview.transform.Rotate(0, rotateSpeed * Time.deltaTime, 0); // chỉ xoay Y nhẹ
        else
            currentPreview.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}