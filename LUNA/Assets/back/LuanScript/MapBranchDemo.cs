
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapBranchDemo : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private RectTransform mapArea;

    [Header("Demo Settings")]
    [SerializeField] private Vector2 nodeSize = new Vector2(76, 58);
    [SerializeField] private float lineThickness = 5f;

    [Header("Scene Settings")]
    [SerializeField] private string battleSceneName = "Battle";
    [SerializeField] private string mapSceneName = "LevelStage";

    private readonly List<Button> buttons = new();
    private readonly List<Image> nodeImages = new();
    private readonly List<List<int>> nextNodes = new();

    private RectTransform pathContainer;
    private RectTransform nodeContainer;

    // Tiến trình chỉ tồn tại trong phiên chơi hiện tại.
    // Static giúp giữ dữ liệu khi chuyển Scene.
    private static int currentNode = -1;
    private static readonly HashSet<int> visitedNodes = new();
    private static readonly HashSet<int> lockedNodes = new();

    // Tự động xóa tiến trình khi bắt đầu một phiên Play mới.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionProgress()
    {
        currentNode = -1;
        visitedNodes.Clear();
        lockedNodes.Clear();
    }

    private readonly string[] nodeNames =
    {
        "START", "BATTLE", "?", "BATTLE",
        "$", "ELITE", "?", "REST",
        "BATTLE", "ELITE", "BOSS"
    };

    // Vị trí tương đối của từng node trên bản đồ.
    private readonly Vector2[] positions =
    {
        new Vector2(0.08f, 0.48f),
        new Vector2(0.20f, 0.67f),
        new Vector2(0.20f, 0.29f),
        new Vector2(0.34f, 0.48f),
        new Vector2(0.46f, 0.67f),
        new Vector2(0.46f, 0.29f),
        new Vector2(0.59f, 0.48f),
        new Vector2(0.70f, 0.67f),
        new Vector2(0.70f, 0.29f),
        new Vector2(0.82f, 0.48f),
        new Vector2(0.94f, 0.48f)
    };

    private readonly int[,] connections =
    {
        { 0, 1 }, { 0, 2 },
        { 1, 3 }, { 2, 3 },
        { 3, 4 }, { 3, 5 },
        { 4, 6 }, { 5, 6 },
        { 6, 7 }, { 6, 8 },
        { 7, 9 }, { 8, 9 },
        { 9, 10 }
    };

    private void Start()
    {
        if (mapArea == null)
            mapArea = transform as RectTransform;

        if (mapArea == null)
        {
            Debug.LogError(
                "MapBranchDemo cần RectTransform của Canvas hoặc khu vực bản đồ."
            );
            return;
        }

        // Xây dựng lại danh sách kết nối cho Scene bản đồ mới.
        nextNodes.Clear();

        for (int i = 0; i < nodeNames.Length; i++)
            nextNodes.Add(new List<int>());

        for (int i = 0; i < connections.GetLength(0); i++)
        {
            int from = connections[i, 0];
            int to = connections[i, 1];

            nextNodes[from].Add(to);
        }

        CreateContainers();
        CreatePaths();
        CreateNodes();

        // Có tiến trình trong phiên chơi thì khôi phục.
        // Nếu chưa đi node nào thì chỉ mở START.
        if (visitedNodes.Count > 0)
        {
            RestoreNodeStates();
        }
        else
        {
            currentNode = -1;
            SetNodeAvailable(0, true);
        }

        Debug.Log("Map demo đã sẵn sàng!");
    }

    private void CreateContainers()
    {
        pathContainer = CreateRect("PathContainer", mapArea);
        nodeContainer = CreateRect("NodeContainer", mapArea);

        // Đường nằm phía sau các node.
        pathContainer.SetAsLastSibling();
        nodeContainer.SetAsLastSibling();
    }

    private RectTransform CreateRect(
        string objectName,
        RectTransform parent)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return rect;
    }

    private Vector2 GetPosition(int index)
    {
        Rect rect = mapArea.rect;

        return new Vector2(
            (positions[index].x - 0.5f) * rect.width,
            (positions[index].y - 0.5f) * rect.height
        );
    }

    private void CreatePaths()
    {
        for (int i = 0; i < connections.GetLength(0); i++)
        {
            int from = connections[i, 0];
            int to = connections[i, 1];

            GameObject obj = new GameObject(
                "Path_" + from + "_" + to,
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(pathContainer, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            Image image = obj.GetComponent<Image>();

            Vector2 a = GetPosition(from);
            Vector2 b = GetPosition(to);
            Vector2 delta = b - a;

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = (a + b) / 2f;

            rect.sizeDelta = new Vector2(
                delta.magnitude,
                lineThickness
            );

            rect.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg
            );

            image.color = new Color(0.25f, 0.28f, 0.27f, 0.9f);
            image.raycastTarget = false;
        }
    }

    private void CreateNodes()
    {
        for (int i = 0; i < positions.Length; i++)
        {
            int index = i;

            GameObject obj = new GameObject(
                "Node_" + i,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

            obj.transform.SetParent(nodeContainer, false);

            RectTransform rect = obj.GetComponent<RectTransform>();

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = GetPosition(i);
            rect.sizeDelta = nodeSize;

            Image image = obj.GetComponent<Image>();
            image.color = new Color(0.20f, 0.22f, 0.22f, 1f);

            Button button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            button.interactable = false;
            button.onClick.AddListener(() => SelectNode(index));

            GameObject labelObj = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(Text)
            );

            labelObj.transform.SetParent(obj.transform, false);

            RectTransform labelRect =
                labelObj.GetComponent<RectTransform>();

            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObj.GetComponent<Text>();
            label.text = nodeNames[i];
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 13;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            buttons.Add(button);
            nodeImages.Add(image);
        }
    }

    private void SelectNode(int index)
    {
        if (index < 0 || index >= buttons.Count)
            return;

        if (visitedNodes.Contains(index) || lockedNodes.Contains(index))
            return;

        if (!buttons[index].interactable)
            return;

        currentNode = index;
        visitedNodes.Add(index);

        // Đổi màu node đã đi qua.
        nodeImages[index].color =
            new Color(0.15f, 0.65f, 0.35f, 1f);

        buttons[index].interactable = false;

        // Mở các node tiếp theo.
        foreach (int next in nextNodes[index])
        {
            if (!visitedNodes.Contains(next) &&
                !lockedNodes.Contains(next))
            {
                SetNodeAvailable(next, true);
            }
        }

        // Khóa các nhánh thay thế cùng node cha.
        LockAlternativeBranches(index);

        Debug.Log(
            "Đã chọn node: " + nodeNames[index] +
            " (ID " + index + ")"
        );

        // Chọn BATTLE thì chuyển sang Scene chiến đấu.
        if (nodeNames[index] == "BATTLE")
        {
            SceneManager.LoadScene(battleSceneName);
            return;
        }

        if (index == 10)
            Debug.Log("Đã đến BOSS!");
    }

    private void LockAlternativeBranches(int selectedIndex)
    {
        // Tìm các node có cùng node cha với node được chọn.
        for (int parent = 0; parent < nextNodes.Count; parent++)
        {
            if (!nextNodes[parent].Contains(selectedIndex))
                continue;

            foreach (int sibling in nextNodes[parent])
            {
                if (sibling == selectedIndex)
                    continue;

                if (visitedNodes.Contains(sibling))
                    continue;

                lockedNodes.Add(sibling);

                buttons[sibling].interactable = false;

                nodeImages[sibling].color =
                    new Color(0.10f, 0.10f, 0.10f, 1f);

                Debug.Log(
                    "Đã khóa nhánh: " + nodeNames[sibling] +
                    " (ID " + sibling + ")"
                );
            }
        }
    }

    private void SetNodeAvailable(int index, bool available)
    {
        if (index < 0 || index >= buttons.Count)
            return;

        if (visitedNodes.Contains(index) || lockedNodes.Contains(index))
        {
            buttons[index].interactable = false;
            return;
        }

        buttons[index].interactable = available;

        if (available)
        {
            nodeImages[index].color =
                new Color(0.15f, 0.48f, 0.65f, 1f);
        }
        else
        {
            nodeImages[index].color =
                new Color(0.20f, 0.22f, 0.22f, 1f);
        }
    }

    private void RestoreNodeStates()
    {
        // Đặt tất cả node về trạng thái mặc định trước.
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;

            nodeImages[i].color =
                new Color(0.20f, 0.22f, 0.22f, 1f);
        }

        // Khôi phục node đã đi qua.
        foreach (int index in visitedNodes)
        {
            if (index < 0 || index >= buttons.Count)
                continue;

            nodeImages[index].color =
                new Color(0.15f, 0.65f, 0.35f, 1f);

            buttons[index].interactable = false;
        }

        // Khôi phục node bị khóa.
        foreach (int index in lockedNodes)
        {
            if (index < 0 || index >= buttons.Count)
                continue;

            if (visitedNodes.Contains(index))
                continue;

            nodeImages[index].color =
                new Color(0.10f, 0.10f, 0.10f, 1f);

            buttons[index].interactable = false;
        }

        // Mở các node tiếp theo chưa đi và chưa bị khóa.
        foreach (int visited in visitedNodes)
        {
            if (visited < 0 || visited >= nextNodes.Count)
                continue;

            foreach (int next in nextNodes[visited])
            {
                if (visitedNodes.Contains(next) ||
                    lockedNodes.Contains(next))
                {
                    continue;
                }

                SetNodeAvailable(next, true);
            }
        }
    }

    // Có thể gọi hàm này từ nút quay về bản đồ nếu cần.
    public void ReturnToMap()
    {
        SceneManager.LoadScene(mapSceneName);
    }

    // Dùng khi muốn chủ động bắt đầu lại bản đồ trong phiên chơi.
    public void ResetMapProgress()
    {
        currentNode = -1;
        visitedNodes.Clear();
        lockedNodes.Clear();

        SceneManager.LoadScene(mapSceneName);
    }
}
