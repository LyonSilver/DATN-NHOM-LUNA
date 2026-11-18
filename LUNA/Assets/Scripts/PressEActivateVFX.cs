
using UnityEngine;
using System.Collections;

public class PressEActivateVFX : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Duration")]
    [SerializeField] private float lifetime = 1.5f;

    [Header("Controls")]
    [SerializeField] private KeyCode activationKey = KeyCode.E;
    [SerializeField] private bool allowRepeat = true;

    private bool isPlaying;

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            if (!isPlaying)
                StartCoroutine(ActivateVFX());
        }
    }

    IEnumerator ActivateVFX()
    {
        if (vfxPrefab == null)
        {
            Debug.LogWarning("Chưa gắn VFX Prefab!");
            yield break;
        }

        isPlaying = true;

        Vector3 position = spawnPoint != null
            ? spawnPoint.position
            : transform.position;

        GameObject vfx = Instantiate(
            vfxPrefab,
            position,
            vfxPrefab.transform.rotation
        );

        vfx.SetActive(true);

        // Kích hoạt Particle System
        ParticleSystem[] particles =
            vfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        yield return new WaitForSeconds(
            Mathf.Max(0.1f, lifetime)
        );

        if (vfx != null)
            Destroy(vfx);

        isPlaying = false;

        if (!allowRepeat)
            enabled = false;
    }
}
