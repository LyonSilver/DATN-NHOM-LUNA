#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChaosCardSceneGuardian : EditorWindow
{
    private Vector2 scroll;
    private string lastMessage = "Chưa thực hiện thao tác nào.";
    private MessageType lastMessageType = MessageType.Info;

    [MenuItem("Chaos Card/Scene Guardian - Save & Backup")]
    public static void OpenWindow()
    {
        GetWindow<ChaosCardSceneGuardian>("Scene Guardian");
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Chaos Card - Scene Guardian", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Tool ưu tiên bảo toàn dữ liệu: không tạo lại UI, không xóa GameObject, không sửa script và không tự thay đổi cấu hình scene. Các thao tác chỉ lưu, sao lưu hoặc kiểm tra.",
            MessageType.Info);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("1. Save", EditorStyles.boldLabel);
        if (GUILayout.Button("Save All Open Scenes", GUILayout.Height(32)))
            SaveAllScenes();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("2. Backup", EditorStyles.boldLabel);
        if (GUILayout.Button("Backup Active Scene", GUILayout.Height(32)))
            BackupActiveScene();

        if (GUILayout.Button("Create Project Backup ZIP", GUILayout.Height(36)))
            CreateProjectBackupZip();

        EditorGUILayout.HelpBox(
            "ZIP gồm Assets, Packages, ProjectSettings và một số file cấu hình Git ở thư mục gốc. Library, Temp, Obj và .git được bỏ qua.",
            MessageType.None);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("3. Check only (không sửa gì)", EditorStyles.boldLabel);
        if (GUILayout.Button("Scan Active Scene for Missing Scripts", GUILayout.Height(32)))
            ScanActiveScene();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Kết quả gần nhất", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lastMessage, lastMessageType);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Lưu ý", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "• Backup được tạo trong thư mục ChaosCard_Backups nằm cạnh thư mục project.\n" +
            "• ZIP có thể khá lớn nếu project chứa nhiều asset.\n" +
            "• Để chia sẻ qua GitHub, vẫn cần commit/push các thay đổi cần thiết.",
            EditorStyles.wordWrappedLabel);

        EditorGUILayout.EndScrollView();
    }

    private void SetStatus(string message, MessageType type = MessageType.Info)
    {
        lastMessage = message;
        lastMessageType = type;
        Repaint();
    }

    private bool SaveAllScenesWithPrompt()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            SetStatus("Đã hủy thao tác. Scene chưa được lưu hoặc người dùng chọn Cancel.", MessageType.Warning);
            return false;
        }

        if (!EditorSceneManager.SaveOpenScenes())
        {
            SetStatus("Unity không xác nhận lưu thành công tất cả scene đang mở.", MessageType.Warning);
            return false;
        }

        return true;
    }

    private void SaveAllScenes()
    {
        if (!SaveAllScenesWithPrompt())
            return;

        AssetDatabase.SaveAssets();
        SetStatus("Đã lưu các scene đang mở và các asset. Tool không thay đổi nội dung scene.");
        Debug.Log("[Chaos Card Scene Guardian] Saved all open scenes and assets.");
    }

    private void BackupActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || string.IsNullOrEmpty(scene.path) || !File.Exists(scene.path))
        {
            SetStatus("Scene hiện tại chưa được lưu thành file .unity. Hãy dùng File > Save As trước.", MessageType.Warning);
            return;
        }

        if (!SaveAllScenesWithPrompt())
            return;

        try
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string backupFolder = Path.Combine(Directory.GetParent(projectRoot).FullName, "ChaosCard_Backups");
            Directory.CreateDirectory(backupFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string sceneFileName = Path.GetFileNameWithoutExtension(scene.path);
            string backupPath = Path.Combine(backupFolder, sceneFileName + "_" + timestamp + ".unity");

            File.Copy(scene.path, backupPath, false);

            string sourceMeta = scene.path + ".meta";
            if (File.Exists(sourceMeta))
                File.Copy(sourceMeta, backupPath + ".meta", false);

            SetStatus("Đã sao lưu scene:\n" + scene.path + "\n\nBản backup:\n" + backupPath);
            Debug.Log("[Chaos Card Scene Guardian] Scene backup created: " + backupPath);
            EditorUtility.RevealInFinder(backupPath);
        }
        catch (Exception ex)
        {
            SetStatus("Không thể tạo backup scene: " + ex.Message, MessageType.Error);
            Debug.LogException(ex);
        }
    }

    private void CreateProjectBackupZip()
    {
        if (!SaveAllScenesWithPrompt())
            return;

        AssetDatabase.SaveAssets();

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string backupRoot = Path.Combine(Directory.GetParent(projectRoot).FullName, "ChaosCard_Backups");
        Directory.CreateDirectory(backupRoot);

        string projectName = new DirectoryInfo(projectRoot).Name;
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string zipPath = Path.Combine(backupRoot, projectName + "_Backup_" + timestamp + ".zip");

        string[] foldersToInclude = { "Assets", "Packages", "ProjectSettings" };
        string[] filesToInclude =
        {
            ".gitignore", ".gitattributes", ".gitmodules",
            "README.md", "README.txt", "LICENSE"
        };

        bool zipCompleted = false;

        try
        {
            int totalFiles = CountFiles(projectRoot, foldersToInclude, filesToInclude);
            int processed = 0;

            using (FileStream zipStream = new FileStream(zipPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (string folderName in foldersToInclude)
                {
                    string fullFolder = Path.Combine(projectRoot, folderName);
                    if (!Directory.Exists(fullFolder))
                        continue;

                    foreach (string file in Directory.GetFiles(fullFolder, "*", SearchOption.AllDirectories))
                    {
                        bool cancel = EditorUtility.DisplayCancelableProgressBar(
                            "Chaos Card Project Backup",
                            "Đang sao lưu: " + GetRelativePath(projectRoot, file),
                            totalFiles == 0 ? 1f : (float)processed / totalFiles);

                        if (cancel)
                            throw new OperationCanceledException("Người dùng đã hủy tạo backup ZIP.");

                        string relativePath = GetRelativePath(projectRoot, file);
                        archive.CreateEntryFromFile(
                            file,
                            relativePath,
                            System.IO.Compression.CompressionLevel.Optimal);

                        processed++;
                    }
                }

                foreach (string fileName in filesToInclude)
                {
                    string fullPath = Path.Combine(projectRoot, fileName);
                    if (!File.Exists(fullPath))
                        continue;

                    bool cancel = EditorUtility.DisplayCancelableProgressBar(
                        "Chaos Card Project Backup",
                        "Đang sao lưu: " + fileName,
                        totalFiles == 0 ? 1f : (float)processed / totalFiles);

                    if (cancel)
                        throw new OperationCanceledException("Người dùng đã hủy tạo backup ZIP.");

                    archive.CreateEntryFromFile(
                        fullPath,
                        fileName,
                        System.IO.Compression.CompressionLevel.Optimal);

                    processed++;
                }
            }

            zipCompleted = true;
            SetStatus("Đã tạo project backup ZIP:\n" + zipPath);
            Debug.Log("[Chaos Card Scene Guardian] Project backup created: " + zipPath);
            EditorUtility.RevealInFinder(zipPath);
        }
        catch (OperationCanceledException ex)
        {
            SetStatus("Đã hủy tạo ZIP. File ZIP chưa hoàn chỉnh sẽ được xóa; project gốc không bị thay đổi.", MessageType.Warning);
            Debug.LogWarning("[Chaos Card Scene Guardian] " + ex.Message);
        }
        catch (Exception ex)
        {
            SetStatus("Không thể tạo project backup ZIP: " + ex.Message, MessageType.Error);
            Debug.LogException(ex);
        }
        finally
        {
            EditorUtility.ClearProgressBar();

            if (!zipCompleted && File.Exists(zipPath))
            {
                try
                {
                    File.Delete(zipPath);
                }
                catch (Exception deleteException)
                {
                    Debug.LogWarning("[Chaos Card Scene Guardian] Không xóa được ZIP chưa hoàn chỉnh: " + deleteException.Message);
                }
            }
        }
    }

    private int CountFiles(string projectRoot, string[] folderNames, string[] fileNames)
    {
        int count = 0;

        foreach (string folderName in folderNames)
        {
            string fullFolder = Path.Combine(projectRoot, folderName);
            if (Directory.Exists(fullFolder))
                count += Directory.GetFiles(fullFolder, "*", SearchOption.AllDirectories).Length;
        }

        foreach (string fileName in fileNames)
        {
            if (File.Exists(Path.Combine(projectRoot, fileName)))
                count++;
        }

        return count;
    }

    private string GetRelativePath(string root, string path)
    {
        string normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        Uri rootUri = new Uri(normalizedRoot);
        Uri fileUri = new Uri(path);
        return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString())
            .Replace('/', Path.DirectorySeparatorChar);
    }

    private void ScanActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            SetStatus("Không có active scene hợp lệ để kiểm tra.", MessageType.Warning);
            return;
        }

        int missingCount = 0;
        int componentCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            MonoBehaviour[] components = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour component in components)
            {
                componentCount++;
                if (component == null)
                {
                    missingCount++;
                    Debug.LogWarning(
                        "[Chaos Card Scene Guardian] Missing script found under: " + GetHierarchyPath(root.transform),
                        root);
                }
            }
        }

        if (missingCount == 0)
        {
            SetStatus("Không phát hiện Missing Script trong active scene. Đã kiểm tra " +
                      componentCount + " vị trí MonoBehaviour. Tool chỉ kiểm tra, không tự sửa.");
        }
        else
        {
            SetStatus("Phát hiện " + missingCount +
                      " Missing Script. Xem Console để xác định GameObject. Tool không tự xóa component.",
                MessageType.Warning);
        }

        Debug.Log("[Chaos Card Scene Guardian] Scan complete. Missing scripts: " + missingCount);
    }

    private string GetHierarchyPath(Transform target)
    {
        if (target == null)
            return "(unknown)";

        string path = target.name;
        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}
#endif
