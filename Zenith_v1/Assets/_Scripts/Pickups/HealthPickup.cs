using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Health")]
    public float healAmount = 25f;

    [Tooltip("Allow healing beyond max health? (usually false)")]
    public bool overheal = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        MainController mc = MainController.Instance;
        if (mc == null)
            return;

        // Apply heal
        mc.currentHealth += healAmount;

        // Clamp if overheal is not allowed
        if (!overheal)
        {
            mc.currentHealth =
                Mathf.Min(mc.currentHealth, mc.maxHealth);
        }

        Destroy(gameObject);
    }
}
