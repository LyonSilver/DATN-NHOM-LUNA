using System.Collections;
using UnityEngine;

public class PlayerSkinController : MonoBehaviour
{
    [Header("References")]
    public Transform visualParent;           // Empty child để chứa model
    public Animator playerAnimator;          // Animator chính (nếu dùng chung)
    
    [Header("Effects")]
    public float dissolveDuration = 0.6f;
    public AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private GameObject currentVisual;
    private SkinData currentSkin;
    private Material[] originalMats;
    private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

    public SkinData CurrentSkin => currentSkin;

    public void ChangeSkin(SkinData newSkin, bool playEffect = true)
    {
        if (newSkin == null || newSkin == currentSkin) return;

        if (playEffect && currentVisual != null)
            StartCoroutine(DissolveAndSwitch(newSkin));
        else
            ApplySkinInstant(newSkin);
    }

    IEnumerator DissolveAndSwitch(SkinData newSkin)
    {
        // Dissolve out
        var renderers = currentVisual.GetComponentsInChildren<Renderer>();
        float t = 0f;
        while (t < dissolveDuration)
        {
            t += Time.deltaTime;
            float amount = dissolveCurve.Evaluate(t / dissolveDuration);
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                    if (mat.HasProperty(DissolveAmount))
                        mat.SetFloat(DissolveAmount, amount);
            }
            yield return null;
        }

        ApplySkinInstant(newSkin);

        // Dissolve in + VFX
        renderers = currentVisual.GetComponentsInChildren<Renderer>();
        t = 0f;
        while (t < dissolveDuration)
        {
            t += Time.deltaTime;
            float amount = 1f - dissolveCurve.Evaluate(t / dissolveDuration);
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                    if (mat.HasProperty(DissolveAmount))
                        mat.SetFloat(DissolveAmount, amount);
            }
            yield return null;
        }

        if (newSkin.equipVFX)
            Instantiate(newSkin.equipVFX, visualParent.position, Quaternion.identity);
        
        if (newSkin.equipSFX)
            AudioSource.PlayClipAtPoint(newSkin.equipSFX, transform.position);
    }

    void ApplySkinInstant(SkinData skin)
    {
        if (currentVisual) Destroy(currentVisual);

        currentVisual = Instantiate(skin.visualPrefab, visualParent);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;

        // Sync Animator nếu cần
        var newAnim = currentVisual.GetComponent<Animator>();
        if (newAnim && playerAnimator)
        {
            newAnim.runtimeAnimatorController = playerAnimator.runtimeAnimatorController;
            playerAnimator.avatar = newAnim.avatar;
        }

        currentSkin = skin;
        SkinManager.Instance?.OnSkinChanged(skin);
    }
}