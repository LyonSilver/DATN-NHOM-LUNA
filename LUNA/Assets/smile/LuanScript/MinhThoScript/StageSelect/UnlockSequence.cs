using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockSequence : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] StageSelectManager manager;
    [SerializeField] PathBuilder2D path;
    [SerializeField] MapCameraController cameraController;
    [SerializeField] NodeSelector selector;
    [SerializeField] StageSelectAudio sfx;

    [Header("Nhịp hiệu ứng (Giây)")]
    [SerializeField] float cameraWait = 0.8f;
    [SerializeField] float starPopTime = 0.3f;
    [SerializeField] float starGap = 0.2f;
    [SerializeField] float pathLightTime = 1.0f;
    [SerializeField] float hopTime = 0.8f;
    [SerializeField] float hopHeight = 1.2f;
    [SerializeField] float focusOffsetX = 1.9f;

    public bool IsPlaying { get; private set; }

    void OnEnable()
    {
        if (manager != null) manager.Refreshed += OnRefreshed;
    }

    void OnDisable()
    {
        if (manager != null) manager.Refreshed -= OnRefreshed;
    }

    void OnRefreshed()
    {
        if (!Application.isPlaying || IsPlaying) return;

        _SaveData save = _SaveSystem.Load();
        if (string.IsNullOrEmpty(save.pendingReveal)) return;

        string id = save.pendingReveal;
        save.pendingReveal = "";
        _SaveSystem.Save();

        Debug.Log("UnlockSequence: chiếu hiệu ứng cho " + id);
        StartCoroutine(Run(id));
    }

    IEnumerator Run(string stageId)
    {
        IsPlaying = true;
        yield return null;   // đợi các script khác xử lý xong Refresh

        StageNode[] nodes = manager.Nodes;
        int i = -1;
        for (int k = 0; k < nodes.Length; k++)
        {
            if (nodes[k] != null && nodes[k].Data != null && nodes[k].Data.stageId == stageId)
            {
                i = k;
                break;
            }
        }
        if (i < 0)
        {
            Finish();
            yield break;
        }

        int finalStars = _SaveSystem.Load().Get(stageId).bestStars;

        // Node mới mở khóa (nếu có) phải nằm ngay sau node vừa thắng
        int j = -1;
        if (i + 1 < nodes.Length && nodes[i + 1].State == NodeState.Current) j = i + 1;

        if (selector != null)
        {
            selector.InputLocked = true;
            selector.HideRing();
        }

        // --- Trạng thái "Trước khi thắng" ---
        Transform[] stars = nodes[i].StarTransforms;
        Vector3[] origScale = new Vector3[stars.Length];
        for (int s = 0; s < stars.Length; s++)
        {
            if (stars[s] == null) continue;
            origScale[s] = stars[s].localScale;
            stars[s].localScale = Vector3.zero;
        }
        if (j >= 0)
        {
            nodes[j].SetState(NodeState.Locked, 0);
            if (path != null) path.ShowUntil(i);
        }

        // --- 1. Camera tới node vừa thắng ---
        if (cameraController != null)
            cameraController.FocusOn(nodes[i].transform.position.x + focusOffsetX);
        yield return new WaitForSeconds(cameraWait);

        // --- 2. Sao bật lên lần lượt ---
        for (int s = 0; s < stars.Length; s++)
        {
            if (stars[s] == null) continue;
            if (s < finalStars && sfx != null) sfx.PlayStar(s);
            yield return Pop(stars[s], origScale[s]);
            yield return new WaitForSeconds(starGap);
        }

        if (j >= 0)
        {
            // --- 3. Chấm sáng dần dọc đường ---
            if (path != null)
            {
                List<int> segDots = new List<int>();
                for (int d = 0; d < path.DotCount; d++)
                    if (path.DotSegmentAt(d) == i) segDots.Add(d);

                float e = 0f;
                int lit = 0;
                while (lit < segDots.Count)
                {
                    e += Time.deltaTime;
                    int should = Mathf.Min(segDots.Count, Mathf.CeilToInt(e / pathLightTime * segDots.Count));
                    while (lit < should)
                    {
                        path.SetDotLit(segDots[lit], true);
                        lit++;
                    }
                    yield return null;
                }
            }

            // --- 4. Node mới bùng sáng ---
            if (sfx != null) sfx.PlayUnlock();
            nodes[j].SetState(NodeState.Current, 0);
            StartCoroutine(Burst(nodes[j]));

            // --- 5. Nhân vật nhảy sang ---
            yield return Hop(nodes[i], nodes[j]);

            // --- 6. Camera tới node mới ---
            if (cameraController != null)
                cameraController.FocusOn(nodes[j].transform.position.x + focusOffsetX);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
        }

        Finish();
    }

    void Finish()
    {
        if (selector != null)
        {
            selector.InputLocked = false;
            selector.Reselect();
        }
        IsPlaying = false;
    }

    IEnumerator Pop(Transform t, Vector3 target)
    {
        float e = 0f;
        while (e < starPopTime)
        {
            e += Time.deltaTime;
            float k = Mathf.Clamp01(e / starPopTime);
            float scale = k < 0.7f
                ? Mathf.Lerp(0f, 1.4f, k / 0.7f)
                : Mathf.Lerp(1.4f, 1f, (k - 0.7f) / 0.3f);
            t.localScale = target * scale;
            yield return null;
        }
        t.localScale = target;
    }

    IEnumerator Hop(StageNode from, StageNode to)
    {
        Transform marker = to.MarkerTransform;
        if (marker == null) yield break;

        Vector3 restLocal = marker.localPosition;
        Vector3 endWorld = to.transform.TransformPoint(restLocal);
        Vector3 startWorld = from.transform.TransformPoint(restLocal);

        to.enabled = false;   // Tạm dừng nhấp nhô để không tranh vị trí
        if (sfx != null) sfx.PlayHop();

        float e = 0f;
        while (e < hopTime)
        {
            e += Time.deltaTime;
            float k = Mathf.Clamp01(e / hopTime);
            Vector3 p = Vector3.Lerp(startWorld, endWorld, k);
            p.y += Mathf.Sin(k * Mathf.PI) * hopHeight;
            marker.position = p;
            yield return null;
        }

        marker.localPosition = restLocal;
        to.enabled = true;
    }

    IEnumerator Burst(StageNode node)
    {
        Sprite spr = node.CircleSprite;
        if (spr == null) yield break;

        for (int r = 0; r < 2; r++)
        {
            GameObject go = new GameObject("Burst");
            go.transform.position = node.transform.position;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingLayerName = "FX";
            sr.sortingOrder = 6;
            StartCoroutine(BurstRing(go.transform, sr, node.transform.lossyScale.x));
            yield return new WaitForSeconds(0.18f);
        }
    }

    IEnumerator BurstRing(Transform t, SpriteRenderer sr, float baseScale)
    {
        const float duration = 0.6f;
        float e = 0f;
        while (e < duration)
        {
            e += Time.deltaTime;
            float k = Mathf.Clamp01(e / duration);
            t.localScale = Vector3.one * Mathf.Lerp(baseScale, baseScale * 2.8f, k);
            sr.color = new Color(0.96f, 0.76f, 0.26f, 0.7f * (1f - k));
            yield return null;
        }
        Destroy(t.gameObject);
    }
}