using UnityEngine;

public class SimpleParallax : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;

    [Header("Layer Transforms")]
    [SerializeField] Transform farLayer;
    [SerializeField] Transform nearLayer;

    [Header("Parallax Strength")]
    [Tooltip("Subtle movement (furthest back)")]
    [SerializeField] Vector2 farMultiplier = new Vector2(0.01f, 0.005f);

    [Tooltip("Stronger movement (closer layer)")]
    [SerializeField] Vector2 nearMultiplier = new Vector2(0.03f, 0.015f);

    Vector3 farStartPos;
    Vector3 nearStartPos;

    void Awake()
    {
        if (farLayer != null)
            farStartPos = farLayer.localPosition;

        if (nearLayer != null)
            nearStartPos = nearLayer.localPosition;
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 playerPos = player.position;

        if (farLayer != null)
        {
            farLayer.localPosition =
                farStartPos +
                new Vector3(
                    playerPos.x * farMultiplier.x,
                    playerPos.y * farMultiplier.y,
                    0f
                );
        }

        if (nearLayer != null)
        {
            nearLayer.localPosition =
                nearStartPos +
                new Vector3(
                    playerPos.x * nearMultiplier.x,
                    playerPos.y * nearMultiplier.y,
                    0f
                );
        }
    }
}
