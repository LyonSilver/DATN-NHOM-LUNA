
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NalaUltimateController : MonoBehaviour
{
    [Header("Rage Manager")]
    [SerializeField] private RageManager rageManager;

    [Header("Nala NPC")]
    [SerializeField] private GameObject nala;
    [SerializeField] private Animator nalaAnimator;

    [Header("Animation")]
    [SerializeField] private string ultimateTrigger = "Ulti";

    [Header("Turn Systems")]
    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private OneCardTurnController oneCardTurnController;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private EndButton endButton;

    [Header("Enemy")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Arrow Rain")]
    [SerializeField] private NalaArrowRain arrowRain;

    private bool isUsingUltimate;
    private bool effectTriggered;
    private bool isFinishing;

    public bool IsUsingUltimate => isUsingUltimate;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Start()
    {
        if (nala != null)
            nala.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            ActivateUltimate();
        }
    }

    // =====================================
    // ACTIVATE ULTIMATE
    // =====================================

    public void ActivateUltimate()
    {
        if (isUsingUltimate)
            return;

        if (Time.timeScale <= 0f)
            return;

        if (rageManager == null ||
            nala == null ||
            nalaAnimator == null ||
            battleStateMachine == null ||
            oneCardTurnController == null ||
            cardManager == null ||
            endButton == null ||
            enemySpawner == null ||
            arrowRain == null)
        {
            Debug.LogError(
                "[NALA] Thieu Reference!"
            );

            return;
        }

        // Chi su dung trong Player Turn.
        if (!battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
            return;

        // Khong su dung khi dang chuyen luot.
        if (endButton.IsResolvingTurn)
            return;

        // Kiem tra gioi han hanh dong.
        if (!oneCardTurnController.CanUseUltimate())
            return;

        // Kiem tra Card va Cinematic.
        if (!cardManager.CanStartUltimate())
            return;

        if (arrowRain.IsAttacking)
            return;

        // Kiem tra Enemy.
        EnemyHealth enemy =
            enemySpawner.GetCurrentEnemyHealth();

        if (enemy == null || enemy.IsDead)
            return;

        // Kiem tra Rage.
        if (!rageManager.IsRageFull)
            return;

        // Tieu hao Rage.
        if (!rageManager.UseRage())
            return;

        // Khoa Ultimate.
        isUsingUltimate = true;
        effectTriggered = false;
        isFinishing = false;

        // Danh dau Player da su dung hanh dong.
        oneCardTurnController.NotifyUltimateStarted();

        // Trieu hoi Nala.
        nala.SetActive(true);

        // Chay Animation Ultimate.
        nalaAnimator.ResetTrigger(ultimateTrigger);
        nalaAnimator.SetTrigger(ultimateTrigger);

        Debug.Log(
            "[NALA] Bat dau Ultimate!"
        );
    }

    // =====================================
    // ANIMATION EVENT - DAMAGE
    // =====================================

    public void OnUltimateEffect()
    {
        if (!isUsingUltimate || effectTriggered)
            return;

        effectTriggered = true;

        if (arrowRain != null)
        {
            arrowRain.StartArrowRain();
        }

        Debug.Log(
            "[NALA] Bat dau mua ten!"
        );
    }

    // =====================================
    // ANIMATION EVENT - FINISHED
    // =====================================

    public void OnUltimateFinished()
    {
        if (!isUsingUltimate || isFinishing)
            return;

        isFinishing = true;

        Debug.Log(
            "[NALA] Animation Ultimate ket thuc!"
        );

        StartCoroutine(
            FinishUltimateRoutine()
        );
    }

    // =====================================
    // FINISH ULTIMATE
    // =====================================

    private IEnumerator FinishUltimateRoutine()
    {
        if (!effectTriggered)
        {
            Debug.LogWarning(
                "[NALA] Thieu Event OnUltimateEffect!"
            );
        }

        // Cho mua ten gay het sat thuong.
        while (arrowRain != null &&
               arrowRain.IsAttacking)
        {
            yield return null;
        }

        // Nala tro ve hau tuyen.
        if (nala != null)
            nala.SetActive(false);

        // Cho he thong cap nhat.
        yield return null;

        // Reset trang thai Ultimate.
        effectTriggered = false;
        isFinishing = false;

        // Thong bao Ultimate da hoan thanh
        // thong qua trang thai IsUsingUltimate.
        isUsingUltimate = false;

        Debug.Log(
            "[NALA] Ultimate hoan thanh!"
        );

        // KHONG goi EndTurn tai day.
        // OneCardTurnController tu dong chuyen luot.
    }
}