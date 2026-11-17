
using System.Collections;
using UnityEngine;

public class PlayerCardCinematic : MonoBehaviour
{
    // =====================================
    // CARD ZOOM SETTINGS
    // =====================================

    [System.Serializable]
    public class CardZoomSetting
    {
        [Tooltip("CardData muốn tùy chỉnh Zoom")]
        public CardData card;

        [Tooltip("Bật hoặc tắt Camera Zoom cho Card này")]
        public bool useZoom = true;
    }

    // =====================================
    // REFERENCES
    // =====================================

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerAnimationController playerAnimation;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private CardEffectManager cardEffectManager;

    // =====================================
    // CAMERA
    // =====================================

    [Header("Camera")]
    [SerializeField] private Camera battleCamera;

    [SerializeField] private float zoomSize = 3f;
    [SerializeField] private float zoomDuration = 0.4f;
    [SerializeField] private float cameraFollowSpeed = 7f;

    [SerializeField]
    private Vector2 cameraFocusOffset = new Vector2(0f, 0.5f);

    // =====================================
    // PER-CARD ZOOM
    // =====================================

    [Header("Zoom Settings Per Card")]

    [Tooltip("Dùng cho Card chưa có trong danh sách bên dưới")]
    [SerializeField] private bool defaultUseZoom = true;

    [Tooltip("Mỗi Card có thể bật hoặc tắt Zoom riêng")]
    [SerializeField]
    private CardZoomSetting[] cardZoomSettings = new CardZoomSetting[0];

    [Tooltip("Ẩn Battle UI khi dùng Card không Zoom")]
    [SerializeField] private bool hideUIWithoutZoom = false;

    // =====================================
    // UI
    // =====================================

    [Header("UI")]
    [SerializeField] private CanvasGroup battleUI;

    // =====================================
    // MOVEMENT
    // =====================================

    [Header("Movement")]
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float returnDuration = 0.4f;

    [Tooltip("Khoảng cách giữa Player và Enemy")]
    [SerializeField] private float stoppingDistance = 1.5f;

    [Tooltip("Bật nếu muốn Player chạy về vị trí ban đầu sau khi tấn công")]
    [SerializeField] private bool returnToOriginalPositionAfterAttack = false;

    // =====================================
    // ANIMATION
    // =====================================

    [Header("Animation Parameters")]
    [SerializeField] private string runningParameter = "IsRunning";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string healTrigger = "";
    [SerializeField] private string shieldTrigger = "";
    [SerializeField] private string buffTrigger = "";

    // =====================================
    // ACTION DURATION
    // =====================================

    [Header("Action Duration")]
    [SerializeField] private float attackDuration = 0.8f;
    [SerializeField] private float buffDuration = 0.7f;

    // =====================================
    // RUNTIME STATE
    // =====================================

    private Vector3 originalPlayerPosition;
    private Vector3 originalCameraPosition;

    private float originalCameraSize;

    private float originalUIAlpha;
    private bool originalUIInteractable;
    private bool originalUIRaycasts;

    private bool zoomThisCard;
    private bool uiHiddenThisCard;

    private Coroutine cinematicCoroutine;

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
                    player.GetComponentInChildren<PlayerAnimationController>(true);
            }
        }
    }

    // =====================================
    // DETERMINE ZOOM FOR CARD
    // =====================================

    private bool ShouldZoom(CardData card)
    {
        if (card == null)
            return defaultUseZoom;

        if (cardZoomSettings != null)
        {
            foreach (CardZoomSetting setting in cardZoomSettings)
            {
                if (setting == null || setting.card == null)
                    continue;

                if (setting.card == card)
                    return setting.useZoom;
            }
        }

        return defaultUseZoom;
    }

    // =====================================
    // PLAY CARD
    // =====================================

    public bool PlayCard(CardData card)
    {
        Debug.Log("[TEST 1] PlayCard đã được gọi!", this);

        if (!isActiveAndEnabled || IsPlaying || card == null)
        {
            Debug.LogWarning(
                "PlayerCardCinematic: Không thể chạy Cinematic!",
                this
            );

            return false;
        }

        // Kiểm tra Reference.
        if (player == null ||
            battleCamera == null ||
            battleUI == null ||
            cardEffectManager == null)
        {
            Debug.LogError(
                "PlayerCardCinematic: Thiếu Reference!",
                this
            );

            return false;
        }

        // Camera phải là Orthographic.
        if (!battleCamera.orthographic)
        {
            Debug.LogError(
                "PlayerCardCinematic: Camera phải là Orthographic!",
                this
            );

            return false;
        }

        // Kiểm tra Enemy nếu là Card tấn công.
        if (IsAttackCard(card))
        {
            EnemyHealth enemy = GetEnemy();

            if (enemy == null || enemy.IsDead)
            {
                Debug.LogWarning(
                    "PlayerCardCinematic: Enemy không hợp lệ!",
                    this
                );

                return false;
            }
        }

        // Lấy cấu hình Zoom của Card.
        zoomThisCard = ShouldZoom(card);

        // Đánh dấu Cinematic đang chạy.
        IsPlaying = true;

        Debug.Log(
            $"PlayerCardCinematic: {card.cardName} | Zoom: {zoomThisCard}",
            this
        );

        cinematicCoroutine = StartCoroutine(
            PlayCardSequence(card)
        );

        return true;
    }

    // =====================================
    // CARD CLASSIFICATION
    // =====================================

    private bool IsAttackCard(CardData card)
    {
        if (card == null)
            return false;

        int type = (int)card.effectType;

        // 0 = Damage
        // 3 = Poison
        // 5 = MultiHit

        return type == 0 ||
               type == 3 ||
               type == 5;
    }

    // =====================================
    // GET ENEMY
    // =====================================

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

        // 1. Ẩn UI nếu cần.
        uiHiddenThisCard = zoomThisCard || hideUIWithoutZoom;

        if (uiHiddenThisCard)
            HideBattleUI();

        // 2. Zoom Camera.
        if (zoomThisCard)
        {
            yield return ZoomCamera(true);
        }

        // 3. Thực thi Card.
        if (IsAttackCard(card))
        {
            yield return ExecuteAttackCard(card);
        }
        else
        {
            yield return ExecuteBuffCard(card);
        }

        // 4. Trả Camera về vị trí ban đầu.
        if (zoomThisCard)
        {
            yield return ZoomCamera(false);
        }

        // 5. Khôi phục trạng thái.
        SetRunning(false);

        if (uiHiddenThisCard)
            RestoreBattleUI();

        // 6. Kết thúc.
        FinishCinematic();

        Debug.Log(
            "PlayerCardCinematic: Hoàn tất " + card.cardName,
            this
        );
    }

    // =====================================
    // ATTACK CARD
    // =====================================

    private IEnumerator ExecuteAttackCard(CardData card)
    {
        Debug.Log("[TEST 2] ExecuteAttackCard đã chạy!", this);

        EnemyHealth enemy = GetEnemy();

        if (enemy == null || enemy.IsDead)
        {
            Debug.LogWarning(
                "PlayerCardCinematic: Không tìm thấy Enemy hoặc Enemy đã chết!",
                this
            );

            yield break;
        }

        // =====================================
        // CALCULATE TARGET POSITION
        // =====================================

        Vector3 targetPosition = originalPlayerPosition;

        float distanceX =
            enemy.transform.position.x -
            originalPlayerPosition.x;

        float direction = distanceX >= 0f ? 1f : -1f;

        float safeStoppingDistance =
            Mathf.Max(0f, stoppingDistance);

        targetPosition.x =
            enemy.transform.position.x -
            direction * safeStoppingDistance;

        targetPosition.y = originalPlayerPosition.y;
        targetPosition.z = originalPlayerPosition.z;

        Debug.Log(
            $"PlayerCardCinematic: Stopping Distance = {safeStoppingDistance}"
        );

        // =====================================
        // 1. PLAYER RUN TO ENEMY
        // =====================================

        if (Vector3.Distance(player.position, targetPosition) > 0.01f)
        {
            SetRunning(true);

            yield return MovePlayer(
                targetPosition,
                dashDuration
            );

            float actualDistance = Mathf.Abs(
                enemy.transform.position.x - player.position.x
            );

            Debug.Log(
                $"[STOPPING DISTANCE] " +
                $"Player X: {player.position.x} | " +
                $"Enemy X: {enemy.transform.position.x} | " +
                $"Khoảng cách thực tế: {actualDistance} | " +
                $"Khoảng cách mong muốn: {safeStoppingDistance}"
            );

            SetRunning(false);
        }

        // =====================================
        // 2. ATTACK ANIMATION
        // =====================================

        PlayAttackAnimation();

        // =====================================
        // 3. EXECUTE CARD EFFECT
        // =====================================

        cardEffectManager.ExecuteCardEffect(card);

        // =====================================
        // 4. WAIT FOR ATTACK
        // =====================================

        yield return new WaitForSeconds(
            Mathf.Max(0f, attackDuration)
        );

        // =====================================
        // 5. RETURN TO ORIGINAL POSITION
        // =====================================

        if (returnToOriginalPositionAfterAttack)
        {
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

        // Thực thi hiệu ứng đúng một lần.
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

        SetTriggerIfExists(attackTrigger);
    }

    // =====================================
    // PLAYER MOVEMENT
    // =====================================

    private IEnumerator MovePlayer(
        Vector3 target,
        float duration)
    {
        if (player == null)
            yield break;

        Vector3 start = player.position;

        duration = Mathf.Max(0f, duration);

        if (duration <= 0f)
        {
            player.position = target;

            if (zoomThisCard)
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

            // Chỉ Camera Follow khi Card có Zoom.
            if (zoomThisCard)
                FollowPlayerCamera();

            yield return null;
        }

        player.position = target;

        if (zoomThisCard)
            FollowPlayerCamera();
    }

    // =====================================
    // CAMERA ZOOM
    // =====================================

    private IEnumerator ZoomCamera(bool zoomIn)
    {
        Vector3 startPosition =
            battleCamera.transform.position;

        float startSize =
            battleCamera.orthographicSize;

        Vector3 targetPosition = zoomIn
            ? GetCameraFocusPosition()
            : originalCameraPosition;

        float targetSize = zoomIn
            ? zoomSize
            : originalCameraSize;

        float duration = Mathf.Max(0.01f, zoomDuration);

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

        battleCamera.transform.position = targetPosition;
        battleCamera.orthographicSize = targetSize;
    }

    // =====================================
    // CAMERA FOLLOW
    // =====================================

    private void FollowPlayerCamera()
    {
        if (!zoomThisCard ||
            battleCamera == null)
            return;

        float smooth = 1f - Mathf.Exp(
            -cameraFollowSpeed * Time.deltaTime
        );

        battleCamera.transform.position =
            Vector3.Lerp(
                battleCamera.transform.position,
                GetCameraFocusPosition(),
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

    // =====================================
    // SAVE ORIGINAL STATE
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

    // =====================================
    // HIDE / RESTORE UI
    // =====================================

    private void HideBattleUI()
    {
        battleUI.alpha = 0f;
        battleUI.interactable = false;
        battleUI.blocksRaycasts = false;
    }

    private void RestoreBattleUI()
    {
        if (battleUI == null)
            return;

        battleUI.alpha = originalUIAlpha;
        battleUI.interactable = originalUIInteractable;
        battleUI.blocksRaycasts = originalUIRaycasts;
    }

    // =====================================
    // ANIMATOR
    // =====================================

    private void SetRunning(bool running)
    {
        if (HasParameter(
                runningParameter,
                AnimatorControllerParameterType.Bool))
        {
            playerAnimator.SetBool(
                runningParameter,
                running
            );
        }
    }

    private void SetTriggerIfExists(string triggerName)
    {
        if (HasParameter(
                triggerName,
                AnimatorControllerParameterType.Trigger))
        {
            playerAnimator.SetTrigger(triggerName);
        }
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
    // FINISH CINEMATIC
    // =====================================

    private void FinishCinematic()
    {
        SetRunning(false);

        IsPlaying = false;
        zoomThisCard = false;
        uiHiddenThisCard = false;

        cinematicCoroutine = null;
    }

    // =====================================
    // CLEANUP
    // =====================================

    private void OnDisable()
    {
        if (!IsPlaying)
            return;

        // Dừng trạng thái Running.
        SetRunning(false);

        // Trả Player về vị trí ban đầu.
        if (player != null)
            player.position = originalPlayerPosition;

        // Khôi phục Camera.
        if (battleCamera != null && zoomThisCard)
        {
            battleCamera.transform.position =
                originalCameraPosition;

            battleCamera.orthographicSize =
                originalCameraSize;
        }

        // Khôi phục UI.
        if (battleUI != null && uiHiddenThisCard)
            RestoreBattleUI();

        IsPlaying = false;

        zoomThisCard = false;
        uiHiddenThisCard = false;
        cinematicCoroutine = null;
    }
}