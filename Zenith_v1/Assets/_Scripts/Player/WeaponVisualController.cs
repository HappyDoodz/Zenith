using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [SerializeField] Transform weaponHolder;

    WeaponView currentView;

    public WeaponView CurrentWeaponView => currentView;

    public Transform CurrentFirePoint =>
        currentView != null ? currentView.FirePoint : null;

    public void SetWeapon(Weapon weapon)
    {
        // Destroy old weapon view
        if (currentView != null)
            Destroy(currentView.gameObject);

        if (weapon == null || weapon.weaponPrefab == null)
        {
            currentView = null;
            return;
        }

        // Spawn new weapon prefab
        GameObject obj = Instantiate(
            weapon.weaponPrefab,
            weaponHolder
        );

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        currentView = obj.GetComponent<WeaponView>();

        if (currentView == null)
        {
            Debug.LogError(
                $"Weapon prefab {weapon.weaponPrefab.name} is missing WeaponView"
            );
        }
    }
}
