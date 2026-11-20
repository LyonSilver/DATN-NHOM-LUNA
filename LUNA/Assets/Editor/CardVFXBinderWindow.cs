#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class CardVFXBinderWindow : EditorWindow
{
    private CardSkillVFX binder;
    private SerializedObject serializedBinder;
    private ReorderableList mappings;
    private Vector2 scroll;
    private CardData selectedCard;
    private GameObject selectedPrefab;

    [MenuItem("Tools/Player VFX PRO/Card VFX Binder")]
    public static void Open() => GetWindow<CardVFXBinderWindow>("Card VFX Binder");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("CARD + PLAYER VFX BINDER", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Uses the existing CardEffectManager.PlayCardVFX hook. No edits to CardManager or CardEffectManager are needed.", MessageType.Info);
        var newBinder = (CardSkillVFX)EditorGUILayout.ObjectField("CardSkillVFX in Scene", binder, typeof(CardSkillVFX), true);
        if (newBinder != binder) { binder = newBinder; Rebuild(); }
        if (binder == null)
        {
            if (GUILayout.Button("Find CardSkillVFX in Scene"))
            {
                binder = FindFirstObjectByType<CardSkillVFX>();
                Rebuild();
            }
            return;
        }
        if (serializedBinder == null) Rebuild();
        serializedBinder.Update();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.PropertyField(serializedBinder.FindProperty("player"));
        EditorGUILayout.PropertyField(serializedBinder.FindProperty("enemySpawner"));
        EditorGUILayout.PropertyField(serializedBinder.FindProperty("playerVFXAnchor"));
        EditorGUILayout.PropertyField(serializedBinder.FindProperty("enemyVFXAnchor"));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Add Mapping", EditorStyles.boldLabel);
        selectedCard = (CardData)EditorGUILayout.ObjectField("Card", selectedCard, typeof(CardData), false);
        selectedPrefab = (GameObject)EditorGUILayout.ObjectField("VFX Prefab", selectedPrefab, typeof(GameObject), false);
        using (new EditorGUI.DisabledScope(selectedCard == null || selectedPrefab == null))
        {
            if (GUILayout.Button("Add Card VFX Mapping"))
            {
                var arr = serializedBinder.FindProperty("cardEffects");
                int index = arr.arraySize;
                arr.InsertArrayElementAtIndex(index);
                var entry = arr.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("card").objectReferenceValue = selectedCard;
                entry.FindPropertyRelative("vfxPrefab").objectReferenceValue = selectedPrefab;
                entry.FindPropertyRelative("target").enumValueIndex = 0;
                entry.FindPropertyRelative("offset").vector3Value = new Vector3(0, 1, 0);
                entry.FindPropertyRelative("lifetime").floatValue = 1.5f;
                entry.FindPropertyRelative("repeatCount").intValue = 1;
                entry.FindPropertyRelative("repeatDelay").floatValue = 0.2f;
                entry.FindPropertyRelative("chance").floatValue = 100f;
                entry.FindPropertyRelative("startDelay").floatValue = 0f;
                entry.FindPropertyRelative("followTarget").boolValue = true;
                entry.FindPropertyRelative("particlesFollowTarget").boolValue = true;
                entry.FindPropertyRelative("scaleMultiplier").vector3Value = Vector3.one;
                entry.FindPropertyRelative("overrideSorting").boolValue = false;
                entry.FindPropertyRelative("sortingLayerName").stringValue = "Default";
                entry.FindPropertyRelative("sortingOrder").intValue = 10;
                entry.FindPropertyRelative("flipWithTarget").boolValue = false;
                serializedBinder.ApplyModifiedProperties();
                Rebuild();
            }
        }
        EditorGUILayout.Space();
        mappings.DoLayoutList();
        EditorGUILayout.EndScrollView();
        serializedBinder.ApplyModifiedProperties();
        if (GUILayout.Button("Select Binder in Hierarchy")) Selection.activeObject = binder.gameObject;
        if (Application.isPlaying && selectedCard != null && GUILayout.Button("TEST Selected Card VFX"))
            binder.PlayCardVFX(selectedCard);
    }

    private void Rebuild()
    {
        if (binder == null) { serializedBinder = null; mappings = null; return; }
        serializedBinder = new SerializedObject(binder);
        mappings = new ReorderableList(serializedBinder, serializedBinder.FindProperty("cardEffects"), true, true, true, true);
        mappings.drawHeaderCallback = r => EditorGUI.LabelField(r, "Card VFX Cues (multiple entries per Card allowed)");
        mappings.elementHeightCallback = i => EditorGUI.GetPropertyHeight(mappings.serializedProperty.GetArrayElementAtIndex(i), true) + 5;
        mappings.drawElementCallback = (r, i, active, focused) =>
        {
            var element = mappings.serializedProperty.GetArrayElementAtIndex(i);
            EditorGUI.PropertyField(new Rect(r.x, r.y + 2, r.width, EditorGUI.GetPropertyHeight(element, true)), element, true);
        };
    }
}
#endif
