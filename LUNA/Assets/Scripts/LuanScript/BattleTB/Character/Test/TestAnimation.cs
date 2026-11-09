using UnityEngine;
using UnityEngine.InputSystem;

public class TestAnimation : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation Names")]
    [SerializeField] private string attackAnimation = "Kirito_BasicAttack";
    [SerializeField] private string summonSwordAnimation = "Kirito_SummonSword";

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Nhấn E: Basic Attack
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            PlayAnimation(attackAnimation);
        }

        // Nhấn W: Summon Sword
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            PlayAnimation(summonSwordAnimation);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator == null)
        {
            Debug.LogWarning("Chưa gán Animator!");
            return;
        }

        // Kiểm tra State có tồn tại không
        int stateHash = Animator.StringToHash(
            "Base Layer." + animationName
        );

        if (!animator.HasState(0, stateHash))
        {
            Debug.LogWarning(
                "Không tìm thấy Animation State: " + animationName
            );
            return;
        }

        // Chạy animation từ đầu
        animator.Play(stateHash, 0, 0f);

        Debug.Log("Đang chạy animation: " + animationName);
    }
}