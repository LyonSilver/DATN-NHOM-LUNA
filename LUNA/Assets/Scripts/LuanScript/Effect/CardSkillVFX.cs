
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkillVFX : MonoBehaviour
{
    public enum EffectTarget
    {
        Player,
        Enemy
    }

    [Serializable]
    public class CardVFXData
    {
        [Header("Card")]
        public CardData card;

        [Header("VFX Prefab")]
        public GameObject vfxPrefab;

        [Header("Spawn Target")]
        public EffectTarget target;

        [Header("Position")]
        public Vector3 offset = new Vector3(0f, 1f, 0f);

        [Header("Duration")]
        [Min(0.1f)]
        public float lifetime = 1.5f;

        [Header("Repeat - Double Strike")]
        [Min(1)]
        public int repeatCount = 1;

        [Min(0f)]
        public float repeatDelay = 0.2f;
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Card Effects")]
    [SerializeField]
    private List<CardVFXData> cardEffects =
        new List<CardVFXData>();

    // =====================================
    // PLAY CARD EFFECT
    // =====================================

    public void PlayCardVFX(CardData card)
    {
        if (card == null)
            return;

        CardVFXData effect = FindEffect(card);

        if (effect == null)
        {
            Debug.LogWarning(
                "CardSkillVFX: Chưa cấu hình VFX cho " +
                card.cardName
            );
            return;
        }

        if (effect.vfxPrefab == null)
        {
            Debug.LogWarning(
                "CardSkillVFX: Chưa gán VFX Prefab cho " +
                card.cardName
            );
            return;
        }

        Transform target = GetTarget(effect.target);

        if (target == null)
        {
            Debug.LogWarning(
                "CardSkillVFX: Không tìm thấy mục tiêu VFX!"
            );
            return;
        }

        // Ghi lại vị trí ngay khi Card tác động.
        Vector3 spawnPosition =
            target.position + effect.offset;

        StartCoroutine(
            PlayEffectSequence(effect, spawnPosition)
        );
    }

    // =====================================
    // FIND CARD VFX
    // =====================================

    private CardVFXData FindEffect(CardData card)
    {
        foreach (CardVFXData effect in cardEffects)
        {
            if (effect == null || effect.card == null)
                continue;

            // So sánh ScriptableObject CardData.
            if (effect.card == card)
                return effect;

            // Hỗ trợ trường hợp CardData được clone.
            if (!string.IsNullOrEmpty(card.cardID) &&
                effect.card.cardID == card.cardID)
            {
                return effect;
            }
        }

        return null;
    }

    // =====================================
    // GET PLAYER / ENEMY TARGET
    // =====================================

    private Transform GetTarget(EffectTarget target)
    {
        if (target == EffectTarget.Player)
        {
            return player;
        }

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
        CardVFXData effect,
        Vector3 spawnPosition)
    {
        int count = Mathf.Max(
            1,
            effect.repeatCount
        );

        for (int i = 0; i < count; i++)
        {
            SpawnVFX(effect, spawnPosition);

            if (i < count - 1 &&
                effect.repeatDelay > 0f)
            {
                yield return new WaitForSeconds(
                    effect.repeatDelay
                );
            }
        }
    }

    // =====================================
    // SPAWN VFX
    // =====================================

    private void SpawnVFX(
        CardVFXData effect,
        Vector3 spawnPosition)
    {
        GameObject instance = Instantiate(
            effect.vfxPrefab,
            spawnPosition,
            effect.vfxPrefab.transform.rotation
        );

        instance.name =
            effect.vfxPrefab.name + "_Runtime";

        instance.SetActive(true);

        // Nếu là ParticleSystem, phát hiệu ứng.
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

        // Nếu là Prefab Animator / Sprite Animation,
        // Animator sẽ tự chạy khi GameObject Active.

        Destroy(
            instance,
            Mathf.Max(0.1f, effect.lifetime)
        );

        Debug.Log(
            "CardSkillVFX: Spawn " +
            effect.vfxPrefab.name
        );
    }

    // =====================================
    // TEST IN UNITY
    // =====================================

    [ContextMenu("Test First Card VFX")]
    private void TestFirstCardVFX()
    {
        if (cardEffects.Count == 0)
        {
            Debug.LogWarning(
                "CardSkillVFX: Danh sách Card Effects đang trống!"
            );
            return;
        }

        if (cardEffects[0].card != null)
        {
            PlayCardVFX(cardEffects[0].card);
        }
    }
}
