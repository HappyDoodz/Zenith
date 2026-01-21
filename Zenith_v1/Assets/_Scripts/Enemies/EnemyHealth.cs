using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50;

    [Header("Feedback")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public float destroyDelay = 0f;

    [Header("Drops")]
    [Tooltip("Possible pickups to spawn on death (can be empty)")]
    public GameObject[] possibleDrops;

    [Tooltip("Chance (0–1) that ANY drop will occur")]
    [Range(0f, 1f)]
    public float dropChance = 0.25f;

    int currentHealth;
    [HideInInspector] public bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Move to DeadEnemy layer (ground-only collisions)
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("DeadEnemy"));

        TrySpawnDrop();

        GetComponent<Animator>()?.SetTrigger("Die");

        Destroy(gameObject, destroyDelay);
    }

    // ================= DROP LOGIC =================

    void TrySpawnDrop()
    {
        if (possibleDrops == null || possibleDrops.Length == 0)
            return;

        // Roll chance first (allows "nothing" most of the time)
        if (Random.value > dropChance)
            return;

        GameObject drop =
            possibleDrops[Random.Range(0, possibleDrops.Length)];

        if (drop == null)
            return;

        Instantiate(
            drop,
            transform.position,
            Quaternion.identity
        );
    }

    // ================= UTIL =================

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
