using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    Transform FirePoint =>
        weaponVisuals.CurrentFirePoint;

    WeaponVisualController weaponVisuals;
    PlayerController2D controller;

    [Header("IK Recoil")]
    [SerializeField] ArmRecoilController rightArmRecoil;


    [Header("Melee")]
    public float meleeRange = 1f;
    public int meleeDamage = 25;
    public float meleeDelay = 0.5f;
    public float meleeDuration = 1f;
    public LayerMask enemyLayer;
    public Transform meleePoint;

    [Header("Grenades")]
    public GameObject grenadePrefab;
    public Transform grenadeSpawn;
    public float grenadeThrowForce = 8f;
    public float grenadeThrowDelay = 0.5f;
    public float grenadeThrowDuration = 1f;

    bool isMeleeAttacking;
    bool isThrowingGrenade;

    Weapon CurrentWeapon => MainController.Instance.GetCurrentWeapon();

    void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        weaponVisuals = GetComponent<WeaponVisualController>();
    }
    
    void Start()
    {
        // MainController is now guaranteed to exist
        if (CurrentWeapon != null)
        {
            weaponVisuals.SetWeapon(CurrentWeapon);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            SwapWeapon();

        if (Input.GetKey(KeyCode.J))
            TryShoot();

        if (Input.GetKeyDown(KeyCode.I))
            Melee();

        if (Input.GetKeyDown(KeyCode.E))
            Reload();

        if (Input.GetKeyDown(KeyCode.K))
            ThrowGrenade();
    }

    // ---------------- SHOOTING ----------------

    void TryShoot()
    {
        if (IsBusy)
            return;

        if (!MainController.Instance.CanFire())
            return;

        if (FirePoint == null)
            return;

        if (CurrentWeapon.Fire(
            FirePoint,
            controller.facingRight
        ))
        {
            MainController.Instance.ConsumeBullet();
            SpawnMuzzleFlash();

            rightArmRecoil?.ApplyRecoil(
            CurrentWeapon.recoilOffset,
            CurrentWeapon.recoilKickTime,
            CurrentWeapon.recoilReturnTime,
            controller.facingRight
            );
        }
    }

    void SpawnMuzzleFlash()
    {
        if (CurrentWeapon == null)
            return;

        var flashes = CurrentWeapon.muzzleFlashSprites;
        if (flashes == null || flashes.Length == 0)
            return;

        if (FirePoint == null)
            return;

        Sprite sprite =
            flashes[Random.Range(0, flashes.Length)];

        GameObject flash = new GameObject("MuzzleFlash");

        // Position
        flash.transform.position =
            FirePoint.position + (Vector3)CurrentWeapon.muzzleFlashOffset;

        // IMPORTANT:
        // FirePoint rotation is useless for 2D when flipping on Y,
        // so we explicitly rotate on Z based on facing.
        float zRotation = controller.facingRight ? 0f : 180f;
        flash.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);

        flash.transform.localScale =
            Vector3.one * CurrentWeapon.muzzleFlashScale;

        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // Sorting
        sr.sortingLayerName = "Projectiles";
        sr.sortingOrder = 0;

        Destroy(flash, CurrentWeapon.muzzleFlashLifetime);
    }

    void Reload()
    {
        MainController.Instance.Reload();
    }

    // ---------------- WEAPON SWAP ----------------

    void SwapWeapon()
    {
        MainController.Instance.SwapWeapon();
        weaponVisuals.SetWeapon(CurrentWeapon);
    }

    // ---------------- MELEE ----------------

    void Melee()
    {
        if (IsBusy)
            return;

        StartCoroutine(MeleeRoutine());
    }


    IEnumerator MeleeRoutine()
    {
        isMeleeAttacking = true;

        GetComponent<PlayerAnimatorController>()?.TriggerMelee();

        // Optional delay to sync hit frame
        yield return new WaitForSeconds(meleeDelay);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleePoint.position,
            meleeRange,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            hit.GetComponent<EnemyHealth>()?.TakeDamage(meleeDamage);
        }

        // Total melee lock duration (tune this)
        yield return new WaitForSeconds(meleeDuration);

        isMeleeAttacking = false;
    }


    // ---------------- GRENADES ----------------

    void ThrowGrenade()
    {
        if (IsBusy)
            return;

        StartCoroutine(GrenadeRoutine());
    }

    IEnumerator GrenadeRoutine()
    {
        isThrowingGrenade = true;

        GetComponent<PlayerAnimatorController>()?.TriggerGrenade();

        // Delay to match throw animation
        yield return new WaitForSeconds(grenadeThrowDelay);

        GameObject grenade = Instantiate(
            grenadePrefab,
            grenadeSpawn.position,
            Quaternion.identity
        );

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        Vector2 direction = controller.facingRight ? Vector2.right : Vector2.left;

        float verticalBoost = controller.IsCrouching ? 1.5f : 3f;
        rb.linearVelocity =
            direction * grenadeThrowForce + Vector2.up * verticalBoost;

        // Lock until animation finishes
        yield return new WaitForSeconds(grenadeThrowDuration);

        isThrowingGrenade = false;
    }


    public bool IsBusy =>
    controller.State == PlayerController2D.PlayerState.Dodging ||
    isMeleeAttacking ||
    isThrowingGrenade;

    // ---------------- DEBUG ----------------

    void OnDrawGizmosSelected()
    {
        if (meleePoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
    }
}
