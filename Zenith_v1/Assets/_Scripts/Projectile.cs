using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    public enum DamageType
    {
        Bullet,
        Explosive
    }

    [Header("Damage")]
    public int damage = 10;
    public DamageType damageType = DamageType.Bullet;

    [Header("Movement")]
    public float speed = 15f;
    public float lifetime = 3f;

    [Header("Piercing")]
    public int pierceCount = 0; // 0 = no piercing

    [Header("Explosion")]
    public bool explosive = false;
    public float explosionRadius = 2f;
    public LayerMask explosionLayers;

    [Header("Knockback")]
    public float knockbackForce = 5f;

    [Header("Collision")]
    public LayerMask hitLayers;
    public bool destroyOnHit = true;

    [Header("Friendly Fire")]
    public bool canHitPlayer = false;
    public bool canHitEnemies = true;

    [Header("Effects")]
    public GameObject hitEffect;
    public GameObject explosionEffect;

    Rigidbody2D rb;
    HashSet<Collider2D> hitTargets = new();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Force trigger collider
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnEnable()
    {
        rb.linearVelocity = transform.right * speed;
        Invoke(nameof(Expire), lifetime);
    }

    void OnDisable()
    {
        CancelInvoke();
        hitTargets.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Layer filter (safe)
        if (hitLayers.value != 0 &&
            (hitLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        // Prevent multi-hit on same collider
        if (hitTargets.Contains(other))
            return;

        hitTargets.Add(other);

        if (explosive)
        {
            Explode();
            return;
        }

        ApplyDamage(other);
        HandlePiercing();
    }

    // ================= DAMAGE =================

    void ApplyDamage(Collider2D target)
    {
        EnemyHealth enemy =
            target.GetComponentInParent<EnemyHealth>();

        if (enemy != null && canHitEnemies)
        {
            enemy.TakeDamage(damage);
            ApplyKnockback(target);
        }

        PlayerHealth player =
            target.GetComponentInParent<PlayerHealth>();

        if (player != null && canHitPlayer)
        {
            player.TakeDamage(damage);
            ApplyKnockback(target);
        }

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
    }

    void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.attachedRigidbody;
        if (targetRb == null) return;

        Vector2 direction =
            (target.transform.position - transform.position).normalized;

        targetRb.AddForce(
            direction * knockbackForce,
            ForceMode2D.Impulse
        );
    }

    // ================= EXPLOSION =================

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius,
            explosionLayers
        );

        foreach (var hit in hits)
        {
            ApplyDamage(hit);
        }

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Disable();
    }

    // ================= HELPERS =================

    void HandlePiercing()
    {
        if (pierceCount <= 0)
        {
            Disable();
            return;
        }

        pierceCount--;
    }

    void Expire()
    {
        Disable();
    }

    void Disable()
    {
        if (destroyOnHit)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    // ================= DEBUG =================

    void OnDrawGizmosSelected()
    {
        if (explosive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
