
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

    [Header("Turn")]
    [SerializeField] private BattleStateMachine battleStateMachine;

    [Header("Arrow Rain")]
    [SerializeField] private NalaArrowRain arrowRain;

    [Header("Settings")]
    [SerializeField] private bool hideNalaAfterUltimate = true;

    private bool isUsingUltimate = false;
    private bool animationFinished = false;
    private bool effectTriggered = false;

    public bool IsUsingUltimate => isUsingUltimate;

    // =====================================
    // START
    // =====================================

    private void Start()
    {
        // Nala ở hậu tuyến khi bắt đầu trận.
        if (nala != null)
        {
            nala.SetActive(false);
        }
    }

    // =====================================
    // UPDATE
    // =====================================

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
            arrowRain == null)
        {
            Debug.LogWarning(
                "[NALA ULTIMATE] Thiếu Reference!"
            );
            return;
        }

        // Chỉ sử dụng trong Player Turn.
        if (!battleStateMachine.IsPlayerTurn() ||
            battleStateMachine.IsBattleFinished())
        {
            return;
        }

        // Kiểm tra Rage.
        if (!rageManager.IsRageFull)
        {
            Debug.Log(
                "[NALA ULTIMATE] Rage chưa đầy!"
            );
            return;
        }

        // Không kích hoạt khi mưa tên đang chạy.
        if (arrowRain.IsAttacking)
            return;

        // Tiêu hao toàn bộ Rage.
        if (!rageManager.UseRage())
            return;

        isUsingUltimate = true;
        animationFinished = false;
        effectTriggered = false;

        // Triệu hồi Nala.
        nala.SetActive(true);

        // Reset Trigger cũ.
        nalaAnimator.ResetTrigger(ultimateTrigger);

        // Chạy animation Ultimate.
        nalaAnimator.SetTrigger(ultimateTrigger);

        Debug.Log(
            "[NALA ULTIMATE] Nala xuất hiện và sử dụng Ultimate!"
        );
    }

    // =====================================
    // ANIMATION EVENT - ARROW RAIN
    // =====================================

    public void OnUltimateEffect()
    {
        if (!isUsingUltimate)
            return;

        // Tránh Animation Event gọi nhiều lần.
        if (effectTriggered)
            return;

        if (arrowRain == null)
            return;

        effectTriggered = true;

        Debug.Log(
            "[NALA ULTIMATE] Bắt đầu mưa tên!"
        );

        arrowRain.StartArrowRain();
    }

    // =====================================
    // ANIMATION EVENT - FINISH
    // =====================================

    public void OnUltimateFinished()
    {
        if (!isUsingUltimate)
            return;

        animationFinished = true;

        StartCoroutine(WaitForUltimateFinish());
    }

    // =====================================
    // WAIT FOR ARROW RAIN
    // =====================================

    private IEnumerator WaitForUltimateFinish()
    {
        // Đợi animation kết thúc.
        if (!animationFinished)
            yield break;

        // Nếu chưa kích hoạt hiệu ứng,
        // không được kết thúc Ultimate.
        if (!effectTriggered)
        {
            Debug.LogWarning(
                "[NALA ULTIMATE] Chưa gọi OnUltimateEffect!"
            );
            yield break;
        }

        // Đợi mưa tên gây sát thương xong.
        while (arrowRain != null &&
               arrowRain.IsAttacking)
        {
            yield return null;
        }

        // =====================================
        // HIDE NALA
        // =====================================

        if (hideNalaAfterUltimate && nala != null)
        {
            nala.SetActive(false);
        }

        isUsingUltimate = false;
        animationFinished = false;
        effectTriggered = false;

        Debug.Log(
            "[NALA ULTIMATE] Hoàn thành! Nala trở về hậu tuyến."
        );
    }
}
