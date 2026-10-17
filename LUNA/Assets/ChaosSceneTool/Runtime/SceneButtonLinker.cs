using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChaosCard.SceneTools
{
    public class SceneButtonLinker : MonoBehaviour
    {
        public enum ButtonAction
        {
            LoadScene,
            BackToPreviousScene,
            QuitGame,
            SetObjectActive,
            ToggleObject,
            OpenURL
        }

        [Serializable]
        public class ButtonLink
        {
            public string label = "New Button";
            public Button button;
            public ButtonAction action = ButtonAction.LoadScene;
            public string sceneName = "";
            public GameObject targetObject;
            public bool activeState = true;
            public string url = "";
        }

        [SerializeField] private List<ButtonLink> links = new List<ButtonLink>();
        private static string previousSceneName;

        public List<ButtonLink> Links => links;

        private void Awake()
        {
            foreach (var link in links)
            {
                if (link == null || link.button == null) continue;
                link.button.onClick.RemoveListener(() => Execute(link));
                ButtonLink captured = link;
                link.button.onClick.AddListener(() => Execute(captured));
            }
        }

        public void Execute(ButtonLink link)
        {
            if (link == null) return;
            switch (link.action)
            {
                case ButtonAction.LoadScene:
                    if (!string.IsNullOrWhiteSpace(link.sceneName))
                    {
                        previousSceneName = SceneManager.GetActiveScene().name;
                        SceneManager.LoadScene(link.sceneName);
                    }
                    else Debug.LogWarning($"[SceneButtonLinker] Chưa chọn scene cho nút '{link.label}'.", this);
                    break;
                case ButtonAction.BackToPreviousScene:
                    if (!string.IsNullOrWhiteSpace(previousSceneName)) SceneManager.LoadScene(previousSceneName);
                    else Debug.LogWarning("[SceneButtonLinker] Chưa có scene trước đó để quay lại.", this);
                    break;
                case ButtonAction.QuitGame:
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                    break;
                case ButtonAction.SetObjectActive:
                    if (link.targetObject != null) link.targetObject.SetActive(link.activeState);
                    break;
                case ButtonAction.ToggleObject:
                    if (link.targetObject != null) link.targetObject.SetActive(!link.targetObject.activeSelf);
                    break;
                case ButtonAction.OpenURL:
                    if (!string.IsNullOrWhiteSpace(link.url)) Application.OpenURL(link.url);
                    break;
            }
        }
    }
}
