
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Managers")]
    [SerializeField] private CardEffectManager cardEffectManager;

    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = true;

    private GameObject currentEnemy;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemy(0);
        }
    }

    public void SpawnEnemy(int enemyIndex)
    {
        if (enemyIndex < 0 ||
            enemyIndex >= enemyPrefabs.Length)
        {
            Debug.LogError("Enemy Index không hợp lệ!");
            return;
        }

        if (spawnPoint == null ||
            enemyPrefabs[enemyIndex] == null ||
            cardEffectManager == null)
        {
            Debug.LogError("Thiếu tham chiếu EnemySpawner!");
            return;
        }

        // Xóa Enemy cũ.
        if (currentEnemy != null)
        {
            DespawnCurrentEnemy();
        }

        // Spawn Enemy mới tại vị trí cũ.
        currentEnemy = Instantiate(
            enemyPrefabs[enemyIndex],
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Lấy EnemyHealth của Enemy mới.
        EnemyHealth health =
    currentEnemy.GetComponentInChildren<EnemyHealth>(true);

        if (health == null)
        {
            Debug.LogError(
                "Enemy Prefab chưa có EnemyHealth!"
            );

            Destroy(currentEnemy);
            currentEnemy = null;
            cardEffectManager.SetCurrentEnemy(null);
            return;
        }

        EnemyAttack attack =
    currentEnemy.GetComponentInChildren<EnemyAttack>(true);

        if (attack != null)
        {
            attack.SetCardEffectManager(cardEffectManager);
        }
        else
        {
            Debug.LogWarning(
                "Enemy mới chưa có EnemyAttack!"
            );
        }

        // Kết nối Enemy mới với CardEffectManager.
        cardEffectManager.SetCurrentEnemy(health);

        Debug.Log(
            $"Spawn {currentEnemy.name} | " +
            $"HP: {health.CurrentHP}/{health.MaxHP}"
        );
    }

    public void DespawnCurrentEnemy()
    {
        // Ngắt liên kết với Enemy cũ.
        if (cardEffectManager != null)
        {
            cardEffectManager.SetCurrentEnemy(null);
        }

        // Xóa Enemy cũ.
        if (currentEnemy != null)
        {
            Destroy(currentEnemy);
            currentEnemy = null;
        }

        Debug.Log("EnemySpawner: Đã xóa Enemy cũ!");
    }

    public EnemyAttack GetCurrentEnemyAttack()
    {
        if (currentEnemy == null)
            return null;

        return currentEnemy.GetComponentInChildren<EnemyAttack>(true);
    }
}
