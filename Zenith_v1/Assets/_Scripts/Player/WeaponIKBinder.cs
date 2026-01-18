using UnityEngine;

public class WeaponIKBinder : MonoBehaviour
{
    [SerializeField] Transform leftHandIKTarget;
    [SerializeField] WeaponVisualController weaponVisuals;

    void LateUpdate()
    {
        if (weaponVisuals == null)
            return;

        WeaponView view = weaponVisuals.CurrentWeaponView;
        if (view == null || view.LeftHandGrip == null)
            return;

        leftHandIKTarget.position = view.LeftHandGrip.position;
        //leftHandIKTarget.rotation = view.LeftHandGrip.rotation;
    }
}
