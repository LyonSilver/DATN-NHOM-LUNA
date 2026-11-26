
using System.Collections;
using UnityEngine;

public class CardSkillVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private CardEffectManager cardEffectManager;

    private GameObject activeShieldVFX;
    private CardData activeShieldCard;

    // =====================================
    // UPDATE SHIELD VFX
    // =====================================

    private void LateUpdate()
    {
        if (activeShieldVFX == null)
            return;

        if (cardEffectManager == null ||
            player == null ||
            activeShieldCard == null)
        {
            RemoveShieldVFX();
            return;
        }

        // Hủy VFX khi hết Shield hoặc Player chết.
        if (cardEffectManager.GetPlayerShield() <= 0 ||
            cardEffectManager.IsPlayerDefeated)
        {
            RemoveShieldVFX();
            return;
        }

        // VFX di chuyển theo Player.
        activeShieldVFX.transform.position =
            player.position + activeShieldCard.vfxOffset;

        // Không bị ảnh hưởng bởi Scale của Player.
        activeShieldVFX.transform.localScale =
            activeShieldCard.vfxScale;
    }

    // =====================================
    // PLAY CARD VFX
    // =====================================

    public void PlayCardVFX(CardData card)
    {
        if (card == null || card.vfxPrefab == null)
            return;

        // Shield VFX tồn tại đến khi hết giáp.
        if (card.vfxDurationType ==
            CardData.VFXDurationType.UntilShieldBroken)
        {
            PlayShieldVFX(card);
            return;
        }

        Transform target = GetTarget(card.vfxTarget);

        if (target == null)
            return;

        Vector3 spawnPosition =
            target.position + card.vfxOffset;

        StartCoroutine(
            PlayEffectSequence(card, spawnPosition)
        );
    }

    // =====================================
    // SHIELD VFX
    // =====================================

    private void PlayShieldVFX(CardData card)
    {
        if (player == null || cardEffectManager == null)
            return;

        if (cardEffectManager.GetPlayerShield() <= 0)
            return;

        // Không tạo thêm VFX nếu Shield vẫn tồn tại.
        if (activeShieldVFX != null)
            return;

        activeShieldCard = card;

        // Không đặt Player làm Parent.
        // Tránh kế thừa Scale của Player.
        activeShieldVFX = Instantiate(
            card.vfxPrefab,
            player.position + card.vfxOffset,
            card.vfxPrefab.transform.rotation
        );

        activeShieldVFX.name = "ShieldVFX_Runtime";

        // Áp dụng Scale từ CardData.
        activeShieldVFX.transform.localScale =
            card.vfxScale;

        activeShieldVFX.SetActive(true);

        PlayParticles(activeShieldVFX, true);

        // Không Destroy theo thời gian.
        // LateUpdate sẽ kiểm tra chỉ số Shield.
    }

    // =====================================
    // REMOVE SHIELD VFX
    // =====================================

    public void RemoveShieldVFX()
    {
        if (activeShieldVFX != null)
        {
            activeShieldVFX.SetActive(false);

            Destroy(activeShieldVFX);
        }

        activeShieldVFX = null;
        activeShieldCard = null;
    }

    // =====================================
    // GET TARGET
    // =====================================

    private Transform GetTarget(
        CardData.VFXTarget target)
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
    // PLAY NORMAL EFFECT
    // =====================================

    private IEnumerator PlayEffectSequence(
        CardData card,
        Vector3 spawnPosition)
    {
        int count = Mathf.Max(
            1,
            card.vfxRepeatCount
        );

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
    // SPAWN NORMAL VFX
    // =====================================

    private void SpawnVFX(
        CardData card,
        Vector3 spawnPosition)
    {
        if (card == null || card.vfxPrefab == null)
            return;

        GameObject instance = Instantiate(
            card.vfxPrefab,
            spawnPosition,
            card.vfxPrefab.transform.rotation
        );

        instance.name =
            card.vfxPrefab.name + "_Runtime";

        // Áp dụng Scale riêng của từng Card.
        instance.transform.localScale =
            card.vfxScale;

        instance.SetActive(true);

        PlayParticles(instance, false);

        Destroy(
            instance,
            Mathf.Max(0.1f, card.vfxLifetime)
        );
    }

    // =====================================
    // PLAY PARTICLE SYSTEM
    // =====================================

    private void PlayParticles(
        GameObject instance,
        bool loop)
    {
        ParticleSystem[] particles =
            instance.GetComponentsInChildren
                <ParticleSystem>(true);

        foreach (ParticleSystem particle in particles)
        {
            if (particle == null)
                continue;

            particle.gameObject.SetActive(true);

            if (loop)
            {
                var main = particle.main;

                main.loop = true;

                main.stopAction =
                    ParticleSystemStopAction.None;
            }

            particle.Play();
        }
    }

    // =====================================
    // CLEANUP
    // =====================================

    private void OnDisable()
    {
        StopAllCoroutines();

        RemoveShieldVFX();
    }
}