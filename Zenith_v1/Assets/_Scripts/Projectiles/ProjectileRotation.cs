using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileRotation : MonoBehaviour
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

    // Track actual DAMAGE receivers, not colliders
    HashSet<object> hitTargets = new HashSet<object>();

    // ================= UNITY =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnEnable()
    {
        // 🔑 THIS IS THE FIX: respect Z rotation
        rb.linearVelocity = transform.right * speed;

        Invoke(nameof(Expire), lifetime);
    }

    void OnDisable()
    {
        CancelInvoke();
        hitTargets.Clear();
    }

    // ================= COLLISION =================

    void OnTriggerEnter2D(Collider2D other)
    {
        // Layer filtering
        if (hitLayers.value != 0 &&
            (hitLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (explosive)
        {
            Explode();
            return;
        }

        if (!ApplyDamage(other))
            return;

        HandlePiercing();
    }

    // ================= DAMAGE =================

    bool ApplyDamage(Collider2D target)
    {
        // ENEMY
        EnemyHealth enemy =
            target.GetComponentInParent<EnemyHealth>();

        if (enemy != null && canHitEnemies)
        {
            if (hitTargets.Contains(enemy))
                return false;

            hitTargets.Add(enemy);

            enemy.TakeDamage(damage);
            ApplyKnockback(target);
            SpawnHitEffect();
            return true;
        }

        // PLAYER
        PlayerHealth player =
            target.GetComponentInParent<PlayerHealth>();

        if (player != null && canHitPlayer)
        {
            if (hitTargets.Contains(player))
                return false;

            hitTargets.Add(player);

            player.TakeDamage(damage);
            ApplyKnockback(target);
            SpawnHitEffect();
            return true;
        }

        return false;
    }

    void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.attachedRigidbody;
        if (targetRb == null)
            return;

        Vector2 direction =
            (target.transform.position - transform.position).normalized;

        targetRb.AddForce(
            direction * knockbackForce,
            ForceMode2D.Impulse
        );
    }

    void SpawnHitEffect()
    {
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
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

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.right
        );
    }
}
