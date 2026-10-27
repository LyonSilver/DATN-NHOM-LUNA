using System.Collections;
using UnityEngine;

public class KiritoVFXController : MonoBehaviour
{
    [Header("Slash VFX")]
    [SerializeField] private Animator slashAnimator;
    [SerializeField] private SpriteRenderer slashRenderer;
    [SerializeField] private AnimationClip slashClip;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        // Ẩn hiệu ứng khi bắt đầu
        if (slashRenderer != null)
        {
            slashRenderer.enabled = false;
        }
    }

    // VFX chém bình thường
    public void PlaySlashNormal()
    {
        PlaySlash(false);
    }

    // VFX chém lật theo trục Y
    public void PlaySlashFlippedY()
    {
        PlaySlash(true);
    }

    private void PlaySlash(bool flipY)
    {
        if (slashAnimator == null ||
            slashRenderer == null ||
            slashClip == null)
        {
            Debug.LogWarning("Chưa gán đủ Slash VFX!");
            return;
        }

        // Dừng bộ đếm ẩn VFX cũ
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // Chỉnh hướng VFX
        slashRenderer.flipY = flipY;

        // Hiện VFX
        slashRenderer.enabled = true;

        // Phát animation từ đầu
        slashAnimator.Play("Slash", 0, 0f);

        // Tự ẩn khi animation kết thúc
        hideCoroutine = StartCoroutine(
            HideVFXAfterDelay(slashClip.length)
        );
    }

    private IEnumerator HideVFXAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        slashRenderer.enabled = false;
        hideCoroutine = null;
    }
}