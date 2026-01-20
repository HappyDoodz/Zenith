using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class RangedEnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.2f;

    [Header("Ranges")]
    public float approachRange = 6f;
    public float holdRange = 4.5f;
    public float attackRange = 5f;

    [Header("Weapon")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    public int clipSize = 6;
    public float fireRate = 0.4f;
    public float reloadTime = 1.6f;

    int currentClip;
    float fireTimer;
    bool isReloading;
    bool isAttacking;

    [Header("Attack Variants")]
    [Range(0f, 1f)]
    public float crouchChance = 0.5f;
    bool isCrouching;

    [Header("IK Recoil")]
    public ArmRecoilController lefttArmRecoil;
    public Vector2 recoilOffset = new Vector2(-0.08f, 0.02f);
    public float recoilKickTime = 0.05f;
    public float recoilReturnTime = 0.1f;

    [Header("Muzzle Flash")]
    public Sprite[] muzzleFlashSprites;
    public Vector2 muzzleFlashOffset;
    public float muzzleFlashScale = 1f;
    public float muzzleFlashLifetime = 0.05f;
    public string muzzleFlashSortingLayer = "Projectiles";
    public int muzzleFlashSortingOrder = 0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    [Range(0f, 0.2f)]
    public float pitchVariance = 0.05f;

    [Header("Separation")]
    public float separationRadius = 0.7f;
    public float separationStrength = 1.2f;
    public LayerMask enemyLayer;

    Rigidbody2D rb;
    Animator animator;
    Transform player;

    bool facingRight = true;

    // ================= UNITY =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 5f;
        rb.freezeRotation = true;

        currentClip = clipSize;
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
        if (player == null)
            return;

        fireTimer -= Time.fixedDeltaTime;

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
            StopAttacking();
            MoveTowardPlayer();
            return;
        }

        if (distance <= holdRange && distance <= attackRange)
        {
            StopMovement();
            StartAttacking();
            TryShoot();
            return;
        }

        StopAttacking();
        StopMovement();
    }

    void MoveTowardPlayer()
    {
        rb.linearVelocity = new Vector2(
            Mathf.Sign(player.position.x - transform.position.x) * moveSpeed,
            rb.linearVelocity.y
        );

        animator?.SetBool("IsMoving", true);
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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

    // ================= ATTACK STATE =================

    void StartAttacking()
    {
        if (isAttacking)
            return;

        isAttacking = true;

        isCrouching =
            Random.value < crouchChance;

        animator?.SetBool("IsAttacking", true);
        animator?.SetBool("IsCrouching", isCrouching);
    }

    void StopAttacking()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        animator?.SetBool("IsAttacking", false);
        animator?.SetBool("IsCrouching", false);
    }

    // ================= SHOOTING =================

    void TryShoot()
    {
        if (!isAttacking || isReloading)
            return;

        if (fireTimer > 0f)
            return;

        if (currentClip <= 0)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        Fire();
    }

    void Fire()
    {
        fireTimer = fireRate;
        currentClip--;

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation
            );

            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
            {
                p.canHitPlayer = true;
                p.canHitEnemies = false;
            }
        }

        SpawnMuzzleFlash();
        PlayFireSound();

        lefttArmRecoil?.ApplyRecoil(
            recoilOffset,
            recoilKickTime,
            recoilReturnTime,
            facingRight
        );
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        StopAttacking();

        //animator?.SetTrigger("Reload");
        PlayReloadSound();

        yield return new WaitForSeconds(reloadTime);

        currentClip = clipSize;
        isReloading = false;
    }

    // ================= MUZZLE FLASH =================

    void SpawnMuzzleFlash()
    {
        if (muzzleFlashSprites == null ||
            muzzleFlashSprites.Length == 0 ||
            firePoint == null)
            return;

        Sprite sprite =
            muzzleFlashSprites[Random.Range(0, muzzleFlashSprites.Length)];

        GameObject flash = new GameObject("EnemyMuzzleFlash");

        flash.transform.position =
            firePoint.position + (Vector3)muzzleFlashOffset;

        float zRotation = facingRight ? 0f : 180f;
        flash.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);

        flash.transform.localScale =
            Vector3.one * muzzleFlashScale;

        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = muzzleFlashSortingLayer;
        sr.sortingOrder = muzzleFlashSortingOrder;

        Destroy(flash, muzzleFlashLifetime);
    }

    // ================= AUDIO =================

    void PlayFireSound()
    {
        if (audioSource == null || fireSound == null)
            return;

        audioSource.pitch =
            1f + Random.Range(-pitchVariance, pitchVariance);

        audioSource.PlayOneShot(fireSound);
    }

    void PlayReloadSound()
    {
        if (audioSource == null || reloadSound == null)
            return;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(reloadSound);
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
}
