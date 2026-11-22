
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

    // =====================================
    // ANIMATION EVENT - HIEN EFFECT KHI BAN
    // =====================================

    public void OnShootEffect()
    {
        if (effectPrefab == null)
            return;

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

        if (effectLifetime > 0f)
            Destroy(effect, effectLifetime);

        Debug.Log("[NALA] Shoot Effect!");
    }

    // =====================================
    // ANIMATION EVENT - GAY DAMAGE
    // =====================================

    public void OnUltimateEffect()
    {
        if (ultimateController != null)
            ultimateController.OnUltimateEffect();
    }

    // =====================================
    // ANIMATION EVENT - KET THUC
    // =====================================

    public void OnUltimateFinished()
    {
        if (ultimateController != null)
            ultimateController.OnUltimateFinished();
    }
}