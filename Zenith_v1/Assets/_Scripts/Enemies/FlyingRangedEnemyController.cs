using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class FlyingRangedEnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.2f;

    [Header("Flying")]
    public float desiredAltitude = 3f;
    public float altitudeAdjustSpeed = 2.5f;
    public float altitudeDeadZone = 0.1f;

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

    [Header("Attack Timing")]
    public float firstShotDelay = 0.5f;

    int currentClip;
    float fireTimer;
    float attackEnterTimer;
    bool isReloading;
    bool isAttacking;

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
    public AudioSource weaponSource;
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
    EnemyHealth enemyHealth;

    bool facingRight = true;

    // ================= UNITY =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        rb.gravityScale = 0f;           // ✈️ flying
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
        if (enemyHealth.isDead || player == null)
            return;

        fireTimer -= Time.fixedDeltaTime;
        if (isAttacking)
            attackEnterTimer -= Time.fixedDeltaTime;

        HandleFacing();
        AimFirePointAtPlayer();
        HandleMovement();
        MaintainAltitude();
        //RotateFirePointTowardPlayer();
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
            MoveHorizontallyTowardPlayer();
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

    void MoveHorizontallyTowardPlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        rb.linearVelocity = new Vector2(
            dir * moveSpeed,
            rb.linearVelocity.y
        );

        animator?.SetBool("IsMoving", true);
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator?.SetBool("IsMoving", false);
    }

    // ================= ALTITUDE =================

    void MaintainAltitude()
    {
        float targetY = player.position.y + desiredAltitude;
        float delta = targetY - transform.position.y;

        if (Mathf.Abs(delta) < altitudeDeadZone)
            return;

        float verticalSpeed =
            Mathf.Clamp(delta, -1f, 1f) * altitudeAdjustSpeed;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            verticalSpeed
        );
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

    // ================= AIMING =================

    void RotateFirePointTowardPlayer()
    {
        if (firePoint == null)
            return;

        Vector2 dir =
            player.position - firePoint.position;

        float angle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        firePoint.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    void AimFirePointAtPlayer()
    {
        if (firePoint == null || player == null)
            return;

        Vector2 dir = player.position - firePoint.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ================= ATTACK STATE =================

    void StartAttacking()
    {
        if (isAttacking)
            return;

        isAttacking = true;
        attackEnterTimer = firstShotDelay;

        animator?.SetBool("IsAttacking", true);
    }

    void StopAttacking()
    {
        if (!isAttacking)
            return;

        isAttacking = false;
        attackEnterTimer = 0f;

        animator?.SetBool("IsAttacking", false);
    }

    // ================= SHOOTING =================

    void TryShoot()
    {
        if (!isAttacking || isReloading)
            return;

        if (attackEnterTimer > 0f || fireTimer > 0f)
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

            ProjectileRotation p = proj.GetComponent<ProjectileRotation>();
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

        PlayReloadSound();

        yield return new WaitForSeconds(reloadTime);

        currentClip = clipSize;
        isReloading = false;
    }

    // ================= EFFECTS =================

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

        flash.transform.rotation = firePoint.rotation;
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
        if (weaponSource == null || fireSound == null)
            return;

        weaponSource.pitch =
            1f + Random.Range(-pitchVariance, pitchVariance);

        weaponSource.PlayOneShot(fireSound);
    }

    void PlayReloadSound()
    {
        if (weaponSource == null || reloadSound == null)
            return;

        weaponSource.pitch = 1f;
        weaponSource.PlayOneShot(reloadSound);
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

            float dist = Mathf.Max(diff.magnitude, 0.001f);
            float strength = 1f - (dist / separationRadius);

            separation += diff.normalized * strength;
            count++;
        }

        if (count > 0)
        {
            separation /= count;

            rb.linearVelocity += new Vector2(
                separation.x * separationStrength * Time.fixedDeltaTime,
                0f
            );
        }
    }
}
