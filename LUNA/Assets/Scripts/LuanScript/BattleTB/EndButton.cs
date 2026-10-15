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

    [Header("Battle FSM")]
    [SerializeField] private BattleStateMachine battleStateMachine;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private bool isResolvingTurn = false;

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
        if (battleStateMachine == null)
        {
            Debug.LogError("EndButton: Chưa gán BattleStateMachine!");
            return;
        }

        // Chỉ cho phép kết thúc lượt trong PlayerTurn.
        if (!battleStateMachine.IsPlayerTurn())
        {
            Debug.Log("EndButton: Hiện không thể kết thúc lượt.");
            return;
        }

        if (isResolvingTurn)
            return;

        isResolvingTurn = true;

        // Bắt đầu lượt địch.
        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.EnemyTurn
        );

        Debug.Log("========== ENEMY TURN ==========");

        if (enemyAttack == null)
        {
            Debug.LogWarning("EndButton: Chưa gán EnemyAttack.");
            FinishTurn();
            return;
        }

        // Địch chọn Attack hoặc Shield.
        bool isAttack = enemyAttack.RandomAction();

        if (!isAttack)
        {
            Debug.Log("Enemy chọn SHIELD.");
            FinishTurn();
            return;
        }

        Debug.Log("Enemy chọn ATTACK.");

        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= timingChance && timingManager != null)
        {
            Debug.Log("Enemy Attack: Timing xuất hiện!");
            timingManager.StartTiming();
        }
        else
        {
            Debug.Log("Enemy Attack: Không có Timing.");

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
        // Bỏ qua kết quả Timing nếu không còn xử lý lượt địch.
        if (!isResolvingTurn ||
            battleStateMachine == null ||
            battleStateMachine.CurrentState !=
            BattleStateMachine.BattleState.EnemyTurn)
        {
            return;
        }

        Debug.Log(
            "Timing Result = " + result +
            " | Multiplier = " + multiplier
        );

        if (enemyAttack != null)
        {
            enemyAttack.PlayAttackAnimation();
            enemyAttack.ApplyTimingDamage(multiplier);
        }

        FinishTurn();
    }

    public void FinishTurn()
    {
        if (!isResolvingTurn)
            return;

        // Dừng Timing trước khi quay lại lượt người chơi.
        if (timingManager != null)
        {
            timingManager.StopTiming();
        }

        // Nếu trận đấu đã kết thúc, không reset lượt.
        if (battleStateMachine == null ||
            battleStateMachine.IsBattleFinished())
        {
            isResolvingTurn = false;
            return;
        }

        // Tạm giữ hành vi reset bài hiện tại của dự án.
        if (deckManager != null)
        {
            deckManager.InitializeDeck();
        }

        if (turnManager != null)
        {
            turnManager.StartTurn();
        }

        isResolvingTurn = false;

        battleStateMachine.ChangeState(
            BattleStateMachine.BattleState.PlayerTurn
        );

        Debug.Log("========== PLAYER TURN ==========");
    }
}