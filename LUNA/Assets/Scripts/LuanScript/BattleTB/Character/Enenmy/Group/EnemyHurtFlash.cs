
using System.Collections;
using UnityEngine;

public class EnemyHurtFlash : MonoBehaviour
{
    [Header("Enemy Health")]
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Enemy Sprite")]
    [SerializeField] private SpriteRenderer[] enemySprites;

    [Header("White Flash Material")]
    [SerializeField] private Material whiteFlashMaterial;

    [Header("Flash Settings")]
    [SerializeField, Min(1)] private int flashCount = 2;
    [SerializeField, Min(0.01f)] private float whiteDuration = 0.08f;
    [SerializeField, Min(0f)] private float normalDuration = 0.06f;

    private Material[] originalMaterials;
    private Coroutine flashCoroutine;

    private int previousHP;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Awake()
    {
        if (enemyHealth == null)
        {
            enemyHealth =
                GetComponentInChildren<EnemyHealth>(true);
        }

        // Tự tìm SpriteRenderer nếu chưa gán.
        if (enemySprites == null || enemySprites.Length == 0)
        {
            enemySprites =
                GetComponentsInChildren<SpriteRenderer>(true);
        }

        originalMaterials = new Material[enemySprites.Length];

        // Lưu Material gốc.
        for (int i = 0; i < enemySprites.Length; i++)
        {
            if (enemySprites[i] != null)
            {
                originalMaterials[i] =
                    enemySprites[i].sharedMaterial;
            }
        }
    }

    private void OnEnable()
    {
        if (enemyHealth == null)
            return;

        previousHP = enemyHealth.CurrentHP;

        // Theo dõi thay đổi HP của Enemy.
        enemyHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= HandleHealthChanged;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        RestoreOriginalMaterials();
    }

    // =====================================
    // DETECT DAMAGE
    // =====================================

    private void HandleHealthChanged(EnemyHealth enemy)
    {
        if (enemy != enemyHealth)
            return;

        int currentHP = enemy.CurrentHP;

        // Chỉ Flash khi HP giảm.
        if (currentHP < previousHP)
        {
            PlayHurtFlash();

            Debug.Log(
                $"Enemy Hurt Flash: {enemy.name} | " +
                $"HP: {previousHP} -> {currentHP}"
            );
        }

        previousHP = currentHP;
    }

    // =====================================
    // PLAY HURT FLASH
    // =====================================

    public void PlayHurtFlash()
    {
        if (!isActiveAndEnabled)
            return;

        if (whiteFlashMaterial == null)
        {
            Debug.LogWarning(
                "EnemyHurtFlash: Chưa gán White Flash Material!"
            );
            return;
        }

        // Nếu bị đánh liên tục, chạy Flash lại từ đầu.
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        RestoreOriginalMaterials();

        flashCoroutine = StartCoroutine(
            FlashSequence()
        );
    }

    private IEnumerator FlashSequence()
    {
        for (int i = 0; i < flashCount; i++)
        {
            // 1. Chuyển Enemy sang trắng.
            SetWhiteMaterial();

            yield return new WaitForSeconds(
                whiteDuration
            );

            // 2. Trở về màu bình thường.
            RestoreOriginalMaterials();

            // 3. Đợi trước khi Flash tiếp.
            if (i < flashCount - 1 && normalDuration > 0f)
            {
                yield return new WaitForSeconds(
                    normalDuration
                );
            }
        }

        RestoreOriginalMaterials();
        flashCoroutine = null;
    }

    // =====================================
    // WHITE MATERIAL
    // =====================================

    private void SetWhiteMaterial()
    {
        foreach (SpriteRenderer sr in enemySprites)
        {
            if (sr != null)
            {
                sr.sharedMaterial = whiteFlashMaterial;
            }
        }
    }

    // =====================================
    // RESTORE MATERIAL
    // =====================================

    private void RestoreOriginalMaterials()
    {
        if (enemySprites == null ||
            originalMaterials == null)
            return;

        for (int i = 0; i < enemySprites.Length; i++)
        {
            if (enemySprites[i] != null)
            {
                enemySprites[i].sharedMaterial =
                    originalMaterials[i];
            }
        }
    }

    // =====================================
    // TEST IN INSPECTOR
    // =====================================

    [ContextMenu("Test Enemy Hurt Flash")]
    public void TestHurtFlash()
    {
        PlayHurtFlash();
    }
}
