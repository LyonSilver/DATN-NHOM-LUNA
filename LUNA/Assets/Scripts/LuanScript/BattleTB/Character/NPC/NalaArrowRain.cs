
using System.Collections;
using UnityEngine;

public class NalaArrowRain : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Arrow Rain Damage")]
    [SerializeField] private int hitCount = 8;
    [SerializeField] private int damagePerHit = 5;
    [SerializeField] private float hitInterval = 0.2f;

    [Header("Timing")]
    [SerializeField] private float rainDelay = 0.5f;

    private bool isAttacking;

    public bool IsAttacking => isAttacking;

    public void StartArrowRain()
    {
        if (isAttacking)
            return;

        StartCoroutine(ArrowRainRoutine());
    }

    private IEnumerator ArrowRainRoutine()
    {
        isAttacking = true;

        // Chờ mũi tên bay lên trời.
        yield return new WaitForSeconds(rainDelay);

        for (int i = 0; i < hitCount; i++)
        {
            if (enemySpawner == null)
                break;

            EnemyHealth enemy =
                enemySpawner.GetCurrentEnemyHealth();

            if (enemy == null || enemy.IsDead)
                break;

            // Gây damage cho Enemy.
            // Thay TakeDamage nếu EnemyHealth
            // đang dùng tên hàm khác.
            enemy.TakeDamage(damagePerHit);

            Debug.Log(
                "[NALA ARROW RAIN] Hit " +
                (i + 1) + "/" + hitCount +
                " | Damage: " + damagePerHit
            );

            yield return new WaitForSeconds(hitInterval);
        }

        isAttacking = false;
    }
}