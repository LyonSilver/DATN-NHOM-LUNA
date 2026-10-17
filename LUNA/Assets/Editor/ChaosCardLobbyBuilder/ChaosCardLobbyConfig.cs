using UnityEngine;

namespace ChaosCard.EditorTools
{
    public class ChaosCardLobbyConfig : ScriptableObject
    {
        [Header("Scene Settings")]
        public string sceneDirectory = "Assets/Scenes";
        public string sceneName = "ChaosCardLobbyScene";

        [Header("Background & Visuals")]
        public Sprite backgroundSprite;
        public Color primaryThemeColor = new Color(0.12f, 0.15f, 0.22f, 0.85f);
        public Color accentGoldColor = new Color(0.95f, 0.75f, 0.25f, 1f);
        public bool createLightingAndParticles = true;

        [Header("Characters")]
        public Sprite mainCharacterSprite;
        public GameObject mainCharacterPrefab;
        public bool enableMainCharacter = true;

        public Sprite blueDragonSprite;
        public GameObject blueDragonPrefab;
        public bool enableBlueDragon = true;

        public Sprite redDragonSprite;
        public GameObject redDragonPrefab;
        public bool enableRedDragon = true;

        [Header("Scene Integration")]
        public string battleTargetSceneName = "ChaosCardBattleScene";
    }
}