using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string weaponId;
    public string weaponName;

    [Header("Visual Prefab")]
    public GameObject weaponPrefab;

    // ================= RANGED =================

    [Header("Ranged Combat")]
    public bool isRanged = true;

    public GameObject projectilePrefab;
    public float fireRate = 0.2f;
    public float projectileSpeed = 15f;

    [Header("Ammo")]
    public int clipSize = 12;
    public bool infiniteAmmo;

    [Header("Reload")]
    public float reloadTime = 1.2f;

    // ================= MELEE =================

    [Header("Melee Combat")]
    public bool isMelee = false;

    public int meleeDamage = 25;
    public float meleeRange = 1f;

    public float meleeWindup = 0.15f;
    public float meleeActiveTime = 0.2f;
    public float meleeRecovery = 0.3f;

    public float meleeKnockback = 4f;
    public LayerMask meleeHitLayers;

    // ================= FX =================

    [Header("Muzzle Flash (Ranged Only)")]
    public Sprite[] muzzleFlashSprites;
    public float muzzleFlashLifetime = 0.05f;
    public float muzzleFlashScale = 1f;
    public Vector2 muzzleFlashOffset;

    [Header("Recoil")]
    public Vector2 recoilOffset = new Vector2(0f, 0.05f);
    public float recoilKickTime = 0.05f;
    public float recoilReturnTime = 0.08f;

    // ================= AUDIO =================

    [Header("Audio")]
    public AudioClip[] fireSounds;
    public AudioClip reloadSound;
    public AudioClip readySound;
    public AudioClip fireStopSound;

    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    public float fireVolume = 1f;
    public float reloadVolume = 1f;
    public float readyVolume = 1f;
    public float fireStopVolume = 1f;

    float lastFireTime;

    // ================= RANGED FIRE =================

    public bool Fire(Transform firePoint, bool facingRight)
    {
        if (!isRanged)
            return false;

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
            Vector2 direction =
                facingRight ? Vector2.right : Vector2.left;
            rb.linearVelocity = direction * projectileSpeed;
        }

        lastFireTime = Time.time;
        return true;
    }
}
