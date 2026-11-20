
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

    private bool isUsingUltimate = false;

    private void Start()
    {
        // Nala ở hậu tuyến khi bắt đầu trận
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

    public void ActivateUltimate()
    {
        if (isUsingUltimate)
            return;

        if (rageManager == null || nala == null ||
            nalaAnimator == null || battleStateMachine == null)
            return;

        // Chỉ dùng Ultimate trong Player Turn
        if (!battleStateMachine.IsPlayerTurn())
            return;

        if (!rageManager.IsRageFull)
            return;

        // Tiêu hao Rage
        if (!rageManager.UseRage())
            return;

        isUsingUltimate = true;

        // Triệu hồi Nala
        nala.SetActive(true);

        // Chạy animation Ultimate
        nalaAnimator.SetTrigger(ultimateTrigger);

        Debug.Log("Nala sử dụng Ultimate!");
    }

    // Animation Event: thời điểm Ultimate gây hiệu ứng
    public void OnUltimateEffect()
    {
        Debug.Log("Nala Ultimate gây hiệu ứng!");

        // Gọi hệ thống gây sát thương tại đây
    }

    // Animation Event: khi Nala hoàn thành Ultimate
    public void OnUltimateFinished()
    {
        isUsingUltimate = false;

        // Nala lui về hậu tuyến
        nala.SetActive(false);

        Debug.Log("Nala đã trở về hậu tuyến!");
    }
}
