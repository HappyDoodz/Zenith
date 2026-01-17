using UnityEngine;
using System;

[RequireComponent(typeof(PlayerController2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Invincibility")]
    public float hitInvincibilityTime = 0.5f;

    [Header("Feedback")]
    public GameObject hitEffect;
    public GameObject deathEffect;

    public bool IsDead { get; private set; }

    PlayerController2D controller;
    float invincibilityTimer;

    // Optional events for UI / audio
    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    void Awake()
    {
        controller = GetComponent<PlayerController2D>();

        // Sync with MainController
        if (MainController.Instance != null)
        {
            maxHealth = MainController.Instance.maxHealth;
            MainController.Instance.currentHealth = maxHealth;
        }
    }

    void Update()
    {
        invincibilityTimer -= Time.deltaTime;
    }

    // ---------------- DAMAGE ----------------

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        // Dodge i-frames
        if (controller != null && controller.IsInvincible)
            return;

        // Hit i-frames
        if (invincibilityTimer > 0)
            return;

        invincibilityTimer = hitInvincibilityTime;

        if (MainController.Instance != null)
        {
            MainController.Instance.currentHealth -= damage;
            MainController.Instance.currentHealth =
                Mathf.Max(0, MainController.Instance.currentHealth);
        }

        SpawnHitEffect();

        OnHealthChanged?.Invoke(
            MainController.Instance.currentHealth,
            maxHealth
        );

        if (MainController.Instance.currentHealth <= 0)
        {
            Die();
        }
    }

    // ---------------- DEATH ----------------

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        OnDeath?.Invoke();

        // Metal Slug style: instant death, no ragdoll
        gameObject.SetActive(false);
    }

    // ---------------- FX ----------------

    void SpawnHitEffect()
    {
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
    }
}
