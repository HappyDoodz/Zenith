using UnityEngine;

public class MeleeWeaponPickup : MonoBehaviour
{
    public Weapon weapon;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        GetComponent<PickupSound>()?.PlayPickupSound();

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        MainController.Instance.meleeWeapon = weapon;

        // Force melee visual refresh
        playerCombat?.RefreshWeaponVisuals();

        Destroy(gameObject);
    }
}
