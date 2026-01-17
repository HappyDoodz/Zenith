using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform weaponHolder;
    [SerializeField] SpriteRenderer weaponRenderer;

    PlayerController2D controller;
    Weapon currentWeapon;

    Vector3 baseLocalPosition;

    void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        baseLocalPosition = weaponHolder.localPosition;
    }

    public void SetWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        UpdateVisual();
    }

    void LateUpdate()
    {
        if (currentWeapon == null)
            return;

        UpdateVisual();
    }

    void UpdateVisual()
    {
        // -------- SPRITE + OFFSET --------
        if (controller.IsCrouching)
        {
            weaponRenderer.sprite =
                currentWeapon.crouchingSprite != null
                ? currentWeapon.crouchingSprite
                : currentWeapon.standingSprite;

            weaponHolder.localPosition =
                baseLocalPosition + (Vector3)currentWeapon.spriteOffsetCrouching;
        }
        else
        {
            weaponRenderer.sprite = currentWeapon.standingSprite;

            weaponHolder.localPosition =
                baseLocalPosition + (Vector3)currentWeapon.spriteOffsetStanding;
        }
    }
}
