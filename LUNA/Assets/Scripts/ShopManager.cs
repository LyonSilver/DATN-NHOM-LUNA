using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public bool BuyCard(string cardID, int price)
    {
        // Đọc dữ liệu mới nhất từ file/RAM
        SaveData saveData = SaveSystem.Load();

        if (saveData.coin >= price)
        {
            saveData.coin -= price;

            if (!saveData.unlockedCardIDs.Contains(cardID))
            {
                saveData.unlockedCardIDs.Add(cardID);
            }

            SaveSystem.AutoSave(saveData, $"Mua thành công lá bài {cardID}");
            return true;
        }

        Debug.LogWarning("[ShopManager] Không đủ coin!");
        return false;
    }
}