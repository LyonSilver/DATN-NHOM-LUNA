
using UnityEngine;

public class NalaAnimationEvents : MonoBehaviour
{
    [Header("Ultimate Controller")]
    [SerializeField]
    private NalaUltimateController ultimateController;

    [Header("Ultimate VFX")]
    [SerializeField] private GameObject effectPrefab;

    [SerializeField] private Transform effectSpawnPoint;

    [Header("Effect Settings")]
    [SerializeField] private float effectLifetime = 3f;

    [SerializeField] private Vector3 effectOffset = Vector3.zero;

    private bool effectSpawned = false;

    // =====================================
    // ULTIMATE EFFECT
    // =====================================

    public void OnUltimateEffect()
    {
        // Tránh tạo VFX nhiều lần trong một lần gọi.
        if (effectSpawned)
            return;

        effectSpawned = true;

        // Kích hoạt sát thương mưa tên.
        if (ultimateController != null)
        {
            ultimateController.OnUltimateEffect();
        }

        // Tạo VFX.
        if (effectPrefab != null)
        {
            Vector3 spawnPosition =
                effectSpawnPoint != null
                ? effectSpawnPoint.position
                : transform.position;

            spawnPosition += effectOffset;

            GameObject effect = Instantiate(
                effectPrefab,
                spawnPosition,
                Quaternion.identity
            );

            // Tự hủy VFX.
            if (effectLifetime > 0f)
            {
                Destroy(effect, effectLifetime);
            }

            Debug.Log(
                "[NALA] Ultimate VFX Spawned!"
            );
        }
    }

    // =====================================
    // ULTIMATE FINISHED
    // =====================================

    public void OnUltimateFinished()
    {
        if (ultimateController != null)
        {
            ultimateController.OnUltimateFinished();
        }

        // Reset để lần Ultimate sau
        // có thể tạo VFX tiếp.
        effectSpawned = false;
    }
}
