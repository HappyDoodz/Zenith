using UnityEngine;

public class GrenadePickup : MonoBehaviour
{
    public GameObject grenadePrefab;
    public string grenadeName = "Grenade";
    public int bonusGrenades = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        MainController mc = MainController.Instance;
        if (mc == null)
            return;

        // Set grenade type + name
        mc.SetGrenade(grenadePrefab, grenadeName);

        // Optional ammo grant
        mc.AddGrenades(bonusGrenades);

        Destroy(gameObject);
    }
}
