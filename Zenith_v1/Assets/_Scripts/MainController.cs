using UnityEngine;
using System.Collections;

public class MainController : MonoBehaviour
{
    public static MainController Instance;

    // ================= PLAYER =================

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Body Armour")]
    public float maxArmour = 50f;
    public float currentArmour = 0f;

    [Header("Conditions")]
    public bool canWeaponSelect = true;

    // ================= WEAPONS =================

    [Header("Equipped Weapons")]
    public Weapon primaryWeapon;     // always available (pistol)
    public Weapon secondaryWeapon;   // replaceable

    [Header("Melee Weapon")]
    public Weapon meleeWeapon;

    [Header("Active Slot")]
    public bool usingPrimary = true;

    [Header("Runtime Ammo State (auto-managed)")]
    public WeaponAmmoState primaryWeaponAmmo;
    public WeaponAmmoState secondaryWeaponAmmo;

    // ================= GRENADES =================

    [Header("Grenades")]
    public GameObject grenadePrefab;
    public string grenadeName = "Frag Grenade";
    public int maxGrenades = 5;
    public int currentGrenades = 3;

    bool initialized;

    // ================= UNITY =================

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeIfNeeded();
    }

    void Update()
    {
        ClampHealth();
        ClampArmour();
    }

    void ClampHealth()
    {
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    void ClampArmour()
    {
        if (currentArmour < 0)
            currentArmour = 0;

        if (currentArmour > maxArmour)
            currentArmour = maxArmour;
    }

    public void ApplyDamage(float damage)
    {
        if (damage <= 0)
            return;

        // Armour absorbs damage first
        if (currentArmour > 0)
        {
            float absorbed = Mathf.Min(currentArmour, damage);
            currentArmour -= absorbed;
            damage -= absorbed;
        }

        // Remaining damage hits health
        if (damage > 0)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
        }
    }

    public void AddArmour(float amount)
    {
        if (amount <= 0)
            return;

        currentArmour += amount;
        ClampArmour();
    }

    // ================= INITIALIZATION =================

    void InitializeIfNeeded()
    {
        if (initialized)
            return;

        BuildPrimaryAmmo();
        BuildSecondaryAmmo();

        initialized = true;
    }

    void BuildPrimaryAmmo()
    {
        if (primaryWeapon == null)
            return;

        primaryWeaponAmmo = new WeaponAmmoState
        {
            weaponId = primaryWeapon.weaponId,
            clipSize = primaryWeapon.clipSize,
            currentClip = primaryWeapon.clipSize,
            reserveAmmo = int.MaxValue,   // infinite reserve
            infiniteAmmo = true
        };
    }

    void BuildSecondaryAmmo()
    {
        if (secondaryWeapon == null)
            return;

        secondaryWeaponAmmo = new WeaponAmmoState
        {
            weaponId = secondaryWeapon.weaponId,
            clipSize = secondaryWeapon.clipSize,
            currentClip = secondaryWeapon.clipSize,
            reserveAmmo = 120,              // starts empty unless pickup says otherwise
            infiniteAmmo = false
        };
    }

    // ================= WEAPON HELPERS =================

    public Weapon GetCurrentWeapon()
    {
        return usingPrimary ? primaryWeapon : secondaryWeapon;
    }

    public WeaponAmmoState GetCurrentAmmoState()
    {
        return usingPrimary ? primaryWeaponAmmo : secondaryWeaponAmmo;
    }

    public void SwapWeapon()
    {
        if (!canWeaponSelect)
            return;

        usingPrimary = !usingPrimary;
    }

    // ================= AMMO API =================

    public bool CanFire()
    {
        WeaponAmmoState ammo = GetCurrentAmmoState();
        return ammo.currentClip > 0 && !ammo.isReloading;
    }

    public void ConsumeBullet()
    {
        WeaponAmmoState ammo = GetCurrentAmmoState();
        ammo.currentClip = Mathf.Max(0, ammo.currentClip - 1);
    }

    public void Reload()
    {
        Weapon weapon = GetCurrentWeapon();
        WeaponAmmoState ammo = GetCurrentAmmoState();

        if (ammo.isReloading)
            return;

        if (ammo.currentClip >= ammo.clipSize)
            return;

        if (!ammo.infiniteAmmo && ammo.reserveAmmo <= 0)
            return;

        StartCoroutine(ReloadRoutine(weapon, ammo));
    }

    IEnumerator ReloadRoutine(Weapon weapon, WeaponAmmoState ammo)
    {
        ammo.isReloading = true;

        yield return new WaitForSeconds(weapon.reloadTime);

        int needed = ammo.clipSize - ammo.currentClip;

        if (ammo.infiniteAmmo)
        {
            ammo.currentClip = ammo.clipSize;
        }
        else
        {
            int taken = Mathf.Min(needed, ammo.reserveAmmo);
            ammo.currentClip += taken;
            ammo.reserveAmmo -= taken;
        }

        ammo.isReloading = false;
    }

    public void AddSecondaryAmmo(int amount)
    {
        if (secondaryWeapon == null)
            return;

        if (secondaryWeaponAmmo == null)
            return;

        // Add to reserve ammo
        secondaryWeaponAmmo.reserveAmmo += amount;
    }

    public bool CanThrowGrenade()
    {
        return grenadePrefab != null && currentGrenades > 0;
    }

    public GameObject GetGrenadePrefab()
    {
        return grenadePrefab;
    }

    public void ConsumeGrenade()
    {
        currentGrenades = Mathf.Max(0, currentGrenades - 1);
    }

    public void AddGrenades(int amount)
    {
        if (amount <= 0)
            return;

        currentGrenades += amount;
        currentGrenades = Mathf.Min(currentGrenades, maxGrenades);
    }

    public void SetGrenade(
        GameObject newPrefab,
        string newName
    )
    {
        grenadePrefab = newPrefab;
        grenadeName = newName;
    }

    // ================= PICKUP API =================

    public void ReplaceSecondaryWeapon(
        Weapon newWeapon,
        int ammoAmount
    )
    {
        // If we already have this weapon equipped, just ADD ammo
        if (secondaryWeapon != null &&
            secondaryWeapon.weaponId == newWeapon.weaponId)
        {
            secondaryWeaponAmmo.reserveAmmo += ammoAmount;
            return;
        }

        // Otherwise: replace the weapon entirely
        secondaryWeapon = newWeapon;

        secondaryWeaponAmmo = new WeaponAmmoState
        {
            weaponId = newWeapon.weaponId,
            clipSize = newWeapon.clipSize,
            currentClip = newWeapon.clipSize,
            reserveAmmo = ammoAmount,
            infiniteAmmo = false
        };
    }
}
