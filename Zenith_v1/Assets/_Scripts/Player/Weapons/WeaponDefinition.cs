using UnityEngine;

[System.Serializable]
public class WeaponDefinition
{
    public string weaponId;
    public string weaponName;

    [Header("Visuals")]
    public Sprite standingSprite;
    public Sprite crouchingSprite;

    [Header("Combat")]
    public float fireRate = 0.15f;
    public int clipSize = 12;
    public bool infiniteAmmo = false;

    [Header("Fire Points")]
    public Transform firePointStanding;
    public Transform firePointCrouching;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
}
