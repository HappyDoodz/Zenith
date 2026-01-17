using UnityEngine;

[System.Serializable]
public class WeaponAmmoState
{
    public string weaponId;
    public int clipSize;
    public int currentClip;
    public int reserveAmmo;
    public bool infiniteAmmo;

    [HideInInspector] public bool isReloading;
}
