using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Weapon weapon;
    public int startingReserveAmmo = 24;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        MainController.Instance.ReplaceSecondaryWeapon(
            weapon,
            startingReserveAmmo
        );

        Destroy(gameObject);
    }
}
