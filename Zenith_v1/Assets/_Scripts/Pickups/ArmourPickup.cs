using UnityEngine;

public class ArmourPickup : MonoBehaviour
{
    public float armourAmount = 25f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        GetComponent<PickupSound>()?.PlayPickupSound();

        MainController.Instance.AddArmour(armourAmount);
        Destroy(gameObject);
    }
}
