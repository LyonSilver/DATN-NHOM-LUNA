
using System.Collections;
using UnityEngine;

public class PlayerCardCinematic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerAnimationController playerAnimation;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private CardEffectManager cardEffectManager;

    [Header("Camera")]
    [SerializeField] private Camera battleCamera;
    [SerializeField] private float zoomSize = 3f;
    [SerializeField] private float zoomDuration = 0.4f;
    [SerializeField] private float cameraFollowSpeed = 7f;
    [SerializeField] private Vector2 cameraFocusOffset =
        new Vector2(0f, 0.5f);

    [Header("UI")]
    [SerializeField] private CanvasGroup battleUI;

    [Header("Movement")]
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float returnDuration = 0.4f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Animation Parameters")]
    [SerializeField] private string runningParameter = "IsRunning";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string healTrigger = "";
    [SerializeField] private string shieldTrigger = "";
    [SerializeField] private string buffTrigger = "";

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

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Awake()
    {
        if (battleCamera == null)
            battleCamera = Camera.main;

        if (player != null)
        {
            if (playerAnimator == null)
            {
                playerAnimator =
                    player.GetComponentInChildren<Animator>(true);
            }

            if (playerAnimation == null)
            {
                playerAnimation =
                    player.GetComponentInChildren<
                        PlayerAnimationController>(true);
            }
        }
    }

    // =====================================
    // PLAY CARD
    // =====================================

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

        // Slash, Heavy Hit, Poison Dart, Double Strike
        // đều cần Enemy còn sống.
        if (IsAttackCard(card))
        {
            EnemyHealth enemy = GetEnemy();

            if (enemy == null || enemy.IsDead)
            {
                Debug.LogWarning(
                    "PlayerCardCinematic: Enemy không hợp lệ!"
                );
                return false;
            }
        }

        IsPlaying = true;
        StartCoroutine(PlayCardSequence(card));

        return true;
    }

    // =====================================
    // CARD CLASSIFICATION
    // =====================================

    private bool IsAttackCard(CardData card)
    {
        if (card == null)
            return false;

        // Theo thứ tự CardEffectType trong CardData:
        // 0 = Damage
        // 3 = Poison
        // 5 = MultiHit

        int type = (int)card.effectType;

        return type == 0 ||
               type == 3 ||
               type == 5;
    }

    private EnemyHealth GetEnemy()
    {
        return enemySpawner != null
            ? enemySpawner.GetCurrentEnemyHealth()
            : null;
    }

    // =====================================
    // MAIN CINEMATIC
    // =====================================

    private IEnumerator PlayCardSequence(CardData card)
    {
        SaveOriginalState();
        HideBattleUI();

        // 1. Zoom vào Player.
        yield return ZoomCamera(true);

        // 2. Thực thi Card.
        if (IsAttackCard(card))
            yield return ExecuteAttackCard(card);
        else
            yield return ExecuteBuffCard(card);

        // 3. Zoom về Camera ban đầu.
        yield return ZoomCamera(false);

        // 4. Khôi phục UI.
        RestoreBattleUI();

        IsPlaying = false;

        Debug.Log(
            "PlayerCardCinematic: Card hoàn tất!"
        );
    }

    // =====================================
    // ATTACK CARD
    // =====================================

    private IEnumerator ExecuteAttackCard(CardData card)
    {
        EnemyHealth enemy = GetEnemy();

        if (enemy == null || enemy.IsDead)
            yield break;

        // Xác định vị trí Player cần chạy tới.
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

        // Giữ nguyên Y và Z.
        targetPosition.y = originalPlayerPosition.y;
        targetPosition.z = originalPlayerPosition.z;

        // 1. Player chạy tới Enemy.
        if (Vector3.Distance(player.position, targetPosition) > 0.01f)
        {
            SetRunning(true);

            yield return MovePlayer(
                targetPosition,
                dashDuration
            );

            SetRunning(false);
        }

        // 2. PLAYER ATTACK ANIMATION.
        PlayAttackAnimation();

        // 3. DAMAGE + SKILL VFX.
        // Chỉ gọi ExecuteCardEffect một lần.
        cardEffectManager.ExecuteCardEffect(card);

        // 4. Đợi Attack Animation.
        yield return new WaitForSeconds(
            Mathf.Max(0f, attackDuration)
        );

        // 5. Player chạy về.
        if (Vector3.Distance(
                player.position,
                originalPlayerPosition) > 0.01f)
        {
            SetRunning(true);

            yield return MovePlayer(
                originalPlayerPosition,
                returnDuration
            );

            SetRunning(false);
        }
    }

    // =====================================
    // BUFF CARD
    // =====================================

    private IEnumerator ExecuteBuffCard(CardData card)
    {
        int type = (int)card.effectType;

        switch (type)
        {
            case 1: // Shield
                SetTriggerIfExists(shieldTrigger);
                break;

            case 2: // Heal
                SetTriggerIfExists(healTrigger);
                break;

            default: // Focus / Draw
                SetTriggerIfExists(buffTrigger);
                break;
        }

        cardEffectManager.ExecuteCardEffect(card);

        yield return new WaitForSeconds(
            Mathf.Max(0f, buffDuration)
        );
    }

    // =====================================
    // ATTACK ANIMATION
    // =====================================

    private void PlayAttackAnimation()
    {
        if (playerAnimation != null)
        {
            playerAnimation.PlayAttack();
            return;
        }

        // Dự phòng nếu chưa gán
        // PlayerAnimationController.
        Debug.LogWarning(
            "PlayerCardCinematic: " +
            "Chưa gán PlayerAnimationController, " +
            "thử phát trực tiếp qua Animator."
        );

        SetTriggerIfExists(attackTrigger);
    }

    // =====================================
    // MOVEMENT
    // =====================================

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
        FollowPlayerCamera();
    }

    // =====================================
    // CAMERA
    // =====================================

    private IEnumerator ZoomCamera(bool zoomIn)
    {
        Vector3 startPosition =
            battleCamera.transform.position;

        float startSize = battleCamera.orthographicSize;

        Vector3 targetPosition = zoomIn
            ? GetCameraFocusPosition()
            : originalCameraPosition;

        float targetSize = zoomIn
            ? zoomSize
            : originalCameraSize;

        float duration = Mathf.Max(
            0.01f,
            zoomDuration
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
                    targetSize,
                    t
                );

            yield return null;
        }

        battleCamera.transform.position =
            targetPosition;

        battleCamera.orthographicSize =
            targetSize;
    }

    private void FollowPlayerCamera()
    {
        float t = 1f - Mathf.Exp(
            -cameraFollowSpeed * Time.deltaTime
        );

        battleCamera.transform.position =
            Vector3.Lerp(
                battleCamera.transform.position,
                GetCameraFocusPosition(),
                t
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

    // =====================================
    // SAVE / RESTORE
    // =====================================

    private void SaveOriginalState()
    {
        originalPlayerPosition = player.position;

        originalCameraPosition =
            battleCamera.transform.position;

        originalCameraSize =
            battleCamera.orthographicSize;

        originalUIAlpha = battleUI.alpha;
        originalUIInteractable = battleUI.interactable;
        originalUIRaycasts = battleUI.blocksRaycasts;
    }

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

    // =====================================
    // ANIMATOR
    // =====================================

    private void SetRunning(bool running)
    {
        if (!HasParameter(
                runningParameter,
                AnimatorControllerParameterType.Bool))
        {
            return;
        }

        playerAnimator.SetBool(runningParameter, running);
    }

    private void SetTriggerIfExists(string triggerName)
    {
        if (!HasParameter(
                triggerName,
                AnimatorControllerParameterType.Trigger))
        {
            return;
        }

        playerAnimator.SetTrigger(triggerName);
    }

    private bool HasParameter(
        string parameterName,
        AnimatorControllerParameterType type)
    {
        if (playerAnimator == null ||
            string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter
                 in playerAnimator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == type)
            {
                return true;
            }
        }

        return false;
    }

    // =====================================
    // CLEANUP
    // =====================================

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
