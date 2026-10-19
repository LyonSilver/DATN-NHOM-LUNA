
using System.Collections;
using UnityEngine;

public class PlayerCardCinematic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private CardEffectManager cardEffectManager;

    [Header("Camera")]
    [SerializeField] private Camera battleCamera;
    [SerializeField] private float zoomSize = 3f;
    [SerializeField] private float zoomDuration = 0.4f;
    [SerializeField] private float cameraFollowSpeed = 7f;
    [SerializeField] private Vector2 cameraFocusOffset = new Vector2(0f, 0.5f);

    [Header("UI")]
    [SerializeField] private CanvasGroup battleUI;

    [Header("Player Movement")]
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float returnDuration = 0.4f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Animation")]
    [SerializeField] private string runningParameter = "IsRunning";

    [SerializeField] private string healTrigger = "";
    [SerializeField] private string shieldTrigger = "";

    [Header("Action Duration")]
    [SerializeField] private float attackDuration = 0.8f;
    [SerializeField] private float buffDuration = 0.7f;

    private Vector3 originalPlayerPosition;
    private Vector3 originalCameraPosition;
    private float originalCameraSize;

    private float originalUIAlpha;
    private bool originalUIInteractable;
    private bool originalUIRaycasts;

    public bool IsPlaying { get; private set; }

    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        if (battleCamera == null)
            battleCamera = Camera.main;

        if (player != null && playerAnimator == null)
        {
            playerAnimator =
                player.GetComponentInChildren<Animator>(true);
        }
    }

    // =========================================
    // PLAY CARD
    // =========================================

    public bool PlayCard(CardData card)
    {
        if (IsPlaying || card == null)
            return false;

        if (player == null ||
            battleCamera == null ||
            cardEffectManager == null ||
            battleUI == null)
        {
            Debug.LogError(
                "PlayerCardCinematic: Thiếu Reference!"
            );

            return false;
        }

        if (!battleCamera.orthographic)
        {
            Debug.LogError(
                "PlayerCardCinematic: Camera phải là Orthographic!"
            );

            return false;
        }

        // Card Damage cần có Enemy.
        if (card.effectType == CardEffectType.Damage)
        {
            if (enemySpawner == null ||
                enemySpawner.GetCurrentEnemyHealth() == null)
            {
                Debug.LogWarning(
                    "PlayerCardCinematic: Không tìm thấy Enemy!"
                );

                return false;
            }

            if (enemySpawner.GetCurrentEnemyHealth().IsDead)
                return false;
        }

        IsPlaying = true;

        StartCoroutine(PlayCardSequence(card));

        return true;
    }

    // =========================================
    // MAIN SEQUENCE
    // =========================================

    private IEnumerator PlayCardSequence(CardData card)
    {
        // Lưu trạng thái trước Cinematic.
        originalPlayerPosition = player.position;
        originalCameraPosition = battleCamera.transform.position;
        originalCameraSize = battleCamera.orthographicSize;

        originalUIAlpha = battleUI.alpha;
        originalUIInteractable = battleUI.interactable;
        originalUIRaycasts = battleUI.blocksRaycasts;

        // =====================================
        // 1. HIDE UI
        // =====================================

        HideBattleUI();

        // =====================================
        // 2. CAMERA ZOOM IN
        // =====================================

        yield return ZoomCameraIn();

        // =====================================
        // 3. EXECUTE CARD
        // =====================================

        if (card.effectType == CardEffectType.Damage)
        {
            yield return ExecuteAttackCard(card);
        }
        else
        {
            yield return ExecuteBuffCard(card);
        }

        // =====================================
        // 4. CAMERA ZOOM OUT
        // =====================================

        yield return ZoomCameraOut();

        // =====================================
        // 5. RESTORE UI
        // =====================================

        RestoreBattleUI();

        IsPlaying = false;

        Debug.Log(
            "PlayerCardCinematic: Hoàn tất Card Cinematic!"
        );
    }

    // =========================================
    // ATTACK CARD
    // =========================================

    private IEnumerator ExecuteAttackCard(CardData card)
    {
        EnemyHealth enemy =
            enemySpawner.GetCurrentEnemyHealth();

        if (enemy == null || enemy.IsDead)
            yield break;

        // =====================================
        // RUN TO ENEMY
        // =====================================

        Vector3 targetPosition = originalPlayerPosition;

        float distanceX =
            enemy.transform.position.x -
            originalPlayerPosition.x;

        if (Mathf.Abs(distanceX) > stoppingDistance)
        {
            float direction = Mathf.Sign(distanceX);

            targetPosition.x =
                enemy.transform.position.x -
                direction * stoppingDistance;
        }

        // Giữ nguyên độ cao và chiều sâu Player.
        targetPosition.y = originalPlayerPosition.y;
        targetPosition.z = originalPlayerPosition.z;

        SetRunning(true);

        yield return MovePlayer(
            targetPosition,
            dashDuration
        );

        SetRunning(false);

        // =====================================
        // PLAY ATTACK AND APPLY DAMAGE
        // =====================================

        // CardEffectManager hiện tại đã gọi
        // PlayerAnimationController.PlayAttack()
        // khi xử lý Card Damage.
        //
        // Không gọi Attack lần thứ hai ở đây.
        cardEffectManager.ExecuteCardEffect(card);

        // Chờ Animation Attack.
        yield return new WaitForSeconds(
            Mathf.Max(0f, attackDuration)
        );

        // =====================================
        // RETURN TO START
        // =====================================

        SetRunning(true);

        yield return MovePlayer(
            originalPlayerPosition,
            returnDuration
        );

        SetRunning(false);
    }

    // =========================================
    // BUFF CARD
    // =========================================

    private IEnumerator ExecuteBuffCard(CardData card)
    {
        // Heal / Shield không chạy tới Enemy.
        // Nếu Animator có Trigger Buff,
        // chúng ta sẽ phát Animation tại chỗ.
        if (card.effectType == CardEffectType.Heal)
        {
            SetTriggerIfExists(healTrigger);
        }
        else if (card.effectType == CardEffectType.Shield)
        {
            SetTriggerIfExists(shieldTrigger);
        }

        // Áp dụng Card Effect một lần.
        cardEffectManager.ExecuteCardEffect(card);

        // Chờ hiệu ứng Buff.
        yield return new WaitForSeconds(
            Mathf.Max(0f, buffDuration)
        );
    }

    // =========================================
    // MOVE PLAYER
    // =========================================

    private IEnumerator MovePlayer(
        Vector3 target,
        float duration)
    {
        Vector3 start = player.position;

        duration = Mathf.Max(0f, duration);

        if (duration <= 0f)
        {
            player.position = target;
            FollowPlayerCamera();
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            // Làm chuyển động tự nhiên hơn.
            t = Mathf.SmoothStep(0f, 1f, t);

            player.position = Vector3.Lerp(
                start,
                target,
                t
            );

            FollowPlayerCamera();

            yield return null;
        }

        player.position = target;
    }

    // =========================================
    // CAMERA ZOOM IN
    // =========================================

    private IEnumerator ZoomCameraIn()
    {
        Vector3 startPosition =
            battleCamera.transform.position;

        float startSize =
            battleCamera.orthographicSize;

        Vector3 targetPosition =
            GetCameraFocusPosition();

        float duration = Mathf.Max(
            0.01f, zoomDuration
        );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            battleCamera.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            battleCamera.orthographicSize =
                Mathf.Lerp(
                    startSize,
                    zoomSize,
                    t
                );

            yield return null;
        }

        battleCamera.transform.position =
            targetPosition;

        battleCamera.orthographicSize = zoomSize;
    }

    // =========================================
    // CAMERA FOLLOW PLAYER
    // =========================================

    private void FollowPlayerCamera()
    {
        Vector3 targetPosition =
            GetCameraFocusPosition();

        float smooth = 1f - Mathf.Exp(
            -cameraFollowSpeed * Time.deltaTime
        );

        battleCamera.transform.position =
            Vector3.Lerp(
                battleCamera.transform.position,
                targetPosition,
                smooth
            );
    }

    private Vector3 GetCameraFocusPosition()
    {
        return new Vector3(
            player.position.x + cameraFocusOffset.x,
            player.position.y + cameraFocusOffset.y,
            originalCameraPosition.z
        );
    }

    // =========================================
    // CAMERA ZOOM OUT
    // =========================================

    private IEnumerator ZoomCameraOut()
    {
        Vector3 startPosition =
            battleCamera.transform.position;

        float startSize =
            battleCamera.orthographicSize;

        float duration = Mathf.Max(
            0.01f, zoomDuration
        );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            battleCamera.transform.position =
                Vector3.Lerp(
                    startPosition,
                    originalCameraPosition,
                    t
                );

            battleCamera.orthographicSize =
                Mathf.Lerp(
                    startSize,
                    originalCameraSize,
                    t
                );

            yield return null;
        }

        battleCamera.transform.position =
            originalCameraPosition;

        battleCamera.orthographicSize =
            originalCameraSize;
    }

    // =========================================
    // UI
    // =========================================

    private void HideBattleUI()
    {
        battleUI.alpha = 0f;
        battleUI.interactable = false;
        battleUI.blocksRaycasts = false;
    }

    private void RestoreBattleUI()
    {
        battleUI.alpha = originalUIAlpha;
        battleUI.interactable = originalUIInteractable;
        battleUI.blocksRaycasts = originalUIRaycasts;
    }

    // =========================================
    // ANIMATOR
    // =========================================

    private void SetRunning(bool running)
    {
        if (playerAnimator == null ||
            string.IsNullOrEmpty(runningParameter))
            return;

        foreach (AnimatorControllerParameter parameter
                 in playerAnimator.parameters)
        {
            if (parameter.name == runningParameter &&
                parameter.type ==
                AnimatorControllerParameterType.Bool)
            {
                playerAnimator.SetBool(
                    runningParameter,
                    running
                );

                return;
            }
        }
    }

    private void SetTriggerIfExists(string triggerName)
    {
        if (playerAnimator == null ||
            string.IsNullOrEmpty(triggerName))
            return;

        foreach (AnimatorControllerParameter parameter
                 in playerAnimator.parameters)
        {
            if (parameter.name == triggerName &&
                parameter.type ==
                AnimatorControllerParameterType.Trigger)
            {
                playerAnimator.SetTrigger(triggerName);
                return;
            }
        }
    }

    // =========================================
    // CLEANUP
    // =========================================

    private void OnDisable()
    {
        if (!IsPlaying)
            return;

        SetRunning(false);

        if (player != null)
            player.position = originalPlayerPosition;

        if (battleCamera != null)
        {
            battleCamera.transform.position =
                originalCameraPosition;

            battleCamera.orthographicSize =
                originalCameraSize;
        }

        if (battleUI != null)
            RestoreBattleUI();

        IsPlaying = false;
    }
}
