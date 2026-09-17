using UnityEngine;

public class Spin : MonoBehaviour
{
    public float xSpeed = 0f;
    public float ySpeed = 0f;
    public float zSpeed = 0f;

    public Color color;

    // Use this for initialization
    private void Start()
    {
        var cubeRenderer = GetComponent<Renderer>();
        cubeRenderer.material.SetColor("_Color", color);
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime);
    }
}
