using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50;

    [Header("Feedback")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public float destroyDelay = 0f;

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

        // Optional: stop AI / attacks
        //GetComponent<MeleeEnemyController>()?.enabled = false;
        //GetComponent<RangedEnemyController>()?.enabled = false;

        GetComponent<Animator>()?.SetTrigger("Die");

        Destroy(gameObject, destroyDelay);
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
