using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuTransition : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform logo;
    [SerializeField] private RectTransform[] menuButtons;

    [Header("Portal")]
    [SerializeField] private RectTransform backgroundRoot;
    [SerializeField] private RectTransform portalFocus;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.7f;
    [SerializeField] private float zoomDuration = 1.3f;
    [SerializeField] private float zoomDelay = 0.25f;
    [SerializeField] private float zoomMultiplier = 3f;

    [Header("Scene Settings")]
    [SerializeField] private bool loadNextScene = false;
    [SerializeField] private string nextSceneName = "GameScene";

    private bool isTransitioning = false;

    public void PlayGame()
    {
        if (isTransitioning)
            return;

        StartCoroutine(PlayTransition());
    }

    private IEnumerator PlayTransition()
    {
        isTransitioning = true;

        if (backgroundRoot == null || portalFocus == null)
        {
            Debug.LogError("Missing BackgroundRoot or PortalFocus!");
            isTransitioning = false;
            yield break;
        }

        RectTransform backgroundParent =
            backgroundRoot.parent as RectTransform;

        Canvas canvas = backgroundRoot.GetComponentInParent<Canvas>();

        if (backgroundParent == null || canvas == null)
        {
            Debug.LogError("Invalid background hierarchy!");
            isTransitioning = false;
            yield break;
        }

        RectTransform canvasRect =
            canvas.GetComponent<RectTransform>();

        // Save initial positions
        Vector2 logoStart = logo != null
            ? logo.anchoredPosition
            : Vector2.zero;

        Vector2[] buttonStarts =
            new Vector2[menuButtons.Length];

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null)
                continue;

            buttonStarts[i] = menuButtons[i].anchoredPosition;

            // Prevent clicking during the animation
            Button button =
                menuButtons[i].GetComponent<Button>();

            if (button != null)
                button.interactable = false;
        }

        // Save original background transformation
        Vector2 bgStartPos = backgroundRoot.anchoredPosition;
        Vector3 bgStartScale = backgroundRoot.localScale;

        Vector3 bgTargetScale =
            bgStartScale * zoomMultiplier;

        // Temporarily zoom to calculate the correct
        // position that centers the portal.
        backgroundRoot.localScale = bgTargetScale;

        Vector3 portalPosition =
            backgroundParent.InverseTransformPoint(
                portalFocus.position
            );

        Vector3 screenCenter =
            backgroundParent.InverseTransformPoint(
                canvasRect.TransformPoint(
                    canvasRect.rect.center
                )
            );

        Vector2 bgTargetPos =
            bgStartPos +
            (Vector2)(screenCenter - portalPosition);

        // Restore background before animation begins
        backgroundRoot.localScale = bgStartScale;

        float time = 0f;

        float totalDuration = Mathf.Max(
            zoomDelay + zoomDuration,
            slideDuration + menuButtons.Length * 0.08f
        );

        while (time < totalDuration)
        {
            time += Time.unscaledDeltaTime;

            // Logo moves upward
            float logoT = Mathf.Clamp01(
                time / slideDuration
            );

            logoT = Mathf.SmoothStep(0f, 1f, logoT);

            if (logo != null)
            {
                logo.anchoredPosition = Vector2.Lerp(
                    logoStart,
                    logoStart + Vector2.up *
                    canvasRect.rect.height * 1.2f,
                    logoT
                );
            }

            // Buttons slide left one by one
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] == null)
                    continue;

                float delay = i * 0.08f;

                float buttonT = Mathf.Clamp01(
                    (time - delay) / slideDuration
                );

                buttonT = Mathf.SmoothStep(
                    0f, 1f, buttonT
                );

                Vector2 targetPosition =
                    buttonStarts[i] +
                    Vector2.left *
                    canvasRect.rect.width * 1.2f;

                menuButtons[i].anchoredPosition =
                    Vector2.Lerp(
                        buttonStarts[i],
                        targetPosition,
                        buttonT
                    );
            }

            // Zoom toward portal
            float zoomT = Mathf.Clamp01(
                (time - zoomDelay) / zoomDuration
            );

            zoomT = Mathf.SmoothStep(
                0f, 1f, zoomT
            );

            backgroundRoot.localScale =
                Vector3.Lerp(
                    bgStartScale,
                    bgTargetScale,
                    zoomT
                );

            backgroundRoot.anchoredPosition =
                Vector2.Lerp(
                    bgStartPos,
                    bgTargetPos,
                    zoomT
                );

            yield return null;
        }

        // Load next scene when enabled
        if (loadNextScene &&
            !string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}