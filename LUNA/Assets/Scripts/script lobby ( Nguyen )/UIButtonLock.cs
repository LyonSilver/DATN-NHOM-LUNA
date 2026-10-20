using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this script directly to the Button you want to lock.
/// Assign your custom lock Sprite in the Inspector.
/// No separate manager GameObject is required.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class UIButtonLock : MonoBehaviour
{
    [Header("Lock Settings")]
    [SerializeField] private bool startLocked = true;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private Vector2 lockIconSize = new Vector2(32f, 32f);
    [SerializeField] private Vector2 lockIconPosition = Vector2.zero;

    private Button button;
    private bool locked;
    private bool originalInteractable;
    private Navigation originalNavigation;
    private readonly List<GraphicState> originalGraphics = new List<GraphicState>();

    private GameObject lockIconObject;
    private Image lockIconImage;

    private sealed class GraphicState
    {
        public Graphic graphic;
        public bool raycastTarget;

        public GraphicState(Graphic graphic)
        {
            this.graphic = graphic;
            raycastTarget = graphic != null && graphic.raycastTarget;
        }
    }

    private void Awake()
    {
        button = GetComponent<Button>();

        originalInteractable = button.interactable;
        originalNavigation = button.navigation;

        originalGraphics.Clear();
        foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true))
            originalGraphics.Add(new GraphicState(graphic));

        if (startLocked)
            Lock();
        else
            HideLockIcon();
    }

    /// <summary>Lock this Button and show the assigned icon, if any.</summary>
    public void Lock()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (!locked)
        {
            originalInteractable = button.interactable;
            originalNavigation = button.navigation;
        }

        locked = true;
        button.interactable = false;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        foreach (GraphicState state in originalGraphics)
        {
            if (state.graphic != null && state.graphic != lockIconImage)
                state.graphic.raycastTarget = false;
        }

        if (lockSprite != null)
        {
            CreateOrUpdateLockIcon();
            lockIconObject.SetActive(true);
            lockIconObject.transform.SetAsLastSibling();
        }
        else if (lockIconObject != null)
        {
            lockIconObject.SetActive(true);
        }
    }

    /// <summary>Unlock this Button and restore its original interaction settings.</summary>
    public void Unlock()
    {
        if (button == null)
            button = GetComponent<Button>();

        locked = false;
        button.interactable = originalInteractable;
        button.navigation = originalNavigation;

        foreach (GraphicState state in originalGraphics)
        {
            if (state.graphic != null)
                state.graphic.raycastTarget = state.raycastTarget;
        }

        HideLockIcon();
    }

    /// <summary>Toggle between locked and unlocked.</summary>
    public void ToggleLock()
    {
        if (locked)
            Unlock();
        else
            Lock();
    }

    public bool IsLocked()
    {
        return locked;
    }

    private void CreateOrUpdateLockIcon()
    {
        if (lockIconObject == null)
        {
            Transform existing = transform.Find("LockIcon_Auto");

            if (existing != null)
            {
                lockIconObject = existing.gameObject;
                lockIconImage = existing.GetComponent<Image>();
            }

            if (lockIconImage == null)
            {
                lockIconObject = new GameObject(
                    "LockIcon_Auto",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));

                lockIconObject.transform.SetParent(transform, false);
                lockIconImage = lockIconObject.GetComponent<Image>();
            }
        }

        lockIconImage.sprite = lockSprite;
        lockIconImage.preserveAspect = true;
        lockIconImage.raycastTarget = false;

        RectTransform rect = lockIconImage.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = lockIconSize;
        rect.anchoredPosition = lockIconPosition;
    }

    private void HideLockIcon()
    {
        if (lockIconObject != null)
            lockIconObject.SetActive(false);
    }
}
