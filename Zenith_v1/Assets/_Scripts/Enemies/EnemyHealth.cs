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
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        GetComponent<Animator>()?.SetTrigger("Die");
        Destroy(gameObject, destroyDelay);
    }
}
