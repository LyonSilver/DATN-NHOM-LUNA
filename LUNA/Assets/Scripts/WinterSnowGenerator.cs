using UnityEngine;

namespace WinterScene
{
    /// <summary>
    /// Tự động tạo hiệu ứng tuyết rơi 2D mượt mà 60FPS phủ lên ảnh nền mà không tốn RAM.
    /// Chỉ cần gắn script này vào một GameObject trống trong Scene (hoặc Canvas).
    /// </summary>
    public class WinterSnowGenerator : MonoBehaviour
    {
        [Header("Cài đặt Tuyết Rơi")]
        public Texture2D snowflakeTexture;
        public int maxParticles = 350;
        public float fallSpeed = 1.8f;
        public float windStrength = 0.6f;
        public Vector2 spawnAreaSize = new Vector2(20f, 12f);

        private ParticleSystem snowPS;

        void Start()
        {
            SetupSnowSystem();
        }

        public void SetupSnowSystem()
        {
            snowPS = GetComponent<ParticleSystem>();
            if (snowPS == null)
            {
                snowPS = gameObject.AddComponent<ParticleSystem>();
            }

            var main = snowPS.main;
            main.loop = true;
            main.startLifetime = 8f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(fallSpeed * 0.7f, fallSpeed * 1.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 1f, 1f, 0.65f),
                new Color(0.88f, 0.96f, 1f, 0.95f)
            );
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;

            // Prewarm để tuyết xuất hiện đầy màn hình ngay khi vào game
            main.prewarm = true;

            var emission = snowPS.emission;
            emission.enabled = true;
            emission.rateOverTime = maxParticles / 6f;

            var shape = snowPS.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(spawnAreaSize.x, 0.5f, 1f);
            shape.position = new Vector3(0f, spawnAreaSize.y * 0.55f, 0f);

            // Gió thổi nhẹ lắc lư
            var velocityOverLifetime = snowPS.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-windStrength, windStrength);

            // Mờ dần khi chạm đất
            var colorOverLifetime = snowPS.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0.0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(1f, 0.85f), new GradientAlphaKey(0f, 1.0f) }
            );
            colorOverLifetime.color = grad;

            // Renderer setup
            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 10; // Đảm bảo tuyết nằm trên ảnh nền
                if (snowflakeTexture != null)
                {
                    Material mat = new Material(Shader.Find("Sprites/Default"));
                    mat.mainTexture = snowflakeTexture;
                    renderer.material = mat;
                }
            }
        }
    }
}
