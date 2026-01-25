using UnityEngine;

public class SpriteSway : MonoBehaviour
{
    [Header("Vertical Sway")]
    public float verticalAmplitude = 0.15f;
    public float verticalSpeed = 2f;

    [Header("Horizontal Sway")]
    public float horizontalAmplitude = 0.05f;
    public float horizontalSpeed = 1.5f;

    [Header("Phase Offset")]
    [Tooltip("Randomizes motion so multiple enemies don't sync")]
    public bool randomizePhase = true;

    Vector3 startLocalPos;
    float verticalOffset;
    float horizontalOffset;

    void Awake()
    {
        startLocalPos = transform.localPosition;

        if (randomizePhase)
        {
            verticalOffset = Random.Range(0f, 10f);
            horizontalOffset = Random.Range(0f, 10f);
        }
    }

    void Update()
    {
        float y =
            Mathf.Sin(Time.time * verticalSpeed + verticalOffset) *
            verticalAmplitude;

        float x =
            Mathf.Sin(Time.time * horizontalSpeed + horizontalOffset) *
            horizontalAmplitude;

        transform.localPosition =
            startLocalPos + new Vector3(x, y, 0f);
    }

    void OnDisable()
    {
        // Reset when pooled / disabled
        transform.localPosition = startLocalPos;
    }
}
