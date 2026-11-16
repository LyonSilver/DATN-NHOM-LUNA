#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ShopInventoryButtonRepairWindow : EditorWindow
{
    Button shopButton, inventoryButton;
    GameObject shopPanel, inventoryPanel;
    bool startInventory = true;

    [MenuItem("Tools/Fantasy UI/Repair Shop Inventory Buttons")]
    static void Open() => GetWindow<ShopInventoryButtonRepairWindow>("Shop/Inventory Repair");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Scans current scene objects, including inactive ones. Does not recreate or move UI. Assign panels explicitly if names are ambiguous.", MessageType.Info);
        if (GUILayout.Button("1. Scan Scene")) Scan();
        shopButton = (Button)EditorGUILayout.ObjectField("SHOP Button", shopButton, typeof(Button), true);
        inventoryButton = (Button)EditorGUILayout.ObjectField("INVENTORY Button", inventoryButton, typeof(Button), true);
        shopPanel = (GameObject)EditorGUILayout.ObjectField("SHOP Panel", shopPanel, typeof(GameObject), true);
        inventoryPanel = (GameObject)EditorGUILayout.ObjectField("INVENTORY Panel", inventoryPanel, typeof(GameObject), true);
        startInventory = EditorGUILayout.Toggle("Start with Inventory", startInventory);
        GUI.enabled = shopButton && inventoryButton && shopPanel && inventoryPanel && shopPanel != inventoryPanel;
        if (GUILayout.Button("2. Repair + Bind Buttons")) Repair();
        GUI.enabled = true;
    }

    static T[] SceneObjects<T>() where T : Component => Resources.FindObjectsOfTypeAll<T>()
        .Where(x => x && x.gameObject.scene.IsValid() && x.gameObject.scene.isLoaded && !EditorUtility.IsPersistent(x)).ToArray();

    static bool Match(string name, string word) => name.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0;

    void Scan()
    {
        var buttons = SceneObjects<Button>();
        shopButton = buttons.FirstOrDefault(b => Match(b.name, "shop") && !Match(b.name, "buy"));
        inventoryButton = buttons.FirstOrDefault(b => Match(b.name, "inventory") || Match(b.name, "bag"));
        var objects = SceneObjects<RectTransform>().Select(t => t.gameObject).Distinct().ToArray();
        shopPanel = objects.FirstOrDefault(o => Match(o.name, "shop") && Match(o.name, "panel"));
        inventoryPanel = objects.FirstOrDefault(o => Match(o.name, "inventory") && Match(o.name, "panel"));
        Debug.Log($"[Tab Repair] Found: SHOP={shopButton}, INVENTORY={inventoryButton}, ShopPanel={shopPanel}, InventoryPanel={inventoryPanel}. Verify before repairing.");
    }

    void Repair()
    {
        if (shopButton == inventoryButton || shopPanel == inventoryPanel) { Debug.LogError("Buttons/panels must be distinct."); return; }
        Undo.RecordObject(shopButton, "Repair SHOP button");
        Undo.RecordObject(inventoryButton, "Repair INVENTORY button");
        Fix(shopButton); Fix(inventoryButton);
        var existing = SceneObjects<ShopInventoryTabRepair>().FirstOrDefault();
        if (!existing)
        {
            var go = new GameObject("ShopInventoryTabRepair");
            SceneManager.MoveGameObjectToScene(go, shopButton.gameObject.scene);
            Undo.RegisterCreatedObjectUndo(go, "Create tab repair controller");
            existing = Undo.AddComponent<ShopInventoryTabRepair>(go);
        }
        Undo.RecordObject(existing, "Bind Shop Inventory tabs");
        existing.shopButton = shopButton;
        existing.inventoryButton = inventoryButton;
        existing.shopPanel = shopPanel;
        existing.inventoryPanel = inventoryPanel;
        existing.startWithInventory = startInventory;
        EditorUtility.SetDirty(existing);
        EditorUtility.SetDirty(shopButton);
        EditorUtility.SetDirty(inventoryButton);
        Debug.Log("[Tab Repair] Bound SHOP/INVENTORY tabs. Save the scene (Ctrl+S), then Play. If another script overwrites buttons, disable that conflicting script.");
    }

    static void Fix(Button button)
    {
        button.interactable = true;
        button.transition = Selectable.Transition.None;
        var graphic = button.targetGraphic;
        if (graphic)
        {
            Undo.RecordObject(graphic, "Repair button graphic");
            graphic.raycastTarget = true;
            graphic.color = Color.white;
            EditorUtility.SetDirty(graphic);
        }
        foreach (var group in button.GetComponentsInParent<CanvasGroup>(true))
        {
            Undo.RecordObject(group, "Repair CanvasGroup interaction");
            group.interactable = true;
            group.blocksRaycasts = true;
            EditorUtility.SetDirty(group);
        }
        var canvas = button.GetComponentInParent<Canvas>(true);
        if (canvas && !canvas.GetComponent<GraphicRaycaster>()) Undo.AddComponent<GraphicRaycaster>(canvas.gameObject);
        if (!SceneObjects<EventSystem>().Any())
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SceneManager.MoveGameObjectToScene(es, button.gameObject.scene);
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }
    }
}
#endif
