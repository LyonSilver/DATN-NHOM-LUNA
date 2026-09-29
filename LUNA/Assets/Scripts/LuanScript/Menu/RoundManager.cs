using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Round")]
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int maxRound = 2;

    [Header("Enemy")]
    [SerializeField] private CardEffectManager cardEffectManager;

    [Header("UI")]
    [SerializeField] private TMP_Text roundText;


    private void Start()
    {
        UpdateRoundUI();
    }


    public void EnemyDefeated()
    {
        Debug.Log("RoundManager: Enemy đã chết!");

        if (currentRound >= maxRound)
        {
            Debug.Log("RoundManager: Đã hoàn thành Round cuối.");
            return;
        }

        currentRound++;

        Debug.Log(
            "RoundManager: Chuyển sang Round " +
            currentRound +
            "/" +
            maxRound
        );

        if (cardEffectManager != null)
        {
            cardEffectManager.NextRoundEnemyHP();
        }
        else
        {
            Debug.LogError(
                "RoundManager: Chưa gán CardEffectManager!"
            );
        }

        UpdateRoundUI();
    }


    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text =
                "ROUND:\n" +
                currentRound +
                "/" +
                maxRound;
        }
        else
        {
            Debug.LogWarning(
                "RoundManager: Chưa gán Round Text!"
            );
        }
    }
}