using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button lock + buy slot. Gắn lên Button cần khóa.
/// Nút "BUY SLOT" gọi TryBuy() của script này.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class UIButtonBuyLock : MonoBehaviour
{
    [Header("Lock")]
    [SerializeField] private bool startLocked = true;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private Vector2 lockIconSize = new Vector2(32f, 32f);
    [SerializeField] private Vector2 lockIconPosition = Vector2.zero;

    [Header("Buy")]
    [SerializeField] private int cost = 100;
    [SerializeField] private string moneyPlayerPrefsKey = "PlayerMoney"; // đổi nếu dùng cách khác
    [SerializeField] private GameObject buyButtonObject; // optional: nút BUY SLOT riêng

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

        if (startLocked) Lock();
        else HideLockIcon();

        UpdateBuyButton();
    }

    public int GetCost() => cost;
    public bool IsLocked() => locked;

    public int GetCurrentMoney()
    {
        return PlayerPrefs.GetInt(moneyPlayerPrefsKey, 0);
    }

    public void SetMoney(int amount)
    {
        PlayerPrefs.SetInt(moneyPlayerPrefsKey, Mathf.Max(0, amount));
        PlayerPrefs.Save();
        UpdateBuyButton();
    }

    public void AddMoney(int amount) => SetMoney(GetCurrentMoney() + amount);

    /// <summary>Gọi từ nút BUY SLOT</summary>
    public void TryBuy()
    {
        if (!locked) return;

        int money = GetCurrentMoney();
        if (money < cost)
        {
            Debug.Log($"Không đủ tiền. Cần {cost}, hiện có {money}");
            return;
        }

        SetMoney(money - cost);
        Unlock();
    }

    public void Lock()
    {
        if (button == null) button = GetComponent<Button>();

        if (!locked)
        {
            originalInteractable = button.interactable;
            originalNavigation = button.navigation;
        }

        locked = true;
        button.interactable = false;

        var nav = button.navigation;
        nav.mode = Navigation.Mode.None;
        button.navigation = nav;

        foreach (var state in originalGraphics)
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

        UpdateBuyButton();
    }

    public void Unlock()
    {
        if (button == null) button = GetComponent<Button>();

        locked = false;
        button.interactable = originalInteractable;
        button.navigation = originalNavigation;

        foreach (var state in originalGraphics)
        {
            if (state.graphic != null)
                state.graphic.raycastTarget = state.raycastTarget;
        }

        HideLockIcon();
        UpdateBuyButton();
    }

    public void ToggleLock()
    {
        if (locked) Unlock();
        else Lock();
    }

    private void UpdateBuyButton()
    {
        if (buyButtonObject == null) return;
        buyButtonObject.SetActive(locked);

        if (locked)
        {
            var buyBtn = buyButtonObject.GetComponent<Button>();
            if (buyBtn != null)
                buyBtn.interactable = GetCurrentMoney() >= cost;
        }
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
                lockIconObject = new GameObject("LockIcon_Auto", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                lockIconObject.transform.SetParent(transform, false);
                lockIconImage = lockIconObject.GetComponent<Image>();
            }
        }

        lockIconImage.sprite = lockSprite;
        lockIconImage.preserveAspect = true;
        lockIconImage.raycastTarget = false;

        var rect = lockIconImage.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
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