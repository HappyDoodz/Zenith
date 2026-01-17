using UnityEngine;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("Primary UI")]
    [SerializeField] CanvasGroup primaryGroup;
    [SerializeField] TextMeshProUGUI primaryName;
    [SerializeField] TextMeshProUGUI primaryClip;
    [SerializeField] TextMeshProUGUI primaryAmmo;

    [Header("Secondary UI")]
    [SerializeField] CanvasGroup secondaryGroup;
    [SerializeField] TextMeshProUGUI secondaryName;
    [SerializeField] TextMeshProUGUI secondaryClip;
    [SerializeField] TextMeshProUGUI secondaryAmmo;

    [Header("Opacity")]
    [SerializeField] float activeAlpha = 1f;
    [SerializeField] float inactiveAlpha = 0.35f;

    void Update()
    {
        if (MainController.Instance == null)
            return;

        UpdatePrimary();
        UpdateSecondary();
        UpdateActiveState();
    }

    void UpdatePrimary()
    {
        Weapon weapon = MainController.Instance.primaryWeapon;
        WeaponAmmoState ammo = MainController.Instance.primaryWeaponAmmo;

        if (weapon == null || ammo == null)
            return;

        primaryName.text = weapon.weaponName;
        primaryClip.text = ammo.currentClip.ToString();
        primaryAmmo.text = ammo.infiniteAmmo
            ? "∞"
            : ammo.reserveAmmo.ToString();
    }

    void UpdateSecondary()
    {
        Weapon weapon = MainController.Instance.secondaryWeapon;
        WeaponAmmoState ammo = MainController.Instance.secondaryWeaponAmmo;

        if (weapon == null || ammo == null)
            return;

        secondaryName.text = weapon.weaponName;
        secondaryClip.text = ammo.currentClip.ToString();
        secondaryAmmo.text = ammo.reserveAmmo.ToString();
    }

    void UpdateActiveState()
    {
        bool usingPrimary = MainController.Instance.usingPrimary;

        primaryGroup.alpha   = usingPrimary ? activeAlpha : inactiveAlpha;
        secondaryGroup.alpha = usingPrimary ? inactiveAlpha : activeAlpha;
    }
}
