using UnityEngine;

public class WeaponView : MonoBehaviour
{
    [SerializeField] Transform firePoint;
    [SerializeField] Transform leftHandGrip;

    public Transform FirePoint => firePoint;
    public Transform LeftHandGrip => leftHandGrip;
}