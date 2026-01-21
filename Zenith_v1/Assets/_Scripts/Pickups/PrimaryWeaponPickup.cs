using UnityEngine;

public class PrimaryWeaponPickup : MonoBehaviour
{
    public Weapon weapon;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        MainController mc = MainController.Instance;

        // Replace primary weapon
        mc.primaryWeapon = weapon;

        // Rebuild primary ammo state
        mc.primaryWeaponAmmo = new WeaponAmmoState
        {
            weaponId = weapon.weaponId,
            clipSize = weapon.clipSize,
            currentClip = weapon.clipSize,
            reserveAmmo = int.MaxValue,
            infiniteAmmo = true
        };

        // If player is currently using primary, refresh visuals
        if (mc.usingPrimary)
        {
            playerCombat?.RefreshWeaponVisuals();
        }

        Destroy(gameObject);
    }
}
