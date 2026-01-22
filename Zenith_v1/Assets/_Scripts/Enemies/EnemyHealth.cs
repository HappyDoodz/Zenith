using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50;

    [Header("Feedback")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public float destroyDelay = 0f;

    [Header("Audio")]
    [Tooltip("Sounds played when the enemy is hurt (optional)")]
    public AudioClip[] hurtSounds;

    [Tooltip("Chance (0–1) that a hurt sound will play")]
    [Range(0f, 1f)]
    public float hurtSoundChance = 0.5f;

    [Tooltip("Sounds played when the enemy dies (always plays one)")]
    public AudioClip[] deathSounds;

    [Range(0.8f, 1.2f)]
    public float pitchMin = 0.95f;

    [Range(0.8f, 1.2f)]
    public float pitchMax = 1.05f;

    [Header("Drops")]
    [Tooltip("Possible pickups to spawn on death (can be empty)")]
    public GameObject[] possibleDrops;

    [Tooltip("Chance (0–1) that ANY drop will occur")]
    [Range(0f, 1f)]
    public float dropChance = 0.25f;

    int currentHealth;
    [HideInInspector] public bool isDead;

    public AudioSource audioSource;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // ================= DAMAGE =================

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        // Play hurt sound only if still alive
        if (currentHealth > 0)
            TryPlayHurtSound();

        if (currentHealth <= 0)
            Die();
    }

    // ================= DEATH =================

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        MainController.Instance.currentKills ++;

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        PlayDeathSound();

        // Move to DeadEnemy layer (ground-only collisions)
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("DeadEnemy"));

        TrySpawnDrop();

        GetComponent<Animator>()?.SetTrigger("Die");

        Destroy(gameObject, destroyDelay);
    }

    // ================= AUDIO =================

    void TryPlayHurtSound()
    {
        if (hurtSounds == null || hurtSounds.Length == 0)
            return;

        if (Random.value > hurtSoundChance)
            return;

        AudioClip clip =
            hurtSounds[Random.Range(0, hurtSounds.Length)];

        audioSource.pitch =
            Random.Range(pitchMin, pitchMax);

        audioSource.PlayOneShot(clip);
    }

    void PlayDeathSound()
    {
        if (deathSounds == null || deathSounds.Length == 0)
            return;

        AudioClip clip =
            deathSounds[Random.Range(0, deathSounds.Length)];

        // Spawn a temp audio object so sound survives destruction
        GameObject audioObj = new GameObject("EnemyDeathSound");
        audioObj.transform.position = transform.position;

        AudioSource src = audioObj.AddComponent<AudioSource>();
        src.clip = clip;
        src.pitch = Random.Range(pitchMin, pitchMax);
        src.spatialBlend = 0f;
        src.Play();

        Destroy(audioObj, clip.length / src.pitch);
    }

    // ================= DROP LOGIC =================

    void TrySpawnDrop()
    {
        if (possibleDrops == null || possibleDrops.Length == 0)
            return;

        if (Random.value > dropChance)
            return;

        GameObject drop =
            possibleDrops[Random.Range(0, possibleDrops.Length)];

        if (drop == null)
            return;

        Instantiate(drop, transform.position, Quaternion.identity);
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
