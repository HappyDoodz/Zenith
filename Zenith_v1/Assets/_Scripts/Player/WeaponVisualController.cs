using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [Header("Weapon Holders")]
    [SerializeField] Transform rangedWeaponHolder;
    [SerializeField] Transform meleeWeaponHolder;

    WeaponView rangedView;
    WeaponView meleeView;

    public WeaponView RangedWeaponView => rangedView;

    // Used by PlayerCombat for firing
    public Transform CurrentFirePoint =>
        rangedView != null ? rangedView.FirePoint : null;

    // ================= PUBLIC API =================

    /// <summary>
    /// Call this when weapons are assigned or replaced
    /// </summary>
    public void SetWeapons(Weapon rangedWeapon, Weapon meleeWeapon)
    {
        SetRangedWeapon(rangedWeapon);
        SetMeleeWeapon(meleeWeapon);
    }

    public void SetRangedWeapon(Weapon weapon)
    {
        if (rangedView != null)
            Destroy(rangedView.gameObject);

        rangedView = null;

        if (weapon == null || weapon.weaponPrefab == null)
            return;

        GameObject obj = Instantiate(
            weapon.weaponPrefab,
            rangedWeaponHolder
        );

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        rangedView = obj.GetComponent<WeaponView>();

        if (rangedView == null)
        {
            Debug.LogError(
                $"Ranged weapon prefab {weapon.weaponPrefab.name} is missing WeaponView"
            );
        }
    }

    public void SetMeleeWeapon(Weapon weapon)
    {
        if (meleeView != null)
            Destroy(meleeView.gameObject);

        meleeView = null;

        if (weapon == null || weapon.weaponPrefab == null)
            return;

        GameObject obj = Instantiate(
            weapon.weaponPrefab,
            meleeWeaponHolder
        );

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        meleeView = obj.GetComponent<WeaponView>();

        if (meleeView == null)
        {
            Debug.LogError(
                $"Melee weapon prefab {weapon.weaponPrefab.name} is missing WeaponView"
            );
        }
    }
}
