
using System.Collections;
using UnityEngine;

public class CardSkillVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemySpawner enemySpawner;

    // =====================================
    // PLAY CARD VFX
    // =====================================

    public void PlayCardVFX(CardData card)
    {
        if (card == null || card.vfxPrefab == null)
            return;

        Transform target = GetTarget(card.vfxTarget);

        if (target == null)
        {
            Debug.LogWarning(
                "CardSkillVFX: Không tìm thấy mục tiêu!"
            );
            return;
        }

        Vector3 spawnPosition =
            target.position + card.vfxOffset;

        StartCoroutine(
            PlayEffectSequence(card, spawnPosition)
        );
    }

    // =====================================
    // GET TARGET
    // =====================================

    private Transform GetTarget(CardData.VFXTarget target)
    {
        if (target == CardData.VFXTarget.Player)
            return player;

        if (enemySpawner == null)
            return null;

        EnemyHealth enemy =
            enemySpawner.GetCurrentEnemyHealth();

        if (enemy == null || enemy.IsDead)
            return null;

        return enemy.transform;
    }

    // =====================================
    // PLAY EFFECT SEQUENCE
    // =====================================

    private IEnumerator PlayEffectSequence(
        CardData card,
        Vector3 spawnPosition)
    {
        int count = Mathf.Max(1, card.vfxRepeatCount);

        for (int i = 0; i < count; i++)
        {
            SpawnVFX(card, spawnPosition);

            if (i < count - 1 &&
                card.vfxRepeatDelay > 0f)
            {
                yield return new WaitForSeconds(
                    card.vfxRepeatDelay
                );
            }
        }
    }

    // =====================================
    // SPAWN VFX
    // =====================================

    private void SpawnVFX(
        CardData card,
        Vector3 spawnPosition)
    {
        GameObject instance = Instantiate(
            card.vfxPrefab,
            spawnPosition,
            card.vfxPrefab.transform.rotation
        );

        instance.name =
            card.vfxPrefab.name + "_Runtime";

        instance.SetActive(true);

        ParticleSystem[] particles =
            instance.GetComponentsInChildren<ParticleSystem>(
                true
            );

        foreach (ParticleSystem particle in particles)
        {
            if (particle == null)
                continue;

            particle.gameObject.SetActive(true);
            particle.Play();
        }

        Destroy(
            instance,
            Mathf.Max(0.1f, card.vfxLifetime)
        );
    }
}