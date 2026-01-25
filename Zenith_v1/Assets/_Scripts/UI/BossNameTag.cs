using UnityEngine;
using TMPro;

public class BossNameTag : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 worldOffset = new Vector3(0f, 2.5f, 0f);

    [Header("UI")]
    public TextMeshProUGUI nameText;

    [Header("Behavior")]
    public bool faceCamera = true;
    public float followSmooth = 12f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Smooth follow
        Vector3 desiredPos = target.position + worldOffset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            Time.deltaTime * followSmooth
        );

        // Always face camera
        if (faceCamera && cam != null)
        {
            transform.rotation = cam.transform.rotation;
        }
    }

    public void SetName(string bossName)
    {
        if (nameText != null)
            nameText.text = bossName;
    }
}
