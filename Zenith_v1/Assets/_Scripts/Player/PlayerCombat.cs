using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    Transform FirePoint =>
        weaponVisuals.CurrentFirePoint;

    WeaponVisualController weaponVisuals;
    PlayerController2D controller;

    [Header("IK Recoil")]
    [SerializeField] ArmRecoilController rightArmRecoil;


    [Header("Melee")]
    public Transform meleePoint;

    [Header("Grenades")]
    public GameObject grenadePrefab;
    public Transform grenadeSpawn;
    public float grenadeThrowForce = 8f;
    public float grenadeThrowDelay = 0.5f;
    public float grenadeThrowDuration = 1f;

    [Header("Audio")]
    [SerializeField] AudioSource weaponAudio;

    bool isMeleeAttacking;
    bool isThrowingGrenade;
    bool isReloading;
    bool isFiring;
    bool wasFiring;

    Weapon CurrentWeapon => MainController.Instance.GetCurrentWeapon();

    void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        weaponVisuals = GetComponent<WeaponVisualController>();
    }
    
    void Start()
    {
        // MainController is now guaranteed to exist
        if (CurrentWeapon != null)
        {
            weaponVisuals.SetWeapons(
                MainController.Instance.primaryWeapon,
                MainController.Instance.meleeWeapon
            );
        }
    }

    void Update()
    {
        wasFiring = isFiring;
        isFiring = false;

        if (Input.GetKeyDown(KeyCode.Q))
            SwapWeapon();

        if (Input.GetKey(KeyCode.J))
            TryShoot();

        if (Input.GetKeyDown(KeyCode.I))
            Melee();

        if (Input.GetKeyDown(KeyCode.E))
            Reload();

        if (Input.GetKeyDown(KeyCode.K))
            ThrowGrenade();

        if (wasFiring && !isFiring)
        {
            PlayFireStopSound();
        }
    }

    // ---------------- SHOOTING ----------------

    void TryShoot()
    {
        if (IsBusy || isReloading)
            return;

        // 🔑 If clip is empty, try auto-reload
        if (!MainController.Instance.CanFire())
        {
            // Only reload if we actually have reserve ammo
            //if (MainController.Instance.CanReload())
            //{
                StartCoroutine(AutoReloadRoutine());
            //}
            return;
        }

        if (FirePoint == null)
            return;

        isFiring = true;

        if (CurrentWeapon.Fire(
            FirePoint,
            controller.facingRight
        ))
        {
            MainController.Instance.ConsumeBullet();
            SpawnMuzzleFlash();

            rightArmRecoil?.ApplyRecoil(
                CurrentWeapon.recoilOffset,
                CurrentWeapon.recoilKickTime,
                CurrentWeapon.recoilReturnTime,
                controller.facingRight
            );

            PlayFireSound(CurrentWeapon);
        }
    }

    void SpawnMuzzleFlash()
    {
        if (CurrentWeapon == null)
            return;

        var flashes = CurrentWeapon.muzzleFlashSprites;
        if (flashes == null || flashes.Length == 0)
            return;

        if (FirePoint == null)
            return;

        Sprite sprite =
            flashes[Random.Range(0, flashes.Length)];

        GameObject flash = new GameObject("MuzzleFlash");

        // Position
        flash.transform.position =
            FirePoint.position + (Vector3)CurrentWeapon.muzzleFlashOffset;

        // IMPORTANT:
        // FirePoint rotation is useless for 2D when flipping on Y,
        // so we explicitly rotate on Z based on facing.
        float zRotation = controller.facingRight ? 0f : 180f;
        flash.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);

        flash.transform.localScale =
            Vector3.one * CurrentWeapon.muzzleFlashScale;

        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // Sorting
        sr.sortingLayerName = "Projectiles";
        sr.sortingOrder = 0;

        Destroy(flash, CurrentWeapon.muzzleFlashLifetime);
    }

    void PlayFireSound(Weapon weapon)
    {
            if (weapon == null)
            return;
    
        var clips = weapon.fireSounds;
        if (clips == null || clips.Length == 0)
            return;
    
        AudioClip clip =
            clips[Random.Range(0, clips.Length)];
    
        weaponAudio.pitch =
            Random.Range(weapon.pitchMin, weapon.pitchMax);
    
        weaponAudio.volume = weapon.fireVolume;
        weaponAudio.PlayOneShot(clip);
    }

    void PlayReloadSound()
    {
        if (CurrentWeapon == null)
            return;

        if (CurrentWeapon.reloadSound == null)
            return;

        weaponAudio.pitch = 1f; // keep reload stable
        weaponAudio.volume = CurrentWeapon.reloadVolume;
        weaponAudio.PlayOneShot(CurrentWeapon.reloadSound);
    }

    void PlayReadySound()
    {
        if (CurrentWeapon == null)
            return;

        if (CurrentWeapon.readySound == null)
            return;

        weaponAudio.pitch = 1f;
        weaponAudio.volume = CurrentWeapon.readyVolume;
        weaponAudio.PlayOneShot(CurrentWeapon.readySound);
    }

    void PlayFireStopSound()
    {
        if (CurrentWeapon == null)
            return;

        if (CurrentWeapon.fireStopSound == null)
            return;

        weaponAudio.pitch = 1f;
        weaponAudio.volume = CurrentWeapon.fireStopVolume;
        weaponAudio.PlayOneShot(CurrentWeapon.fireStopSound);
    }

    void Reload()
    {
        if (isReloading)
            return;

        StartCoroutine(AutoReloadRoutine());
    }

    IEnumerator AutoReloadRoutine()
    {
        isReloading = true;

        PlayReloadSound();

        MainController.Instance.Reload();

        // Wait for weapon-specific reload time
        yield return new WaitForSeconds(CurrentWeapon.reloadTime);

        isReloading = false;
    }

    // ---------------- WEAPON SWAP ----------------

    void SwapWeapon()
    {
        MainController.Instance.SwapWeapon();
    
        // Refresh visuals using the CURRENT equipped ranged weapon
        weaponVisuals.SetRangedWeapon(MainController.Instance.GetCurrentWeapon());
    
        // (Optional) refresh melee too if it can change via pickups
        weaponVisuals.SetMeleeWeapon(MainController.Instance.meleeWeapon);
    
        PlayReadySound();
    }
    
    public void RefreshWeaponVisuals()
    {
        // Always refresh ranged weapon based on current slot
        weaponVisuals.SetRangedWeapon(MainController.Instance.GetCurrentWeapon());
    
        // Refresh melee if applicable
        weaponVisuals.SetMeleeWeapon(MainController.Instance.meleeWeapon);
    }

    // ---------------- MELEE ----------------

    void Melee()
    {
        Weapon weapon = MainController.Instance.meleeWeapon;

        if (weapon == null || !weapon.isMelee)
            return;

        if (IsBusy || isMeleeAttacking)
            return;

        StartCoroutine(MeleeRoutine(weapon));
    }

    IEnumerator MeleeRoutine(Weapon weapon)
    {
        isMeleeAttacking = true;

        GetComponent<PlayerAnimatorController>()
            ?.SetMeleeAttacking(true);

        PlayFireSound(weapon);

        yield return new WaitForSeconds(weapon.meleeWindup);

        float timer = weapon.meleeActiveTime;
        while (timer > 0f)
        {
            DoMeleeHit(weapon);
            timer -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(weapon.meleeRecovery);

        GetComponent<PlayerAnimatorController>()
            ?.SetMeleeAttacking(false);

        isMeleeAttacking = false;
    }

    void DoMeleeHit(Weapon weapon)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleePoint.position,
            weapon.meleeRange,
            weapon.meleeHitLayers
        );

        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null) continue;

            enemy.TakeDamage(weapon.meleeDamage);

            Rigidbody2D rb = hit.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir =
                    (hit.transform.position - transform.position).normalized;
                rb.AddForce(dir * weapon.meleeKnockback, ForceMode2D.Impulse);
            }
        }
    }

    // ---------------- GRENADES ----------------

    void ThrowGrenade()
    {
        if (IsBusy)
            return;

        StartCoroutine(GrenadeRoutine());
    }

    IEnumerator GrenadeRoutine()
    {
        isThrowingGrenade = true;

        GetComponent<PlayerAnimatorController>()?.TriggerGrenade();

        // Delay to match throw animation
        yield return new WaitForSeconds(grenadeThrowDelay);

        GameObject grenade = Instantiate(
            grenadePrefab,
            grenadeSpawn.position,
            Quaternion.identity
        );

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        Vector2 direction = controller.facingRight ? Vector2.right : Vector2.left;

        float verticalBoost = controller.IsCrouching ? 1.5f : 3f;
        rb.linearVelocity =
            direction * grenadeThrowForce + Vector2.up * verticalBoost;

        // Lock until animation finishes
        yield return new WaitForSeconds(grenadeThrowDuration);

        isThrowingGrenade = false;
    }


    public bool IsBusy =>
    controller.State == PlayerController2D.PlayerState.Dodging ||
    isMeleeAttacking ||
    isThrowingGrenade;

    // ---------------- DEBUG ----------------

    void OnDrawGizmosSelected()
    {
        if (meleePoint == null)
            return;

        Weapon weapon =
            MainController.Instance != null
                ? MainController.Instance.meleeWeapon
                : null;

        if (weapon == null || !weapon.isMelee)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            meleePoint.position,
            weapon.meleeRange
        );
    }
}
