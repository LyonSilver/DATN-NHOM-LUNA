#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class VFXFrameBuilderWindow : EditorWindow
{
    Texture2D sheet;
    int columns = 5, rows = 3, fps = 15, pixelsPerUnit = 100;
    bool loop, autoDestroy = true, skipTransparent;
    string outputFolder = "Assets/GeneratedVFX";
    int previewFrame;
    double lastPreview;
    bool playing;
    Texture2D previewTexture;
    string previewKey;

    [MenuItem("Tools/2D VFX/Frame Builder PRO v2")]
    static void Open() => GetWindow<VFXFrameBuilderWindow>("VFX Frame Builder PRO v2");

    void OnDisable() { if (previewTexture) DestroyImmediate(previewTexture); }

    void OnGUI()
    {
        EditorGUILayout.LabelField("SPRITE SHEET → ANIMATION + PREFAB", EditorStyles.boldLabel);
        sheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet", sheet, typeof(Texture2D), false);
        columns = Mathf.Max(1, EditorGUILayout.IntField("Columns", columns));
        rows = Mathf.Max(1, EditorGUILayout.IntField("Rows", rows));
        fps = Mathf.Clamp(EditorGUILayout.IntField("FPS", fps), 1, 120);
        pixelsPerUnit = Mathf.Max(1, EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit));
        loop = EditorGUILayout.Toggle("Loop", loop);
        autoDestroy = EditorGUILayout.Toggle("Auto Destroy (non-loop)", autoDestroy);
        skipTransparent = EditorGUILayout.Toggle("Skip fully transparent cells", skipTransparent);
        outputFolder = EditorGUILayout.TextField("Output folder (Assets/...)", outputFolder);
        EditorGUILayout.HelpBox("Works with uneven image sizes too. Frame order: left to right, top to bottom. Original texture is not modified. For this uploaded image: 5 columns × 3 rows.", MessageType.Info);
        if (!sheet) return;
        EditorGUILayout.LabelField($"Source: {sheet.width} × {sheet.height} | Grid: {columns * rows} cells");
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(playing ? "Pause Preview" : "Play Preview")) { playing = !playing; lastPreview = EditorApplication.timeSinceStartup; }
            if (GUILayout.Button("Previous")) { playing = false; previewFrame = (previewFrame - 1 + columns * rows) % (columns * rows); }
            if (GUILayout.Button("Next")) { playing = false; previewFrame = (previewFrame + 1) % (columns * rows); }
        }
        previewFrame = EditorGUILayout.IntSlider("Frame", previewFrame, 0, columns * rows - 1);
        DrawPreview();
        GUI.enabled = sheet && outputFolder.StartsWith("Assets/");
        if (GUILayout.Button("BUILD FRAMES + ANIMATION + PREFAB", GUILayout.Height(38))) Build();
        GUI.enabled = true;
    }

    void Update()
    {
        if (!playing || !sheet) return;
        double now = EditorApplication.timeSinceStartup;
        if (now - lastPreview >= 1.0 / fps)
        {
            previewFrame = (previewFrame + 1) % (columns * rows);
            lastPreview = now;
            Repaint();
        }
    }

    RectInt CellRect(int index, int width, int height)
    {
        int c = index % columns;
        int r = index / columns; // top-down
        int x0 = Mathf.RoundToInt(c * width / (float)columns);
        int x1 = Mathf.RoundToInt((c + 1) * width / (float)columns);
        int top0 = Mathf.RoundToInt(r * height / (float)rows);
        int top1 = Mathf.RoundToInt((r + 1) * height / (float)rows);
        return new RectInt(x0, height - top1, x1 - x0, top1 - top0);
    }

    Texture2D ReadableCopy()
    {
        var rt = RenderTexture.GetTemporary(sheet.width, sheet.height, 0, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;
        try
        {
            Graphics.Blit(sheet, rt);
            RenderTexture.active = rt;
            var copy = new Texture2D(sheet.width, sheet.height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, sheet.width, sheet.height), 0, 0);
            copy.Apply();
            return copy;
        }
        finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(rt); }
    }

    void DrawPreview()
    {
        Rect area = GUILayoutUtility.GetRect(200, 260, GUILayout.ExpandWidth(true));
        if (Event.current.type != EventType.Repaint) return;
        string key = sheet.GetInstanceID() + ":" + columns + ":" + rows + ":" + previewFrame;
        if (previewTexture == null || previewKey != key)
        {
            if (previewTexture) DestroyImmediate(previewTexture);
            var src = ReadableCopy();
            RectInt rect = CellRect(previewFrame, src.width, src.height);
            previewTexture = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
            previewTexture.SetPixels(src.GetPixels(rect.x, rect.y, rect.width, rect.height));
            previewTexture.Apply();
            DestroyImmediate(src);
            previewKey = key;
        }
        if (previewTexture)
        {
            float side = Mathf.Min(area.width, area.height);
            var dest = new Rect(area.x + (area.width - side) / 2, area.y + (area.height - side) / 2, side, side);
            EditorGUI.DrawTextureTransparent(dest, previewTexture, ScaleMode.ScaleToFit);
        }
    }

    void Build()
    {
        if (!sheet || columns * rows > 2048) { EditorUtility.DisplayDialog("Invalid", "Choose a sheet and grid up to 2048 cells.", "OK"); return; }
        if (!outputFolder.StartsWith("Assets/") || outputFolder.Contains("..")) { EditorUtility.DisplayDialog("Invalid path", "Output must be inside Assets/.", "OK"); return; }
        var src = ReadableCopy();
        string safeName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sheet));
        string folder = outputFolder.TrimEnd('/') + "/" + safeName;
        try
        {
            Directory.CreateDirectory(folder);
            string framesFolder = folder + "/Frames";
            Directory.CreateDirectory(framesFolder);
            var paths = new System.Collections.Generic.List<string>();
            for (int i = 0; i < columns * rows; i++)
            {
                RectInt rect = CellRect(i, src.width, src.height);
                if (rect.width <= 0 || rect.height <= 0) continue;
                var pixels = src.GetPixels(rect.x, rect.y, rect.width, rect.height);
                if (skipTransparent && Array.TrueForAll(pixels, p => p.a <= 0.001f)) continue;
                var frame = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
                frame.SetPixels(pixels);
                frame.Apply();
                string path = framesFolder + "/frame_" + i.ToString("D3") + ".png";
                File.WriteAllBytes(path, frame.EncodeToPNG());
                DestroyImmediate(frame);
                paths.Add(path);
            }
            AssetDatabase.Refresh();
            var sprites = new System.Collections.Generic.List<Sprite>();
            foreach (var path in paths)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(path));
            }
            if (sprites.Count == 0) throw new Exception("No nonempty frames were found.");
            string clipPath = folder + "/" + safeName + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (!clip) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
            clip.frameRate = fps;
            var binding = new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" };
            var keys = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++) keys[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = sprites[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            string controllerPath = folder + "/" + safeName + ".controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (!controller) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            AnimatorState state = null;
            foreach (var child in machine.states) if (child.state.name == "PlayVFX") { state = child.state; break; }
            if (!state) state = machine.AddState("PlayVFX");
            state.motion = clip;
            machine.defaultState = state;
            EditorUtility.SetDirty(controller);
            var go = new GameObject(safeName + "_VFX");
            try
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprites[0];
                var animator = go.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                if (autoDestroy && !loop)
                {
                    var lifetime = go.AddComponent<VFXLifetime>();
                    lifetime.seconds = sprites.Count / (float)fps;
                }
                string prefabPath = folder + "/" + safeName + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
                EditorUtility.DisplayDialog("VFX created", $"Created {sprites.Count} frames, Animation, Animator Controller and Prefab in:\n{folder}", "OK");
            }
            finally { DestroyImmediate(go); }
        }
        catch (Exception e) { Debug.LogException(e); EditorUtility.DisplayDialog("Build failed", e.Message, "OK"); }
        finally { DestroyImmediate(src); }
    }
}
#endif
