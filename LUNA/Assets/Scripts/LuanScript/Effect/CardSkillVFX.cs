using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkillVFX : MonoBehaviour
{
    public enum EffectTarget { Player, Enemy }

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
        [Min(0.1f)] public float lifetime = 1.5f;
        [Header("Repeat - Double Strike")]
        [Min(1)] public int repeatCount = 1;
        [Min(0f)] public float repeatDelay = 0.2f;

        [Header("Follow Target")]
        public bool followTarget = true;
        public bool particlesFollowTarget = true;
        [Header("Visual")]
        public Vector3 scaleMultiplier = Vector3.one;
        public bool overrideSorting = false;
        public string sortingLayerName = "Default";
        public int sortingOrder = 10;
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemySpawner enemySpawner;
    [Header("Optional VFX Anchors")]
    [SerializeField] private Transform playerVFXAnchor;
    [SerializeField] private Transform enemyVFXAnchor;
    [Header("Card Effects")]
    [SerializeField] private List<CardVFXData> cardEffects = new List<CardVFXData>();

    public void PlayCardVFX(CardData card)
    {
        if (card == null) return;
        CardVFXData effect = FindEffect(card);
        if (effect == null)
        {
            Debug.LogWarning("CardSkillVFX: Chưa cấu hình VFX cho " + card.cardName, this);
            return;
        }
        if (effect.vfxPrefab == null)
        {
            Debug.LogWarning("CardSkillVFX: Chưa gán VFX Prefab cho " + card.cardName, this);
            return;
        }
        Transform target = GetTarget(effect.target);
        if (target == null)
        {
            Debug.LogWarning("CardSkillVFX: Không tìm thấy mục tiêu VFX cho " + card.cardName, this);
            return;
        }
        StartCoroutine(PlayEffectSequence(effect, target));
    }

    private CardVFXData FindEffect(CardData card)
    {
        if (cardEffects == null) return null;
        foreach (CardVFXData effect in cardEffects)
        {
            if (effect == null || effect.card == null) continue;
            if (effect.card == card) return effect;
            if (!string.IsNullOrEmpty(card.cardID) && effect.card.cardID == card.cardID)
                return effect;
        }
        return null;
    }

    private Transform GetTarget(EffectTarget target)
    {
        if (target == EffectTarget.Player)
            return playerVFXAnchor != null ? playerVFXAnchor : player;
        if (enemySpawner == null) return null;
        EnemyHealth enemy = enemySpawner.GetCurrentEnemyHealth();
        if (enemy == null || enemy.IsDead) return null;
        if (enemyVFXAnchor != null && enemyVFXAnchor.IsChildOf(enemy.transform))
            return enemyVFXAnchor;
        return enemy.transform;
    }

    private IEnumerator PlayEffectSequence(CardVFXData effect, Transform target)
    {
        int count = Mathf.Max(1, effect.repeatCount);
        for (int i = 0; i < count; i++)
        {
            if (target == null) yield break;
            SpawnVFX(effect, target);
            if (i < count - 1 && effect.repeatDelay > 0f)
                yield return new WaitForSeconds(effect.repeatDelay);
        }
    }

    private void SpawnVFX(CardVFXData effect, Transform target)
    {
        if (effect.vfxPrefab == null || target == null) return;
        GameObject instance;
        if (effect.followTarget)
        {
            instance = Instantiate(effect.vfxPrefab, target);
            instance.transform.localPosition = effect.offset;
            instance.transform.localRotation = effect.vfxPrefab.transform.localRotation;
            instance.transform.localScale = Vector3.Scale(effect.vfxPrefab.transform.localScale, effect.scaleMultiplier);
        }
        else
        {
            instance = Instantiate(effect.vfxPrefab,
                target.position + effect.offset,
                effect.vfxPrefab.transform.rotation);
            instance.transform.localScale = Vector3.Scale(effect.vfxPrefab.transform.localScale, effect.scaleMultiplier);
        }

        instance.name = effect.vfxPrefab.name + "_Runtime";
        instance.SetActive(true);

        ParticleSystem[] particles = instance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particle in particles)
        {
            if (particle == null) continue;
            particle.gameObject.SetActive(true);
            if (effect.followTarget && effect.particlesFollowTarget)
            {
                var main = particle.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }
            particle.Play(true);
        }

        if (effect.overrideSorting)
        {
            foreach (ParticleSystemRenderer renderer in instance.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                renderer.sortingLayerName = effect.sortingLayerName;
                renderer.sortingOrder = effect.sortingOrder;
            }
            foreach (SpriteRenderer renderer in instance.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.sortingLayerName = effect.sortingLayerName;
                renderer.sortingOrder = effect.sortingOrder;
            }
        }
        Destroy(instance, Mathf.Max(0.1f, effect.lifetime));
        Debug.Log("CardSkillVFX: Spawn " + effect.vfxPrefab.name, this);
    }

    [ContextMenu("Test First Card VFX")]
    private void TestFirstCardVFX()
    {
        if (cardEffects == null || cardEffects.Count == 0 || cardEffects[0] == null || cardEffects[0].card == null)
        {
            Debug.LogWarning("CardSkillVFX: Chưa cấu hình Card đầu tiên!", this);
            return;
        }
        PlayCardVFX(cardEffects[0].card);
    }
}
