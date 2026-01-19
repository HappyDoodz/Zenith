using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Get current secondary weapon
        Weapon secondary = MainController.Instance.secondaryWeapon;

        if (secondary == null)
            return;

        // Add ammo equal to this weapon’s clip size
        MainController.Instance.AddSecondaryAmmo(secondary.clipSize);

        Destroy(gameObject);
    }
}
