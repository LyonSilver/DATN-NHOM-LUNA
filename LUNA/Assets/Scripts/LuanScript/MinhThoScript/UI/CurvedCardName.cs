
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurvedCardName : MonoBehaviour
{
    [Header("Wave Text Settings")]

    [Tooltip("Độ cao của đường sóng. Giá trị càng lớn, chữ càng uốn mạnh.")]
    [Range(0f, 30f)]
    public float waveAmplitude = 5f;

    [Tooltip("Số nhịp sóng trên toàn bộ dòng chữ.")]
    [Range(0.25f, 2f)]
    public float waveCycles = 1f;

    [Tooltip("Điều chỉnh vị trí bắt đầu của đường sóng.")]
    [Range(0f, 360f)]
    public float wavePhase = 0f;

    [Tooltip("Xoay từng ký tự theo đường cong.")]
    public bool rotateCharacters = true;

    private TextMeshProUGUI tmp;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    private void LateUpdate()
    {
        if (tmp == null) return;

        // Tạo lại mesh gốc trước khi biến dạng
        tmp.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmp.textInfo;
        int count = textInfo.characterCount;

        if (count < 2) return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        // Tìm chiều rộng thực tế của dòng chữ
        for (int i = 0; i < count; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible) continue;

            minX = Mathf.Min(minX, character.bottomLeft.x);
            maxX = Mathf.Max(maxX, character.topRight.x);
        }

        float width = maxX - minX;

        if (width <= 0f) return;

        float phase = wavePhase * Mathf.Deg2Rad;
        float angularFrequency = Mathf.PI * 2f * waveCycles;

        // Biến dạng từng ký tự theo đường sóng
        for (int i = 0; i < count; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible) continue;

            int materialIndex = character.materialReferenceIndex;
            int vertexIndex = character.vertexIndex;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            // Tâm của ký tự
            Vector3 charCenter = new Vector3(
                (character.bottomLeft.x + character.topRight.x) * 0.5f,
                (character.bottomLeft.y + character.topRight.y) * 0.5f,
                0f
            );

            // Vị trí chuẩn hóa trên toàn bộ dòng chữ
            float t = (charCenter.x - minX) / width;

            // Tính độ lệch theo đường sóng
            float angleInWave = t * angularFrequency + phase;

            float yOffset =
                waveAmplitude * Mathf.Sin(angleInWave);

            // Tính độ nghiêng của đường sóng
            float slope =
                waveAmplitude * angularFrequency
                * Mathf.Cos(angleInWave) / width;

            float rotationAngle = rotateCharacters
                ? Mathf.Atan(slope) * Mathf.Rad2Deg
                : 0f;

            Quaternion rotation =
                Quaternion.Euler(0f, 0f, rotationAngle);

            // Biến dạng 4 đỉnh của ký tự
            for (int j = 0; j < 4; j++)
            {
                Vector3 vertex = vertices[vertexIndex + j];

                vertex -= charCenter;
                vertex = rotation * vertex;
                vertex += charCenter;

                vertex.y += yOffset;

                vertices[vertexIndex + j] = vertex;
            }
        }

        // Cập nhật mesh TextMeshPro
        tmp.UpdateVertexData(
            TMP_VertexDataUpdateFlags.Vertices
        );
    }
}
