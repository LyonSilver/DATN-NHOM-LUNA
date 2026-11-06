
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

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerAnimationController playerAnimation;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private CardEffectManager cardEffectManager;

    [Header("One Card Per Turn")]
    [SerializeField] private OneCardTurnController oneCardTurnController;

    // =====================================
    // CAMERA
    // =====================================

    [Header("Camera")]
    [SerializeField] private Camera battleCamera;

    [SerializeField] private float zoomSize = 3f;
    [SerializeField] private float zoomDuration = 0.4f;
    [SerializeField] private float cameraFollowSpeed = 7f;

    [SerializeField] private Vector2 cameraFocusOffset =
        new Vector2(0f, 0.5f);

    // =====================================
    // PER-CARD ZOOM
    // =====================================

    [Header("Zoom Settings Per Card")]

    [Tooltip("Dùng cho Card chưa có trong danh sách bên dưới")]
    [SerializeField] private bool defaultUseZoom = true;

    [Tooltip("Mỗi Card có thể bật hoặc tắt Zoom riêng")]
    [SerializeField]
    private CardZoomSetting[] cardZoomSettings =
        new CardZoomSetting[0];

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
    [SerializeField] private float stoppingDistance = 1.5f;

    // =====================================
    // ANIMATION
    // =====================================

    [Header("Animation Parameters")]
    [SerializeField] private string runningParameter = "IsRunning";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string healTrigger = "";
    [SerializeField] private string shieldTrigger = "";
    [SerializeField] private string buffTrigger = "";

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

    public bool IsPlaying { get; private set; }

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Awake()
    {
        if (battleCamera == null)
            battleCamera = Camera.main;

        if (player == null)
            return;

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

                // So sánh đúng CardData asset.
                if (setting.card == card)
                {
                    return setting.useZoom;
                }
            }
        }

        // Card chưa cấu hình sẽ dùng giá trị mặc định.
        return defaultUseZoom;
    }

    // =====================================
    // PLAY CARD
    // =====================================

    public bool PlayCard(CardData card)
    {
        if (!isActiveAndEnabled || IsPlaying || card == null)
            return false;

        if (oneCardTurnController == null)
        {
            Debug.LogError(
                "PlayerCardCinematic: " +
                "Chưa gán OneCardTurnController!"
            );
            return false;
        }

        // Chỉ cho phép sử dụng 1 Card mỗi lượt.
        if (!oneCardTurnController.CanUseCard())
        {
            Debug.Log(
                "PlayerCardCinematic: " +
                "Không thể dùng thêm Card trong lượt này!"
            );
            return false;
        }

        if (player == null ||
            battleCamera == null ||
            battleUI == null ||
            cardEffectManager == null)
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

        // Lấy cấu hình Zoom của Card vừa chọn.
        zoomThisCard = ShouldZoom(card);

        IsPlaying = true;

        Debug.Log(
            $"PlayerCardCinematic: {card.cardName} | " +
            $"Zoom: {zoomThisCard}"
        );

        StartCoroutine(PlayCardSequence(card));

        // CardManager sẽ gọi NotifyCardStarted()
        // sau khi PlayCard trả về true.
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

        // Chỉ ẩn UI khi Zoom hoặc khi được bật tùy chọn.
        uiHiddenThisCard =
            zoomThisCard || hideUIWithoutZoom;

        if (uiHiddenThisCard)
            HideBattleUI();

        // 1. Zoom nếu Card được phép Zoom.
        if (zoomThisCard)
        {
            yield return ZoomCamera(true);
        }

        // 2. Thực hiện Card.
        if (IsAttackCard(card))
        {
            yield return ExecuteAttackCard(card);
        }
        else
        {
            yield return ExecuteBuffCard(card);
        }

        // 3. Trở về Camera ban đầu nếu đã Zoom.
        if (zoomThisCard)
        {
            yield return ZoomCamera(false);
        }

        // 4. Khôi phục UI.
        SetRunning(false);

        if (uiHiddenThisCard)
            RestoreBattleUI();

        // 5. Kết thúc Cinematic.
        IsPlaying = false;
        zoomThisCard = false;
        uiHiddenThisCard = false;

        Debug.Log(
            "PlayerCardCinematic: Hoàn tất " +
            card.cardName
        );

        // OneCardTurnController sẽ chuyển Enemy Turn
        // sau khi CardManager hoàn tất.
    }

    // =====================================
    // ATTACK CARD
    // =====================================

    private IEnumerator ExecuteAttackCard(CardData card)
    {
        EnemyHealth enemy = GetEnemy();

        if (enemy == null || enemy.IsDead)
            yield break;

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

        targetPosition.y = originalPlayerPosition.y;
        targetPosition.z = originalPlayerPosition.z;

        // 1. Player chạy tới Enemy.
        if (Vector3.Distance(
                player.position,
                targetPosition) > 0.01f)
        {
            SetRunning(true);

            yield return MovePlayer(
                targetPosition,
                dashDuration
            );

            SetRunning(false);
        }

        // 2. Phát Attack Animation.
        PlayAttackAnimation();

        // 3. Damage và Skill VFX.
        // Chỉ gọi một lần.
        cardEffectManager.ExecuteCardEffect(card);

        // 4. Chờ Animation.
        yield return new WaitForSeconds(
            Mathf.Max(0f, attackDuration)
        );

        // 5. Player chạy về vị trí ban đầu.
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

        // Buff vẫn hoạt động dù không Zoom.
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
        Vector3 start = player.position;

        duration = Mathf.Max(0f, duration);

        if (duration <= 0f)
        {
            player.position = target;

            // Chỉ Follow Camera nếu Card đang Zoom.
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

            // Card không Zoom: Camera đứng yên.
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

    // =====================================
    // CAMERA FOLLOW
    // =====================================

    private void FollowPlayerCamera()
    {
        // Khi Card không Zoom, không di chuyển Camera.
        if (!zoomThisCard)
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
    // CLEANUP
    // =====================================

    private void OnDisable()
    {
        if (!IsPlaying)
            return;

        SetRunning(false);

        if (player != null)
            player.position = originalPlayerPosition;

        // Chỉ khôi phục Camera nếu Cinematic có Zoom.
        if (battleCamera != null && zoomThisCard)
        {
            battleCamera.transform.position =
                originalCameraPosition;

            battleCamera.orthographicSize =
                originalCameraSize;
        }

        if (battleUI != null && uiHiddenThisCard)
            RestoreBattleUI();

        IsPlaying = false;
        zoomThisCard = false;
        uiHiddenThisCard = false;
    }
}
