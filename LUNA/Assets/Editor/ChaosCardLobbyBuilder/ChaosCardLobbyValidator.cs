using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChaosCard.EditorTools
{
    public static class ChaosCardLobbyValidator
    {
        public static List<string> ValidateLobbyScene()
        {
            List<string> issues = new List<string>();

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                issues.Add("Lỗi: Không tìm thấy Canvas trong Scene.");
            }
            else
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    issues.Add("Cảnh báo: CanvasScaler chưa được cấu hình về Scale With Screen Size (1920x1080).");
                }
            }

            UnityEngine.EventSystems.EventSystem es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                issues.Add("Lỗi: Không tìm thấy EventSystem (Các nút UI sẽ không thể click được).");
            }

            GameObject battleBtn = GameObject.Find("Btn_BATTLE");
            if (battleBtn == null)
            {
                issues.Add("Cảnh báo: Thiếu nút 'BATTLE' chính trong Scene.");
            }

            GameObject root = GameObject.Find("[CHAOS_CARD_LOBBY_ROOT]");
            if (root == null)
            {
                issues.Add("Cảnh báo: Không tìm thấy GameObject gốc '[CHAOS_CARD_LOBBY_ROOT]'. Scene này có thể không được tạo từ Tool.");
            }

            return issues;
        }
    }
}