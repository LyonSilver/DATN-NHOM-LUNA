#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace QLDATN.ProjectTracker
{
    // File cá nhân hóa: chỉ cần chép vào Assets/Editor.
    // File chỉ chứa mã đăng ký dùng một lần và hết hạn sau 10 phút.
    [InitializeOnLoad]
    public static class QLDATNAutoTrackerSetup
    {
        private const string EnrollmentUrl = "https://datn.unifolio.io.vn/api/project-tracker/enroll";
        private const string EnrollmentCode = "qlde_LpQV0JrpCRjY_wJPCB_JqRy_uZhNovBAF8CnOt1UGsA";
        private const string TrackerUrlPreference = "QLDATN_PROJECT_TRACKER_URL";
        private const string DeviceSecretPreference = "QLDATN_PROJECT_TRACKER_TOKEN";
        private const string DeviceKeyIdPreference = "QLDATN_PROJECT_TRACKER_KEY_ID";
        private const string DeviceIdPreference = "QLDATN_PROJECT_TRACKER_DEVICE_ID";
        private const string EnrollmentCodePreference = "QLDATN_PROJECT_TRACKER_ENROLLMENT_CODE";

        private static readonly string ProjectCredentialsPath = Path.Combine(
            "Library",
            "QLDATNTracker",
            "credentials.json"
        );

        [Serializable]
        private class ProjectCredentials
        {
            public string trackerUrl;
            public string deviceSecret;
            public string deviceKeyId;
            public string deviceId;
            public string enrollmentCode;
        }

        private static string GetProjectPrefKey(string baseKey)
        {
            try
            {
                var projectPath = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    var hashBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(projectPath));
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < 4; i++) sb.Append(hashBytes[i].ToString("X2"));
                    return sb.ToString() + "_" + baseKey;
                }
            }
            catch
            {
                return baseKey;
            }
        }

        static QLDATNAutoTrackerSetup()
        {
            EditorApplication.delayCall += EnrollAndInstall;
        }

        private static void EnrollAndInstall()
        {
            try
            {
                EnsureGitIgnore();
                var deviceId = EditorPrefs.GetString(DeviceIdPreference);
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = Guid.NewGuid().ToString("N");
                    EditorPrefs.SetString(DeviceIdPreference, deviceId);
                }
                EnrollmentResponse enrollment = null;
                try
                {
                    if (File.Exists(ProjectCredentialsPath))
                    {
                        var json = File.ReadAllText(ProjectCredentialsPath);
                        var creds = JsonUtility.FromJson<ProjectCredentials>(json);
                        if (creds != null && creds.enrollmentCode == EnrollmentCode
                            && !string.IsNullOrEmpty(creds.deviceKeyId)
                            && !string.IsNullOrEmpty(creds.deviceSecret)
                            && !string.IsNullOrEmpty(creds.trackerUrl))
                        {
                            enrollment = new EnrollmentResponse
                            {
                                deviceKeyId = creds.deviceKeyId,
                                deviceSecret = creds.deviceSecret,
                                heartbeatEndpoint = creds.trackerUrl
                            };
                        }
                    }
                }
                catch {}

                var scopedCodeKey = GetProjectPrefKey(EnrollmentCodePreference);
                var scopedKeyIdKey = GetProjectPrefKey(DeviceKeyIdPreference);
                if (enrollment == null
                    && EditorPrefs.GetString(scopedCodeKey) == EnrollmentCode
                    && !string.IsNullOrEmpty(EditorPrefs.GetString(scopedKeyIdKey))
                    && !string.IsNullOrEmpty(EditorPrefs.GetString(GetProjectPrefKey(DeviceSecretPreference)))
                    && !string.IsNullOrEmpty(EditorPrefs.GetString(GetProjectPrefKey(TrackerUrlPreference))))
                {
                    enrollment = new EnrollmentResponse
                    {
                        deviceKeyId = EditorPrefs.GetString(scopedKeyIdKey),
                        deviceSecret = EditorPrefs.GetString(GetProjectPrefKey(DeviceSecretPreference)),
                        heartbeatEndpoint = EditorPrefs.GetString(GetProjectPrefKey(TrackerUrlPreference))
                    };
                }

                if (enrollment == null)
                {
                    var enrollmentBody = JsonUtility.ToJson(new EnrollmentRequest
                    {
                        code = EnrollmentCode,
                        deviceId = deviceId,
                        deviceName = SystemInfo.deviceName,
                        platform = "UNITY",
                        clientVersion = "qldatn-unity-installer-2.1.0"
                    });
                    string enrollmentResponseText;
                    using (var client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        enrollmentResponseText = client.UploadString(
                            EnrollmentUrl,
                            "POST",
                            enrollmentBody
                        );
                    }
                    enrollment = JsonUtility.FromJson<EnrollmentResponse>(
                        enrollmentResponseText
                    );
                }
                if (
                    enrollment == null
                    || string.IsNullOrEmpty(enrollment.deviceKeyId)
                    || string.IsNullOrEmpty(enrollment.deviceSecret)
                    || string.IsNullOrEmpty(enrollment.heartbeatEndpoint)
                )
                {
                    throw new Exception("Server không trả về device credential hợp lệ.");
                }

                try
                {
                    var credsDir = Path.GetDirectoryName(ProjectCredentialsPath);
                    if (!string.IsNullOrEmpty(credsDir) && !Directory.Exists(credsDir))
                    {
                        Directory.CreateDirectory(credsDir);
                    }
                    var credsData = new ProjectCredentials
                    {
                        trackerUrl = enrollment.heartbeatEndpoint,
                        deviceSecret = enrollment.deviceSecret,
                        deviceKeyId = enrollment.deviceKeyId,
                        deviceId = deviceId,
                        enrollmentCode = EnrollmentCode
                    };
                    File.WriteAllText(ProjectCredentialsPath, JsonUtility.ToJson(credsData, true));
                }
                catch {}

                EditorPrefs.SetString(GetProjectPrefKey(TrackerUrlPreference), enrollment.heartbeatEndpoint);
                EditorPrefs.SetString(GetProjectPrefKey(DeviceKeyIdPreference), enrollment.deviceKeyId);
                EditorPrefs.SetString(GetProjectPrefKey(DeviceSecretPreference), enrollment.deviceSecret);
                EditorPrefs.SetString(GetProjectPrefKey(EnrollmentCodePreference), EnrollmentCode);
                EditorPrefs.SetString(GetProjectPrefKey(DeviceIdPreference), deviceId);

                EditorPrefs.SetString(TrackerUrlPreference, enrollment.heartbeatEndpoint);
                EditorPrefs.SetString(DeviceKeyIdPreference, enrollment.deviceKeyId);
                EditorPrefs.SetString(DeviceSecretPreference, enrollment.deviceSecret);
                EditorPrefs.SetString(EnrollmentCodePreference, EnrollmentCode);

                var editorDirectory = Path.Combine(Application.dataPath, "Editor");
                Directory.CreateDirectory(editorDirectory);
                var targetPath = Path.Combine(editorDirectory, "QLDATNSceneTracker.cs");
                var downloadPath = targetPath + ".download";
                var origin = new Uri(enrollment.heartbeatEndpoint).GetLeftPart(UriPartial.Authority);
                var sourceUrl = origin
                    + "/api/project-tracker/client-artifact?platform=UNITY&version="
                    + Uri.EscapeDataString("qldatn-unity-3.10.0");
                using (var client = new WebClient())
                {
                    client.DownloadFile(sourceUrl, downloadPath);
                }
                var downloadedSource = File.ReadAllText(downloadPath);
                if (!downloadedSource.Contains("qldatn-unity-3.10.0"))
                {
                    File.Delete(downloadPath);
                    throw new Exception("Server không trả về đúng phiên bản Unity tracker yêu cầu.");
                }
                File.Copy(downloadPath, targetPath, true);
                File.Delete(downloadPath);
                AssetDatabase.ImportAsset(
                    "Assets/Editor/QLDATNSceneTracker.cs",
                    ImportAssetOptions.ForceUpdate
                );
                EditorApplication.delayCall += RemovePersonalizedInstallers;
                Debug.Log("[QLDATN Tracker] Cài đặt hoàn tất. Tracking đã tự khởi động.");
            }
            catch (Exception error)
            {
                Debug.LogError("[QLDATN Tracker] Không thể tự cài đặt: " + error.Message);
            }
        }

        private static void EnsureGitIgnore()
        {
            var projectDirectory = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectDirectory)) return;

            var gitignorePath = Path.Combine(projectDirectory, ".gitignore");
            var current = File.Exists(gitignorePath)
                ? File.ReadAllText(gitignorePath)
                : "";
            var normalized = "\n" + current.Replace("\r\n", "\n").TrimEnd('\n') + "\n";
            var additions = "";
            foreach (var rule in new[]
            {
                "Assets/Editor/QLDATNTracker_*.cs",
                "Assets/Editor/QLDATNTracker_*.cs.meta",
                "Assets/Editor/QLDATNSceneTracker.cs",
                "Assets/Editor/QLDATNSceneTracker.cs.meta",
                "Assets/Editor/QLDATNSceneTracker.cs.download"
            })
            {
                if (normalized.IndexOf("\n" + rule + "\n", StringComparison.Ordinal) < 0)
                {
                    additions += rule + Environment.NewLine;
                }
            }
            if (string.IsNullOrEmpty(additions)) return;

            var prefix = string.IsNullOrEmpty(current)
                || current.EndsWith("\n")
                || current.EndsWith("\r")
                ? ""
                : Environment.NewLine;
            File.AppendAllText(gitignorePath, prefix + additions);
        }

        private static void RemovePersonalizedInstallers()
        {
            var editorDirectory = Path.Combine(Application.dataPath, "Editor");
            foreach (var installerPath in Directory.GetFiles(editorDirectory, "QLDATNTracker_*.cs"))
            {
                File.Delete(installerPath);
                var metaPath = installerPath + ".meta";
                if (File.Exists(metaPath)) File.Delete(metaPath);
            }
            AssetDatabase.Refresh();
        }

        [Serializable]
        private class EnrollmentRequest
        {
            public string code;
            public string deviceId;
            public string deviceName;
            public string platform;
            public string clientVersion;
        }

        [Serializable]
        private class EnrollmentResponse
        {
            public string deviceKeyId;
            public string deviceSecret;
            public string heartbeatEndpoint;
            public string credentialExpiresAt;
        }
    }
}
#endif
