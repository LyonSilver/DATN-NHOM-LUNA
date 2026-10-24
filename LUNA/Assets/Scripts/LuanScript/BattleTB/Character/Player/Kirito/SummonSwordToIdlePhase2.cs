using UnityEngine;

public class SummonSwordToIdlePhase2 : StateMachineBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 1.5f;

    [SerializeField] private string idleStateName =
        "Base Layer.Kirito_Idle_Phase2";

    private float timer;
    private bool hasTransitioned;

    // Khi bắt đầu SummonSword
    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        timer = 0f;
        hasTransitioned = false;

        Debug.Log("Bắt đầu SummonSword!");
    }

    // Đếm thời gian trong SummonSword
    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        if (hasTransitioned)
            return;

        timer += Time.deltaTime;

        if (timer >= transitionDelay)
        {
            int stateHash = Animator.StringToHash(idleStateName);

            if (!animator.HasState(layerIndex, stateHash))
            {
                Debug.LogWarning(
                    "Không tìm thấy State: " + idleStateName
                );

                hasTransitioned = true;
                return;
            }

            hasTransitioned = true;

            // Chuyển sang Idle Phase 2
            animator.Play(stateHash, layerIndex, 0f);

            Debug.Log("Chuyển sang Idle Phase 2!");
        }
    }
}