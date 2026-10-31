using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    // 1 = Dính theo màn hình (Rất xa), 0 = Đứng yên theo Map (Gần)
    [Range(0f, 1f)] public float factor = 0.5f;

    Transform cam;
    float startX;
    float camStartX;

    void Start()
    {
        cam = Camera.main.transform;
        startX = transform.position.x;
        camStartX = cam.position.x;
    }

    void LateUpdate()
    {
        float dx = cam.position.x - camStartX;
        Vector3 p = transform.position;
        p.x = startX + dx * factor;
        transform.position = p;
    }
}