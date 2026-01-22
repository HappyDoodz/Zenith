using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController2D : MonoBehaviour
{
    public enum PlayerState
    {
        Normal,
        Dodging,
        ElevatorLocked,
        Dead,
        IntroLocked
    }

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float crouchSpeed = 3f;
    public float jumpForce = 12f;

    [Header("Dodge")]
    public float dodgeSpeed = 12f;
    public float dodgeDuration = 0.25f;
    public float dodgeCooldown = 0.5f;
    public string dodgeLayerName = "PlayerDash";

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Audio")]
    public AudioClip[] jumpSounds;
    public AudioClip[] dodgeSounds;
    [Range(0.9f, 1.1f)] public float pitchMin = 0.95f;
    [Range(0.9f, 1.1f)] public float pitchMax = 1.05f;

    public bool IsGrounded { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsInvincible { get; private set; }
    public PlayerState State { get; private set; } = PlayerState.IntroLocked;

    Rigidbody2D rb;
    PlayerCombat combat;
    PlayerHealth health;
    AfterImageEffect afterImage;
    public AudioSource audioSource;

    float moveInput;
    public bool facingRight = true;
    bool canDodge = true;

    public bool CanAct =>
    State == PlayerState.Normal;

    int dodgeLayer;

    // Sprite sorting restore data
    Dictionary<SpriteRenderer, string> originalSortingLayers =
        new Dictionary<SpriteRenderer, string>();

    // ================= UNITY =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
        dodgeLayer = LayerMask.NameToLayer(dodgeLayerName);
    }

    void Start()
    {
        combat = GetComponent<PlayerCombat>();
        afterImage = GetComponent<AfterImageEffect>();

        MusicManager.Instance?.PlayGameplayMusic();

        if (health != null)
            health.OnDeath += HandleDeath;

        StartCoroutine(IntroLockRoutine());
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    // ================= INTRO LOCK =================

    IEnumerator IntroLockRoutine()
    {
        CacheAndMoveSpritesToBackground();

        yield return new WaitForSeconds(1f);

        RestoreSpriteSortingLayers();
        State = PlayerState.Normal;
    }

    void CacheAndMoveSpritesToBackground()
    {
        originalSortingLayers.Clear();

        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in renderers)
        {
            originalSortingLayers[sr] = sr.sortingLayerName;
            sr.sortingLayerName = "Walls";
        }
    }

    void RestoreSpriteSortingLayers()
    {
        foreach (var pair in originalSortingLayers)
        {
            if (pair.Key != null)
                pair.Key.sortingLayerName = pair.Value;
        }

        originalSortingLayers.Clear();
    }

    // ================= UPDATE =================

    void Update()
    {
        if (State == PlayerState.Dead ||
            State == PlayerState.Dodging ||
            State == PlayerState.ElevatorLocked ||
            State == PlayerState.IntroLocked)
            return;

        moveInput = Input.GetAxisRaw("Horizontal");
        IsCrouching = Input.GetKey(KeyCode.S);

        afterImage?.SetAlwaysEmitting(!IsGrounded);

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded && !IsCrouching)
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDodge &&
            (combat == null || !combat.IsBusy))
        {
            StartCoroutine(Dodge());
        }

        HandleFacing();
    }

    void FixedUpdate()
    {
        CheckGrounded();

        if (State == PlayerState.Dead ||
            State == PlayerState.ElevatorLocked ||
            State == PlayerState.IntroLocked)
            return;

        if (State != PlayerState.Dodging)
            Move();
    }

    // ================= MOVEMENT =================

    void Move()
    {
        float speed = IsCrouching ? crouchSpeed : moveSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        PlayJumpSound();
    }

    // ================= DODGE =================

    IEnumerator Dodge()
    {
        State = PlayerState.Dodging;
        IsInvincible = true;
        canDodge = false;

        PlayDodgeSound();
        afterImage?.EmitForDuration(dodgeDuration);

        float originalGravity = rb.gravityScale;
        int originalLayer = gameObject.layer;

        rb.gravityScale = 0f;
        gameObject.layer = dodgeLayer;

        float direction = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * dodgeSpeed, 0f);

        yield return new WaitForSeconds(dodgeDuration);

        rb.gravityScale = originalGravity;
        gameObject.layer = originalLayer;

        IsInvincible = false;
        State = PlayerState.Normal;

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    // ================= AUDIO =================

    void PlayJumpSound()
    {
        if (jumpSounds == null || jumpSounds.Length == 0)
            return;

        AudioClip clip =
            jumpSounds[UnityEngine.Random.Range(0, jumpSounds.Length)];

        audioSource.pitch =
            UnityEngine.Random.Range(pitchMin, pitchMax);

        audioSource.PlayOneShot(clip);
    }

    void PlayDodgeSound()
    {
        if (dodgeSounds == null || dodgeSounds.Length == 0)
            return;

        AudioClip clip =
            dodgeSounds[UnityEngine.Random.Range(0, dodgeSounds.Length)];

        audioSource.pitch =
            UnityEngine.Random.Range(pitchMin, pitchMax);

        audioSource.PlayOneShot(clip);
    }

    // ================= FACING =================

    void HandleFacing()
    {
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;

        transform.localRotation = Quaternion.Euler(
            0f,
            facingRight ? 0f : 180f,
            0f
        );
    }

    // ================= GROUND =================

    void CheckGrounded()
    {
        IsGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    // ================= DEATH =================

    void HandleDeath()
    {
        State = PlayerState.Dead;
        IsInvincible = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    // ================= ELEVATOR =================

    public void EnterElevator()
    {
        if (State == PlayerState.Dead)
            return;

        State = PlayerState.ElevatorLocked;

        rb.linearVelocity = Vector2.zero;
        CacheAndMoveSpritesToBackground();
    }
}
