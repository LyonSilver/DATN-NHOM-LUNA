using UnityEngine;
public sealed class VFXLifetime : MonoBehaviour
{
    [Min(0.01f)] public float seconds = 1f;
    void Start() { Destroy(gameObject, seconds); }
}
