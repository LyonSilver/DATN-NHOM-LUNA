using UnityEngine;

public class UnlockSystemDemo : MonoBehaviour
{
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 340, 280), "AI UNLOCK SYSTEM DEMO");

        if (GUI.Button(new Rect(20, 40, 320, 30), "1. Unlock Card (Card_Dragon)"))
        {
            UnlockManager.Instance.Unlock(UnlockType.Card, "Card_Dragon");
        }

        if (GUI.Button(new Rect(20, 80, 320, 30), "2. Unlock Stage (Stage_DarkForest)"))
        {
            UnlockManager.Instance.Unlock(UnlockType.Stage, "Stage_DarkForest");
        }

        if (GUI.Button(new Rect(20, 120, 320, 30), "3. Unlock Boss (Boss_DemonKing)"))
        {
            UnlockManager.Instance.Unlock(UnlockType.Boss, "Boss_DemonKing");
        }

        if (GUI.Button(new Rect(20, 160, 320, 30), "4. Level Up Player to Level 10"))
        {
            if (AIScannerUnlockOverlay.Instance != null)
            {
                AIScannerUnlockOverlay.Instance.SetPlayerLevel(10);
            }
        }

        if (GUI.Button(new Rect(20, 200, 320, 30), "5. Unlock Reward (Reward_Chest)"))
        {
            UnlockManager.Instance.Unlock(UnlockType.Reward, "Reward_Chest");
        }

        if (GUI.Button(new Rect(20, 240, 320, 30), "RESET ALL DATA"))
        {
            UnlockManager.Instance.ClearAll();
            if (AIScannerUnlockOverlay.Instance != null)
            {
                AIScannerUnlockOverlay.Instance.SetPlayerLevel(1);
            }
        }
    }
}