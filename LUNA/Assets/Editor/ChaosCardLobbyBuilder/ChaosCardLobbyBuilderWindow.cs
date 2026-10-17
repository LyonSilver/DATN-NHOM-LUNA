using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChaosCard.EditorTools
{
    public class ChaosCardLobbyBuilderWindow : EditorWindow
    {
        private ChaosCardLobbyConfig _config;
        private Vector2 _scrollPos;

        [MenuItem("Tools/Chaos Card/Lobby Builder", false, 1)]
        public static void OpenWindow()
        {
            var window = GetWindow<ChaosCardLobbyBuilderWindow>("Chaos Card Lobby Builder");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        private void OnEnable()
        {
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<ChaosCardLobbyConfig>();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("CHAOS CARD - LOBBY BUILDER TOOL", EditorStyles.boldLabel);
            GUILayout.Label("Công cụ tự động khởi tạo và cấu hình Scene Lobby", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("1. Thư mục & Đường dẫn", EditorStyles.boldLabel);
            _config.sceneDirectory = EditorGUILayout.TextField("Scene Directory", _config.sceneDirectory);
            _config.sceneName = EditorGUILayout.TextField("Scene Name", _config.sceneName);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("2. Giao diện & Background", EditorStyles.boldLabel);
            _config.backgroundSprite = (Sprite)EditorGUILayout.ObjectField("Background Sprite", _config.backgroundSprite, typeof(Sprite), false);
            _config.primaryThemeColor = EditorGUILayout.ColorField("Theme Color", _config.primaryThemeColor);
            _config.accentGoldColor = EditorGUILayout.ColorField("Gold Accent Color", _config.accentGoldColor);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("3. Cấu hình Nhân vật & Rồng", EditorStyles.boldLabel);
            
            _config.enableMainCharacter = EditorGUILayout.Toggle("Hiển thị Nhân vật chính", _config.enableMainCharacter);
            if (_config.enableMainCharacter)
            {
                _config.mainCharacterSprite = (Sprite)EditorGUILayout.ObjectField(" Main Char Sprite", _config.mainCharacterSprite, typeof(Sprite), false);
                _config.mainCharacterPrefab = (GameObject)EditorGUILayout.ObjectField(" Main Char Prefab", _config.mainCharacterPrefab, typeof(GameObject), false);
            }

            _config.enableBlueDragon = EditorGUILayout.Toggle("Hiển thị Rồng Xanh (Băng)", _config.enableBlueDragon);
            if (_config.enableBlueDragon)
            {
                _config.blueDragonSprite = (Sprite)EditorGUILayout.ObjectField(" Blue Dragon Sprite", _config.blueDragonSprite, typeof(Sprite), false);
                _config.blueDragonPrefab = (GameObject)EditorGUILayout.ObjectField(" Blue Dragon Prefab", _config.blueDragonPrefab, typeof(GameObject), false);
            }

            _config.enableRedDragon = EditorGUILayout.Toggle("Hiển thị Rồng Lửa (Đỏ Cam)", _config.enableRedDragon);
            if (_config.enableRedDragon)
            {
                _config.redDragonSprite = (Sprite)EditorGUILayout.ObjectField(" Red Dragon Sprite", _config.redDragonSprite, typeof(Sprite), false);
                _config.redDragonPrefab = (GameObject)EditorGUILayout.ObjectField(" Red Dragon Prefab", _config.redDragonPrefab, typeof(GameObject), false);
            }

            EditorGUILayout.Space(15);

            if (GUILayout.Button("1. Preview Configuration", GUILayout.Height(30)))
            {
                Debug.Log($"[Preview] Target: {_config.sceneDirectory}/{_config.sceneName}.unity | Theme: {_config.primaryThemeColor}");
            }

            if (GUILayout.Button("2. Create Lobby Scene", GUILayout.Height(35)))
            {
                ChaosCardLobbySceneGenerator.GenerateScene(_config);
            }

            if (GUILayout.Button("3. Validate Scene", GUILayout.Height(30)))
            {
                List<string> issues = ChaosCardLobbyValidator.ValidateLobbyScene();
                if (issues.Count == 0)
                {
                    EditorUtility.DisplayDialog("Validation Pass", "Scene Lobby hợp lệ và không phát hiện lỗi cấu hình!", "OK");
                }
                else
                {
                    string errorMsg = string.Join("\n", issues);
                    EditorUtility.DisplayDialog("Validation Result", "Phát hiện các vấn đề:\n\n" + errorMsg, "OK");
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}