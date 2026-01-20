using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class MeleeEnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Movement Ranges")]
    public float approachRange = 1.4f;   // start walking if farther than this
    public float holdRange = 1.0f;        // stop moving inside this

    [Header("Attack")]
    public int meleeDamage = 10;
    public float attackRange = 0.9f;
    public float attackCooldown = 1.2f;
    public float attackDelay = 0.2f;

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

    bool facingRight = true;
    bool canAttack = true;
    bool isGrounded;

    // ================= UNITY =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        rb.gravityScale = 5f;      // gravity ON
        rb.freezeRotation = true;  // prevent tipping
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
        if (enemyHealth.isDead)
            return;
            
        if (player == null)
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

        // TOO FAR → MOVE TOWARD PLAYER
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

        // WITHIN HOLD RANGE → STOP MOVING
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

        // DEAD ZONE (between approach & hold) → IDLE
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
        if (enemyHealth.isDead)
            return;
            
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

        animator?.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        if (player != null)
        {
            player.GetComponent<PlayerHealth>()
                ?.TakeDamage(meleeDamage);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // ================= SEPARATION =================

    void ApplySeparation()
    {
        Collider2D[] neighbors =
            Physics2D.OverlapCircleAll(
                transform.position,
                separationRadius,
                enemyLayer
            );

        Vector2 separation = Vector2.zero;

        foreach (var other in neighbors)
        {
            if (other.gameObject == gameObject)
                continue;

            Vector2 diff =
                (Vector2)(transform.position - other.transform.position);

            float dist = diff.magnitude;
            if (dist > 0f)
                separation += diff.normalized / dist;
        }

        if (separation != Vector2.zero)
        {
            rb.linearVelocity +=
                separation.normalized *
                separationStrength *
                Time.fixedDeltaTime;
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

    // ================= DEBUG =================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, holdRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, approachRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}
