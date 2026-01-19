using UnityEngine;

public class PickupBob : MonoBehaviour
{
    [Header("Bob Settings")]
    [SerializeField] float amplitude = 0.15f;
    [SerializeField] float frequency = 2f;

    Vector3 startPos;

    void Awake()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float offset =
            Mathf.Sin(Time.time * frequency) * amplitude;

        transform.localPosition =
            startPos + Vector3.up * offset;
    }
}
