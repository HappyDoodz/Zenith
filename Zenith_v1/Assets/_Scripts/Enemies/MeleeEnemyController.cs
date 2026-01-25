using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(AudioSource))]
public class MeleeEnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Movement Ranges")]
    public float approachRange = 1.4f;
    public float holdRange = 1.0f;

    [Header("Attack")]
    public int meleeDamage = 10;
    public float attackRange = 0.9f;
    public float attackCooldown = 1.2f;
    public float attackDelay = 0.2f;
    private int attackCounter;

    [Header("Audio")]
    public AudioClip[] meleeSwingSounds;
    public AudioClip[] meleeHitSounds;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;
    public float swingVolume = 1f;
    public float hitVolume = 1f;

    [Header("Separation")]
    public float separationRadius = 0.6f;
    public float separationStrength = 1.5f;
    public LayerMask enemyLayer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    Animator animator;
    Transform player;
    EnemyHealth enemyHealth;
    public AudioSource weaponSource;

    bool facingRight = true;
    bool canAttack = true;
    bool isGrounded;

    // ================= UNITY =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        rb.gravityScale = 5f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (enemyHealth.isDead || player == null)
            return;

        CheckGrounded();
        HandleFacing();
        HandleMovement();
        ApplySeparation();
    }

    // ================= MOVEMENT =================

    void HandleMovement()
    {
        float distance =
            Vector2.Distance(transform.position, player.position);

        if (distance > approachRange)
        {
            Vector2 dir =
                (player.position - transform.position).normalized;

            rb.linearVelocity = new Vector2(
                dir.x * moveSpeed,
                rb.linearVelocity.y
            );

            animator?.SetBool("IsMoving", true);
            return;
        }

        if (distance <= holdRange)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );

            animator?.SetBool("IsMoving", false);
            TryAttack();
            return;
        }

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );

        animator?.SetBool("IsMoving", false);
    }

    // ================= FACING =================

    void HandleFacing()
    {
        bool shouldFaceRight =
            player.position.x > transform.position.x;

        if (shouldFaceRight != facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;

        transform.rotation = Quaternion.Euler(
            0f,
            facingRight ? 0f : 180f,
            0f
        );
    }

    // ================= ATTACK =================

    void TryAttack()
    {
        if (!canAttack)
            return;

        float dist =
            Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
            StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;
    
        attackCounter = Random.Range(0, 3);
        animator?.SetInteger("AttackCounter", attackCounter);
        animator?.SetTrigger("Attack");
    
        yield return new WaitForSeconds(attackDelay);

        PlaySwingSound();
    
        if (enemyHealth.isDead || player == null)
            yield break;
    
        // ===== RE-CHECK RANGE AT HIT TIME =====
    
        float dist = Vector2.Distance(transform.position, player.position);
    
        // Player must still be in attack range
        if (dist > attackRange)
            goto EndAttack;
    
        // Player must be in front of enemy (not behind)
        float dirToPlayer = player.position.x - transform.position.x;
        if (facingRight && dirToPlayer < 0f)
            goto EndAttack;
        if (!facingRight && dirToPlayer > 0f)
            goto EndAttack;
    
        // ===== APPLY DAMAGE =====
    
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(meleeDamage);
            PlayHitSound();
        }
    
    EndAttack:
    
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // ================= AUDIO =================

    void PlaySwingSound()
    {
        if (meleeSwingSounds == null ||
            meleeSwingSounds.Length == 0)
            return;

        AudioClip clip =
            meleeSwingSounds[Random.Range(0, meleeSwingSounds.Length)];

        weaponSource.pitch =
            Random.Range(pitchMin, pitchMax);

        weaponSource.volume = swingVolume;
        weaponSource.PlayOneShot(clip);
    }

    void PlayHitSound()
    {
        if (meleeHitSounds == null ||
            meleeHitSounds.Length == 0)
            return;

        AudioClip clip =
            meleeHitSounds[Random.Range(0, meleeHitSounds.Length)];

        weaponSource.pitch =
            Random.Range(pitchMin, pitchMax);

        weaponSource.volume = hitVolume;
        weaponSource.PlayOneShot(clip);
    }

    // ================= SEPARATION =================

    void ApplySeparation()
    {
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(
            transform.position,
            separationRadius,
            enemyLayer
        );

        Vector2 separation = Vector2.zero;
        int count = 0;

        foreach (var other in neighbors)
        {
            if (other.gameObject == gameObject)
                continue;

            Vector2 diff =
                (Vector2)(transform.position - other.transform.position);

            float dist = diff.magnitude;
            if (dist < 0.001f)
            {
                diff = Random.insideUnitCircle.normalized;
                dist = 0.001f;
            }

            float strength = 1f - (dist / separationRadius);
            separation += diff.normalized * strength;
            count++;
        }

        if (count > 0)
        {
            separation /= count;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x +
                separation.normalized.x *
                separationStrength *
                Time.fixedDeltaTime,
                rb.linearVelocity.y
            );
        }
    }

    // ================= GROUND CHECK =================

    void CheckGrounded()
    {
        if (groundCheck == null)
            return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }
}
