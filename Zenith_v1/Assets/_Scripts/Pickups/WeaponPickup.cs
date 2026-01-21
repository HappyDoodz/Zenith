using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Weapon weapon;
    public int startingReserveAmmo = 24;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        GetComponent<PickupSound>()?.PlayPickupSound();

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        MainController.Instance.ReplaceSecondaryWeapon(
            weapon,
            startingReserveAmmo
        );

        playerCombat?.RefreshWeaponVisuals();

        Destroy(gameObject);
    }
}
