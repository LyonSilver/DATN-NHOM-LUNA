
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class ChaosCardLobbyPolishTool : EditorWindow
{
    private bool styleLeftMenu = true;
    private bool styleRightMenu = true;
    private bool styleBattleButton = true;
    private bool stylePlayerHeader = true;
    private bool addOutlines = true;
    private bool adjustExistingLayouts = true;

    private static readonly Color Navy =
        new Color(0.035f, 0.055f, 0.12f, 0.94f);

    private static readonly Color Gold =
        new Color(0.91f, 0.68f, 0.28f, 1f);

    private static readonly Color White =
        new Color(0.94f, 0.92f, 0.85f, 1f);

    private static readonly Color BattleRed =
        new Color(0.58f, 0.10f, 0.13f, 1f);

    [MenuItem("Chaos Card/UI Polish Tool")]
    public static void OpenWindow()
    {
        GetWindow<ChaosCardLobbyPolishTool>(
            "Chaos Card UI Polish");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "CHAOS CARD",
            EditorStyles.boldLabel);

        EditorGUILayout.LabelField(
            "Lobby UI Polish Tool",
            EditorStyles.largeLabel);

        EditorGUILayout.HelpBox(
            "Tinh chỉnh UI fantasy card theo phong cách " +
            "MOBA. Giữ nguyên GameObject, gameplay và OnClick.",
            MessageType.Info);

        EditorGUILayout.Space(8);

        stylePlayerHeader = EditorGUILayout.ToggleLeft(
            "Player Header", stylePlayerHeader);

        styleLeftMenu = EditorGUILayout.ToggleLeft(
            "Menu bên trái", styleLeftMenu);

        styleRightMenu = EditorGUILayout.ToggleLeft(
            "Menu bên phải", styleRightMenu);

        styleBattleButton = EditorGUILayout.ToggleLeft(
            "Nút Battle", styleBattleButton);

        addOutlines = EditorGUILayout.ToggleLeft(
            "Viền vàng cho Button", addOutlines);

        adjustExistingLayouts = EditorGUILayout.ToggleLeft(
            "Tinh chỉnh Vertical Layout có sẵn",
            adjustExistingLayouts);

        EditorGUILayout.Space(12);

        GUI.backgroundColor = Gold;

        if (GUILayout.Button(
            "APPLY UI POLISH",
            GUILayout.Height(38)))
        {
            ApplyPolish();
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "Hãy lưu scene trước khi áp dụng. " +
            "Tool chỉ chỉnh các component UI hiện có " +
            "và không tự tạo lại hệ thống gameplay.",
            MessageType.Warning);
    }

    private void ApplyPolish()
    {
        Canvas canvas = FindLobbyCanvas();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "Không tìm thấy LobbyCanvas",
                "Hãy mở ChaosCardLobbyScene và kiểm tra " +
                "Hierarchy có LobbyCanvas.",
                "OK");
            return;
        }

        int changed = 0;

        Undo.SetCurrentGroupName("Chaos Card UI Polish");
        int undoGroup = Undo.GetCurrentGroup();

        Transform playerHeader =
            FindChild(canvas.transform, "UI_PlayerHeader");

        Transform leftMenu =
            FindChild(canvas.transform, "UI_LeftMenu");

        Transform rightMenu =
            FindChild(canvas.transform, "UI_RightMenu");

        Transform bottomArea =
            FindChild(canvas.transform, "UI_BottomArea");

        if (stylePlayerHeader && playerHeader != null)
            changed += StylePanel(playerHeader);

        if (styleLeftMenu && leftMenu != null)
            changed += StyleMenu(leftMenu);

        if (styleRightMenu && rightMenu != null)
            changed += StyleMenu(rightMenu);

        if (styleBattleButton && bottomArea != null)
            changed += StyleBattle(bottomArea);

        Undo.CollapseUndoOperations(undoGroup);

        EditorSceneManager.MarkSceneDirty(
            canvas.gameObject.scene);

        Debug.Log(
            $"[Chaos Card UI Polish] Đã tinh chỉnh " +
            $"{changed} thành phần UI. " +
            "Kiểm tra scene trước khi lưu.");

        EditorUtility.DisplayDialog(
            "Chaos Card UI Polish",
            $"Đã tinh chỉnh {changed} thành phần UI.\n\n" +
            "Hãy kiểm tra lại giao diện trong Scene/Game " +
            "rồi lưu scene nếu đã đạt yêu cầu.",
            "OK");
    }

    private Canvas FindLobbyCanvas()
    {
        Canvas[] canvases =
            Resources.FindObjectsOfTypeAll<Canvas>();

        foreach (Canvas canvas in canvases)
        {
            if (!canvas.gameObject.scene.IsValid())
                continue;

            if (canvas.name == "LobbyCanvas")
                return canvas;
        }

        return null;
    }

    private Transform FindChild(
        Transform parent, string targetName)
    {
        if (parent.name == targetName)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindChild(child, targetName);

            if (result != null)
                return result;
        }

        return null;
    }

    private int StylePanel(Transform root)
    {
        int changed = 0;

        Image image = root.GetComponent<Image>();

        if (image != null)
        {
            Undo.RecordObject(image, "Style Player Header");

            image.color = Navy;
            image.raycastTarget = false;

            EditorUtility.SetDirty(image);
            changed++;
        }

        foreach (TMP_Text text in
                 root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name.ToLower().Contains("avatar"))
                continue;

            Undo.RecordObject(text, "Style Player Text");

            text.color = White;

            EditorUtility.SetDirty(text);
            changed++;
        }

        return changed;
    }

    private int StyleMenu(Transform root)
    {
        int changed = 0;

        VerticalLayoutGroup layout =
            root.GetComponent<VerticalLayoutGroup>();

        if (adjustExistingLayouts && layout != null)
        {
            Undo.RecordObject(layout, "Adjust Menu Layout");

            layout.spacing = 9;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            EditorUtility.SetDirty(layout);
            changed++;
        }

        Button[] buttons =
            root.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button.transform == root)
                continue;

            StyleButton(button, false);
            changed++;
        }

        return changed;
    }

    private int StyleBattle(Transform root)
    {
        int changed = 0;

        Button[] buttons =
            root.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            string buttonName =
                button.name.ToLowerInvariant();

            bool isBattle =
                buttonName.Contains("battle") ||
                buttonName.Contains("fight") ||
                buttonName.Contains("ranked");

            if (!isBattle)
                continue;

            StyleButton(button, true);
            changed++;
        }

        return changed;
    }

    private void StyleButton(Button button, bool battle)
    {
        Undo.RecordObject(button, "Style Chaos Card Button");

        Graphic graphic = button.targetGraphic;

        if (graphic == null)
            graphic = button.GetComponent<Image>();

        if (graphic != null)
        {
            Undo.RecordObject(graphic, "Style Button Graphic");

            graphic.color = battle ? BattleRed : Navy;
            graphic.raycastTarget = true;

            if (addOutlines)
            {
                Outline outline =
                    graphic.GetComponent<Outline>();

                if (outline == null)
                {
                    outline = Undo.AddComponent<Outline>(
                        graphic.gameObject);
                }

                Undo.RecordObject(outline, "Button Gold Outline");

                outline.effectColor = battle
                    ? Gold
                    : new Color(0.63f, 0.46f, 0.21f, 0.9f);

                outline.effectDistance = battle
                    ? new Vector2(2f, 2f)
                    : new Vector2(1f, 1f);

                outline.useGraphicAlpha = true;
                EditorUtility.SetDirty(outline);
            }

            EditorUtility.SetDirty(graphic);
        }

        ColorBlock colors = button.colors;

        colors.normalColor = Color.white;
        colors.highlightedColor =
            new Color(1f, 0.84f, 0.52f, 1f);

        colors.pressedColor =
            new Color(0.72f, 0.72f, 0.72f, 1f);

        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor =
            new Color(0.45f, 0.45f, 0.45f, 0.6f);

        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.12f;

        button.colors = colors;

        EditorUtility.SetDirty(button);

        foreach (TMP_Text text in
                 button.GetComponentsInChildren<TMP_Text>(true))
        {
            Undo.RecordObject(text, "Style Button Text");

            text.color = White;
            text.fontStyle = FontStyles.Bold;

            EditorUtility.SetDirty(text);
        }
    }
}
