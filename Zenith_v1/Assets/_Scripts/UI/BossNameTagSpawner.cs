using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class BossNameTagSpawner : MonoBehaviour
{
    [Header("Name Tag")]
    [Tooltip("World-space name tag prefab")]
    public GameObject nameTagPrefab;

    [Tooltip("Displayed boss name")]
    public string bossName = "BOSS";

    [Tooltip("World offset above the boss")]
    public Vector3 nameTagOffset = new Vector3(0f, 2.5f, 0f);

    GameObject spawnedTag;
    BossNameTag tag;
    EnemyHealth health;

    void Start()
    {
        health = GetComponent<EnemyHealth>();

        if (nameTagPrefab == null)
        {
            Debug.LogWarning(
                $"BossNameTagSpawner on {name} has no prefab assigned."
            );
            return;
        }

        // ✅ Spawn at correct world position immediately
        Vector3 spawnPos = transform.position + nameTagOffset;

        spawnedTag = Instantiate(
            nameTagPrefab,
            spawnPos,
            Quaternion.identity
        );

        tag = spawnedTag.GetComponent<BossNameTag>();

        if (tag == null)
        {
            Debug.LogError(
                "NameTag prefab is missing BossNameTag component."
            );
            Destroy(spawnedTag);
            return;
        }

        tag.target = transform;
        tag.worldOffset = nameTagOffset;
        tag.SetName(bossName);
    }

    void Update()
    {
        // Auto-cleanup when boss dies
        if (health != null && health.isDead && spawnedTag != null)
        {
            Destroy(spawnedTag);
            spawnedTag = null;
        }
    }

    void OnDestroy()
    {
        if (spawnedTag != null)
            Destroy(spawnedTag);
    }
}
