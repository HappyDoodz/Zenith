using UnityEngine;

public class GrenadeAmmoPickup : MonoBehaviour
{
    public int grenadeAmount = 2;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        MainController mc = MainController.Instance;
        if (mc == null)
            return;

        mc.AddGrenades(grenadeAmount);

        Destroy(gameObject);
    }
}
