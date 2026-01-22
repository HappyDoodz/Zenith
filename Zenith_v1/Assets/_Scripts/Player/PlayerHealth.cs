using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerController2D))]
[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Invincibility")]
    public float hitInvincibilityTime = 0.5f;

    [Header("Feedback")]
    public GameObject hitEffect;
    public GameObject deathEffect;

    [Header("Audio")]
    [Tooltip("Sounds played when the player is hurt (optional)")]
    public AudioClip[] hurtSounds;

    [Tooltip("Chance (0–1) that a hurt sound will play")]
    [Range(0f, 1f)]
    public float hurtSoundChance = 0.6f;

    [Tooltip("Sounds played when the player dies (always plays one)")]
    public AudioClip[] deathSounds;

    [Range(0.8f, 1.2f)]
    public float pitchMin = 0.95f;

    [Range(0.8f, 1.2f)]
    public float pitchMax = 1.05f;

    public bool IsDead { get; private set; }

    PlayerController2D controller;
    PlayerAnimatorController anim;
    public AudioSource audioSource;

    float invincibilityTimer;

    // Optional events for UI / audio
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnArmourChanged;
    public event Action OnDeath;

    void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        anim = GetComponent<PlayerAnimatorController>();

        if (MainController.Instance != null)
        {
            maxHealth = MainController.Instance.maxHealth;
        }
    }

    void Update()
    {
        invincibilityTimer -= Time.deltaTime;
    }

    // ================= DAMAGE =================

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        if (controller != null && controller.IsInvincible)
            return;

        if (invincibilityTimer > 0f)
            return;

        invincibilityTimer = hitInvincibilityTime;

        if (MainController.Instance != null)
        {
            MainController.Instance.ApplyDamage(damage);
        }

        SpawnHitEffect();
        TryPlayHurtSound();

        OnHealthChanged?.Invoke(
            MainController.Instance.currentHealth,
            maxHealth
        );

        OnArmourChanged?.Invoke(
            MainController.Instance.currentArmour,
            MainController.Instance.maxArmour
        );

        if (MainController.Instance.currentHealth <= 0f)
        {
            Die();
        }
    }

    // ================= DEATH =================

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        anim?.TriggerDeath();

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        PlayDeathSound();

        OnDeath?.Invoke();

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return ScreenFader.Instance.FadeOut();

        MainController.Instance.ResetRun();

        string menu = MainController.Instance.mainMenuSceneName;

        if (!string.IsNullOrEmpty(menu))
            SceneManager.LoadScene(menu);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ================= AUDIO =================

    void TryPlayHurtSound()
    {
        if (hurtSounds == null || hurtSounds.Length == 0)
            return;

        if (UnityEngine.Random.value > hurtSoundChance)
            return;

        AudioClip clip =
            hurtSounds[UnityEngine.Random.Range(0, hurtSounds.Length)];

        audioSource.pitch =
            UnityEngine.Random.Range(pitchMin, pitchMax);

        audioSource.PlayOneShot(clip);
    }

    void PlayDeathSound()
    {
        if (deathSounds == null || deathSounds.Length == 0)
            return;

        AudioClip clip =
            deathSounds[UnityEngine.Random.Range(0, deathSounds.Length)];

        GameObject audioObj = new GameObject("PlayerDeathSound");
        audioObj.transform.position = transform.position;

        AudioSource src = audioObj.AddComponent<AudioSource>();
        src.clip = clip;
        src.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
        src.spatialBlend = 0f;
        src.Play();

        Destroy(audioObj, clip.length / src.pitch);
    }

    // ================= FX =================

    void SpawnHitEffect()
    {
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
    }
}
