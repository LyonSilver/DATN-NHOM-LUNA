using UnityEngine;

public class EndButton : MonoBehaviour
{
    [Header("Enemy Attack")]
    [SerializeField] private EnemyAttack enemyAttack;

    [Header("Timing")]
    [SerializeField] private TimingManager timingManager;

    [Range(0f, 100f)]
    [SerializeField] private float timingChance = 100f;

    [Header("Card")]
    [SerializeField] private DeckManager deckManager;

    [Header("Turn")]
    [SerializeField] private TurnManager turnManager;

    [Header("Animator")]
    [SerializeField] private Animator animator;


    private void Start()
    {
        if (timingManager != null)
        {
            timingManager.OnTimingResult += HandleTimingResult;
            timingManager.StopTiming();
        }
    }


    private void OnDestroy()
    {
        if (timingManager != null)
        {
            timingManager.OnTimingResult -= HandleTimingResult;
        }
    }


    public void EndTurn()
    {
        Debug.Log("========== END TURN ==========");

        if (enemyAttack == null)
        {
            Debug.LogWarning("EndButton: Chưa gán EnemyAttack.");
            FinishTurn();
            return;
        }

        // Random Enemy Action
        bool isAttack = enemyAttack.RandomAction();

        // =========================
        // ENEMY SHIELD
        // =========================
        if (!isAttack)
        {
            Debug.Log("Enemy chọn SHIELD.");

            FinishTurn();
            return;
        }

        // =========================
        // ENEMY ATTACK
        // =========================

        Debug.Log("Enemy chọn ATTACK.");

        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= timingChance &&
            timingManager != null)
        {
            Debug.Log(
                "Enemy Attack: Timing xuất hiện!"
            );

            timingManager.StartTiming();
        }
        else
        {
            Debug.Log(
                "Enemy Attack: Không có Timing."
            );

            enemyAttack.PlayAttackAnimation();
            enemyAttack.AttackWithoutTiming();

            FinishTurn();
        }
    }

    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }


    private void HandleTimingResult(
     TimingManager.TimingResult result,
     float multiplier)
    {
        Debug.Log(
            "End: Timing Result = " +
            result +
            " | Multiplier = " +
            multiplier
        );

        if (enemyAttack != null)
        {
            enemyAttack.PlayAttackAnimation();
            enemyAttack.ApplyTimingDamage(multiplier);
            FinishTurn();
        }
        else
        {
            Debug.LogWarning(
                "EndButton: Chưa gán EnemyAttack."
            );

            FinishTurn();
        }
    }


    public void FinishTurn()
    {
        if (deckManager != null)
        {
            deckManager.InitializeDeck();
        }

        if (turnManager != null)
        {
            turnManager.StartTurn();
        }

        Debug.Log("End: Đã bắt đầu lượt mới.");
    }
}