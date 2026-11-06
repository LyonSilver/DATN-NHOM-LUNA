#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Read-only heuristic inspector. Emits warnings into Console, forwarded by Unity Live Bridge.
public static class UnitySceneOverlapInspector
{
    [MenuItem("Tools/Unity Conflict Monitor/Inspect Active Scene UI Overlaps")]
    public static void Inspect()
    {
        var images = UnityEngine.Object.FindObjectsByType<Graphic>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int count = 0;
        for (int i = 0; i < images.Length; i++)
        {
            var a = images[i];
            if (!a.raycastTarget && a.color.a <= 0.01f) continue;
            var ca = a.canvas;
            if (ca == null || a.rectTransform == null) continue;
            for (int j = i + 1; j < images.Length; j++)
            {
                var b = images[j];
                if (b.canvas != ca || b.rectTransform == null || b.color.a <= 0.01f) continue;
                if (a.transform.IsChildOf(b.transform) || b.transform.IsChildOf(a.transform)) continue;
                // Screen-space bounds, heuristic; world-space/rotated objects may be imprecise.
                var ra = Bounds(a.rectTransform);
                var rb = Bounds(b.rectTransform);
                if (!ra.Overlaps(rb)) continue;
                float areaA = ra.width * ra.height;
                float areaB = rb.width * rb.height;
                var intersection = Rect.MinMaxRect(Mathf.Max(ra.xMin, rb.xMin), Mathf.Max(ra.yMin, rb.yMin), Mathf.Min(ra.xMax, rb.xMax), Mathf.Min(ra.yMax, rb.yMax));
                if (areaA <= 0 || areaB <= 0 || intersection.width * intersection.height < Mathf.Min(areaA, areaB) * 0.80f) continue;
                Debug.LogWarning($"[UCM_OVERLAP] Possible overlapping UI: '{Path(a.transform)}' and '{Path(b.transform)}' on Canvas '{ca.name}'. Check sorting, visibility, scene transitions and intentional layering.", a);
                count++;
                if (count >= 100) { Debug.LogWarning("[UCM_OVERLAP] Output capped at 100 candidate pairs."); return; }
            }
        }
        Debug.Log($"[UCM_OVERLAP] Scan complete. {count} possible overlapping UI pairs. These are heuristics, not confirmed errors.");
    }
    static Rect Bounds(RectTransform rt)
    {
        var corners = new Vector3[4]; rt.GetWorldCorners(corners);
        var camera = rt.GetComponentInParent<Canvas>()?.worldCamera;
        for (int i = 0; i < 4; i++) corners[i] = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
        float xmin = Mathf.Min(corners[0].x, corners[2].x), xmax = Mathf.Max(corners[0].x, corners[2].x);
        float ymin = Mathf.Min(corners[0].y, corners[2].y), ymax = Mathf.Max(corners[0].y, corners[2].y);
        return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
    }
    static string Path(Transform t)
    {
        var names = new Stack<string>();
        while (t != null) { names.Push(t.name); t = t.parent; }
        return string.Join("/", names);
    }
}
#endif
