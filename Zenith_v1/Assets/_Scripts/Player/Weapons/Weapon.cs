using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string weaponId;
    public string weaponName;

    [Header("Visual Prefab")]
    public GameObject weaponPrefab;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public float fireRate = 0.2f;
    public float projectileSpeed = 15f;

    //[Header("Fire Point Offsets")]
    //public Vector2 firePointStandingOffset;
    //public Vector2 firePointCrouchingOffset;

    [Header("Ammo")]
    public int clipSize = 12;
    [HideInInspector] public int currentClip;
    public bool infiniteAmmo;

    [Header("Reload")]
    public float reloadTime = 1.2f; // seconds

    [Header("Muzzle Flash")]
    public Sprite[] muzzleFlashSprites;
    public float muzzleFlashLifetime = 0.05f;
    public float muzzleFlashScale = 1f;
    public Vector2 muzzleFlashOffset;

    [Header("Recoil")]
    public Vector2 recoilOffset = new Vector2(-0.05f, 0.02f);
    public float recoilKickTime = 0.05f;
    public float recoilReturnTime = 0.08f;

    float lastFireTime;

    /// <summary>
    /// Fires the weapon using a player-owned fire point.
    /// Returns true if a shot was actually fired.
    /// </summary>
    public bool Fire(
        Transform firePoint,
        bool facingRight
    )
    {
        if (Time.time < lastFireTime + fireRate)
            return false;

        GameObject projectile = Object.Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            rb.linearVelocity = direction * projectileSpeed;
        }

        lastFireTime = Time.time;
        return true;
    }
}
