using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Fire Point")]
    [SerializeField] Transform firePoint;

    WeaponVisualController weaponVisuals;
    PlayerController2D controller;

    [Header("Melee")]
    public float meleeRange = 1f;
    public int meleeDamage = 25;
    public LayerMask enemyLayer;
    public Transform meleePoint;

    [Header("Grenades")]
    public GameObject grenadePrefab;
    public Transform grenadeSpawn;
    public float grenadeThrowForce = 8f;

    Weapon CurrentWeapon => MainController.Instance.GetCurrentWeapon();

    void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        weaponVisuals = GetComponent<WeaponVisualController>();

        // Sync visuals + fire point from MainController state
        weaponVisuals.SetWeapon(CurrentWeapon);
        UpdateFirePoint();
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

        if (Input.GetKeyDown(KeyCode.G))
            ThrowGrenade();
    }

    void LateUpdate()
    {
        UpdateFirePoint();
    }

    // ---------------- SHOOTING ----------------

    void TryShoot()
    {
        if (!MainController.Instance.CanFire())
            return;

        if (CurrentWeapon.Fire(
            firePoint,
            controller.facingRight
        ))
        {
            MainController.Instance.ConsumeBullet();
        }
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
        UpdateFirePoint();
    }

    // ---------------- FIRE POINT ----------------

    void UpdateFirePoint()
    {
        if (CurrentWeapon == null)
            return;

        Vector2 offset = controller.IsCrouching
            ? CurrentWeapon.firePointCrouchingOffset
            : CurrentWeapon.firePointStandingOffset;

        firePoint.localPosition = offset;
    }

    // ---------------- MELEE ----------------

    void Melee()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleePoint.position,
            meleeRange,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            hit.GetComponent<EnemyHealth>()?.TakeDamage(meleeDamage);
        }
    }

    // ---------------- GRENADES ----------------

    void ThrowGrenade()
    {
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
    }

    // ---------------- DEBUG ----------------

    void OnDrawGizmosSelected()
    {
        if (meleePoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
    }
}
