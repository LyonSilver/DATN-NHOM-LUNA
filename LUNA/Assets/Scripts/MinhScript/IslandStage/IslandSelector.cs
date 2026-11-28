
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class IslandSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] IslandStageManager manager;
    [SerializeField] Camera mapCamera;

    [Header("Click Settings")]
    [SerializeField] float dragThreshold = 10f;

    public event Action<StageData> StageChosen;
    public event Action Moved;
    public event Action LockedClicked;

    public IslandStage Selected { get; private set; }

    public bool InputLocked { get; set; }

    private Vector2 mouseDownPosition;
    private bool validPress;

    private void Awake()
    {
        if (mapCamera == null)
            mapCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (manager != null)
            manager.Refreshed += Reselect;
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.Refreshed -= Reselect;

        validPress = false;
    }

    private void Update()
    {
        if (InputLocked)
        {
            validPress = false;
            return;
        }

        CheckMouse();
    }

    // =====================================
    // MOUSE INPUT
    // =====================================

    private void CheckMouse()
    {
        if (Mouse.current == null ||
            mapCamera == null ||
            manager == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseDownPosition = mousePosition;
            validPress = !IsPointerOverUI();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            bool canClick = validPress;
            validPress = false;

            if (!canClick || IsPointerOverUI())
                return;

            float distance = Vector2.Distance(
                mouseDownPosition,
                mousePosition
            );

            // Kéo camera thì không chọn đảo.
            if (distance > dragThreshold)
                return;

            SelectAtPosition(mousePosition);
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

    // =====================================
    // FIND ISLAND
    // =====================================

    private void SelectAtPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition =
            mapCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    -mapCamera.transform.position.z
                )
            );

        IslandStage[] islands = manager.Islands;

        if (islands == null)
            return;

        foreach (IslandStage island in islands)
        {
            if (island == null)
                continue;

            if (!island.ContainsPoint(worldPosition))
                continue;

            SelectIsland(island);
            return;
        }
    }

    // =====================================
    // SELECT ISLAND
    // =====================================

    public void SelectIsland(IslandStage island)
    {
        if (InputLocked ||
            island == null ||
            island.Data == null)
            return;

        if (!island.IsUnlocked)
        {
            Debug.Log(
                "Dao dang khoa: " +
                island.Data.displayName
            );

            LockedClicked?.Invoke();
            return;
        }

        Selected = island;

        Debug.Log(
            "Da chon dao: " +
            island.Data.displayName
        );

        Moved?.Invoke();

        StageChosen?.Invoke(island.Data);
    }

    // =====================================
    // SELECT CURRENT ISLAND
    // =====================================

    public void Reselect()
    {
        if (manager == null)
            return;

        Selected = manager.CurrentIsland;

        if (Selected != null)
            return;

        IslandStage[] islands = manager.Islands;

        if (islands == null)
            return;

        for (int i = islands.Length - 1; i >= 0; i--)
        {
            if (islands[i] != null &&
                islands[i].IsUnlocked)
            {
                Selected = islands[i];
                return;
            }
        }
    }
}