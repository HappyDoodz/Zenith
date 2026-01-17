using UnityEngine;
using System.Collections;

public class MainController : MonoBehaviour
{
    public static MainController Instance;

    // ================= PLAYER =================

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Conditions")]
    public bool canWeaponSelect = true;

    // ================= WEAPONS =================

    [Header("Equipped Weapons")]
    public Weapon primaryWeapon;     // always available (pistol)
    public Weapon secondaryWeapon;   // replaceable

    [Header("Active Slot")]
    public bool usingPrimary = true;

    [Header("Runtime Ammo State (auto-managed)")]
    public WeaponAmmoState primaryWeaponAmmo;
    public WeaponAmmoState secondaryWeaponAmmo;

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
    }

    void ClampHealth()
    {
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
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
            reserveAmmo = 0,              // starts empty unless pickup says otherwise
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



    // ================= PICKUP API =================

    public void ReplaceSecondaryWeapon(
        Weapon newWeapon,
        int startingReserveAmmo
    )
    {
        secondaryWeapon = newWeapon;

        secondaryWeaponAmmo = new WeaponAmmoState
        {
            weaponId = newWeapon.weaponId,
            clipSize = newWeapon.clipSize,
            currentClip = newWeapon.clipSize,
            reserveAmmo = startingReserveAmmo,
            infiniteAmmo = false
        };
    }
}
