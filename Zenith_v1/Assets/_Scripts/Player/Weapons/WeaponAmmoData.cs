[System.Serializable]
public class WeaponAmmoData
{
    public string weaponId;   // Must match Weapon.weaponId
    public int maxAmmo;
}

public class AmmoData
{
    public int currentAmmo;
    public int maxAmmo;
}