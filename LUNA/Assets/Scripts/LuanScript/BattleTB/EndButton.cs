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

        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= timingChance &&
            timingManager != null)
        {
            Debug.Log("End: Timing xuất hiện!");

            timingManager.StartTiming();
        }
        else
        {
            Debug.Log("End: Không có Timing.");

            if (enemyAttack != null)
            {
                enemyAttack.AttackWithoutTiming();

                // Enemy đánh xong → kết thúc lượt
                FinishTurn();
            }
            else
            {
                Debug.LogWarning("EndButton: Chưa gán EnemyAttack.");

                FinishTurn();
            }
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
            enemyAttack.ApplyTimingDamage(multiplier);

            // Enemy đánh xong → kết thúc lượt
            FinishTurn();
        }
        else
        {
            Debug.LogWarning("EndButton: Chưa gán EnemyAttack.");

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