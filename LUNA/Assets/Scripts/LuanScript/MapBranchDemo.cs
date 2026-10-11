
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

    [Header("Battle Scene")]
    [SerializeField] private string battleSceneName = "Battle";
    [SerializeField] private string mapSceneName = "LevelStageCanvas";

    [Header("Progress Settings")]
    [SerializeField] private bool loadSavedProgress = true;

    private readonly List<Button> buttons = new();
    private readonly List<Image> nodeImages = new();
    private readonly List<List<int>> nextNodes = new();

    private RectTransform pathContainer;
    private RectTransform nodeContainer;

    private int currentNode = -1;

    // Các node đã đi qua.
    private readonly HashSet<int> visitedNodes = new();

    // Các node bị khóa.
    private readonly HashSet<int> lockedNodes = new();

    private const string CurrentNodeKey = "Map_CurrentNode";
    private const string VisitedNodesKey = "Map_VisitedNodes";
    private const string LockedNodesKey = "Map_LockedNodes";
    private const string ProgressExistsKey = "Map_ProgressExists";

    private readonly string[] nodeNames =
    {
        "START", "BATTLE", "?", "BATTLE",
        "$", "ELITE", "?", "REST",
        "BATTLE", "ELITE", "BOSS"
    };

    // Vị trí tương đối trên bản đồ.
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

        // Tạo danh sách các node tiếp theo.
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

        if (loadSavedProgress &&
            PlayerPrefs.GetInt(ProgressExistsKey, 0) == 1)
        {
            LoadProgress();
            RestoreNodeStates();
        }
        else
        {
            // Lần đầu chơi: chỉ mở START.
            currentNode = -1;
            SetNodeAvailable(0, true);
        }

        Debug.Log("Map demo đã sẵn sàng!");
    }

    private void CreateContainers()
    {
        pathContainer = CreateRect("PathContainer", mapArea);
        nodeContainer = CreateRect("NodeContainer", mapArea);

        // Đường nằm sau các nút.
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

            rect.anchorMin = rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = (a + b) / 2f;

            rect.sizeDelta = new Vector2(
                delta.magnitude,
                lineThickness
            );

            rect.localRotation = Quaternion.Euler(
                0,
                0,
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

            rect.anchorMin = rect.anchorMax =
                new Vector2(0.5f, 0.5f);

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
            label.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

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
        if (visitedNodes.Contains(index) ||
            lockedNodes.Contains(index))
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

        // Khóa nhánh thay thế.
        LockAlternativeBranches(index);

        // Lưu tiến trình trước khi chuyển Scene.
        SaveProgress();

        Debug.Log(
            "Đã chọn node: " + nodeNames[index] +
            " (ID " + index + ")"
        );

        // Chọn BATTLE thì vào Scene chiến đấu.
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
        // Tìm các node có cùng node cha.
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
        if (visitedNodes.Contains(index) ||
            lockedNodes.Contains(index))
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

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(CurrentNodeKey, currentNode);

        PlayerPrefs.SetString(
            VisitedNodesKey,
            string.Join(",", visitedNodes)
        );

        PlayerPrefs.SetString(
            LockedNodesKey,
            string.Join(",", lockedNodes)
        );

        PlayerPrefs.SetInt(ProgressExistsKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Đã lưu tiến trình bản đồ!");
    }

    private void LoadProgress()
    {
        currentNode = PlayerPrefs.GetInt(CurrentNodeKey, -1);

        visitedNodes.Clear();
        lockedNodes.Clear();

        LoadNodeSet(VisitedNodesKey, visitedNodes);
        LoadNodeSet(LockedNodesKey, lockedNodes);
    }

    private void LoadNodeSet(string key, HashSet<int> target)
    {
        string savedData = PlayerPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(savedData))
            return;

        string[] values = savedData.Split(',');

        foreach (string value in values)
        {
            if (int.TryParse(value, out int index))
            {
                if (index >= 0 && index < nodeNames.Length)
                    target.Add(index);
            }
        }
    }

    private void RestoreNodeStates()
    {
        // Đặt lại toàn bộ node.
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;

            nodeImages[i].color =
                new Color(0.20f, 0.22f, 0.22f, 1f);
        }

        // Khôi phục node đã đi qua.
        foreach (int index in visitedNodes)
        {
            nodeImages[index].color =
                new Color(0.15f, 0.65f, 0.35f, 1f);
        }

        // Khôi phục node bị khóa.
        foreach (int index in lockedNodes)
        {
            if (visitedNodes.Contains(index))
                continue;

            nodeImages[index].color =
                new Color(0.10f, 0.10f, 0.10f, 1f);
        }

        // Mở các node tiếp theo hợp lệ.
        foreach (int visited in visitedNodes)
        {
            foreach (int next in nextNodes[visited])
            {
                if (visitedNodes.Contains(next) ||
                    lockedNodes.Contains(next))
                    continue;

                SetNodeAvailable(next, true);
            }
        }
    }

    // Gọi hàm này nếu muốn xóa tiến trình bản đồ
    // và bắt đầu lại từ đầu.
    public void ResetMapProgress()
    {
        PlayerPrefs.DeleteKey(CurrentNodeKey);
        PlayerPrefs.DeleteKey(VisitedNodesKey);
        PlayerPrefs.DeleteKey(LockedNodesKey);
        PlayerPrefs.DeleteKey(ProgressExistsKey);
        PlayerPrefs.Save();

        Debug.Log("Đã xóa tiến trình bản đồ.");
    }
}