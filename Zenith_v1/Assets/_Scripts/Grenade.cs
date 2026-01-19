using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Fuse")]
    [SerializeField] float fuseTime = 1.5f;

    [Header("Explosion")]
    [SerializeField] float explosionRadius = 2.5f;
    [SerializeField] int damage = 50;
    [SerializeField] LayerMask damageLayers;

    [Header("Force")]
    [SerializeField] float explosionForce = 8f;

    [Header("Visuals")]
    [SerializeField] GameObject explosionVFX;
    [SerializeField] bool destroyOnExplode = true;

    bool exploded;

    void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        // Damage + force
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius,
            damageLayers
        );

        foreach (var hit in hits)
        {
            // Damage
            hit.GetComponent<EnemyHealth>()?.TakeDamage(damage);

            // Knockback (optional)
            Rigidbody2D rb = hit.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = (rb.position - (Vector2)transform.position).normalized;
                rb.AddForce(dir * explosionForce, ForceMode2D.Impulse);
            }
        }

        // Visual effect
        if (explosionVFX != null)
        {
            Instantiate(
                explosionVFX,
                transform.position,
                Quaternion.identity
            );
        }

        if (destroyOnExplode)
            Destroy(gameObject);
    }

    // ---------------- DEBUG ----------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}