using UnityEngine;

public class WeaponView : MonoBehaviour
{
    [SerializeField] Transform firePoint;

    public Transform FirePoint => firePoint;
}
