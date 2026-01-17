using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponDefinition definition;

    public int currentClip;

    float lastFireTime;

    public void Initialize()
    {
        currentClip = definition.clipSize;
    }

    public bool CanFire()
    {
        if (currentClip <= 0)
            return false;

        return Time.time >= lastFireTime + definition.fireRate;
    }

    public bool Fire(bool crouching, bool facingRight)
    {
        if (!CanFire())
            return false;

        Transform firePoint = crouching
            ? definition.firePointCrouching
            : definition.firePointStanding;

        GameObject projectile = Object.Instantiate(
            definition.projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            rb.linearVelocity = dir * definition.projectileSpeed;
        }

        currentClip--;
        lastFireTime = Time.time;
        return true;
    }

    public void Reload(int reserveAmmo)
    {
        int needed = definition.clipSize - currentClip;
        int taken = definition.infiniteAmmo
            ? needed
            : Mathf.Min(needed, reserveAmmo);

        currentClip += taken;
    }
}
