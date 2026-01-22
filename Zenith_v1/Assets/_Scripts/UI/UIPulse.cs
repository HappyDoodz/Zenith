using UnityEngine;

public class UIPulse : MonoBehaviour
{
    [Header("Opacity")]
    public CanvasGroup canvasGroup;
    public float minAlpha = 0.5f;
    public float maxAlpha = 1f;

    [Header("Scale Pulse")]
    public bool pulseScale = false;
    public float minScale = 1f;
    public float maxScale = 1.05f;

    [Header("Directional Nudge")]
    public bool enableNudge = false;
    public Vector2 nudgeDirection = Vector2.right; // left or right
    public float nudgeDistance = 6f;
    public float nudgeSpeed = 2.5f; // 🔑 independent nudge speed

    [Header("Timing")]
    public float pulseSpeed = 1.5f; // opacity / scale speed

    Vector3 baseScale;
    Vector3 basePosition;

    void Awake()
    {
        baseScale = transform.localScale;
        basePosition = transform.localPosition;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // Pulse wave (opacity / scale)
        float pulseT =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        // Nudge wave (movement)
        float nudgeT =
            (Mathf.Sin(Time.time * nudgeSpeed) + 1f) * 0.5f;

        // Opacity
        if (canvasGroup != null)
            canvasGroup.alpha =
                Mathf.Lerp(minAlpha, maxAlpha, pulseT);

        // Scale
        if (pulseScale)
            transform.localScale =
                baseScale * Mathf.Lerp(minScale, maxScale, pulseT);

        // Directional nudge
        if (enableNudge)
        {
            Vector3 offset =
                (Vector3)nudgeDirection.normalized *
                Mathf.Lerp(0f, nudgeDistance, nudgeT);

            transform.localPosition = basePosition + offset;
        }
    }

    void OnDisable()
    {
        // Reset transforms when hidden
        transform.localScale = baseScale;
        transform.localPosition = basePosition;

        if (canvasGroup != null)
            canvasGroup.alpha = maxAlpha;
    }
}
