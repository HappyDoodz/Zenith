using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    public enum PlayerState
    {
        Normal,
        Dodging
    }

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float crouchSpeed = 3f;
    public float jumpForce = 12f;

    [Header("Dodge")]
    public float dodgeSpeed = 12f;
    public float dodgeDuration = 0.25f;
    public float dodgeCooldown = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsInvincible { get; private set; }
    public PlayerState State { get; private set; }

    Rigidbody2D rb;
    PlayerCombat combat;
    AfterImageEffect afterImage;
    float moveInput;
    public bool facingRight = true;
    bool canDodge = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        combat = GetComponent<PlayerCombat>();
        afterImage = GetComponent<AfterImageEffect>();
    }

    void Update()
    {
        if (State == PlayerState.Dodging) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        IsCrouching = Input.GetKey(KeyCode.S);

        afterImage.SetAlwaysEmitting(!IsGrounded);

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded && !IsCrouching)
        {
            Jump();
        }

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

        if (State != PlayerState.Dodging)
        {
            Move();
        }
    }

    void Move()
    {
        float speed = IsCrouching ? crouchSpeed : moveSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    IEnumerator Dodge()
    {
        State = PlayerState.Dodging;
        afterImage.EmitForDuration(dodgeDuration);
        IsInvincible = true;
        canDodge = false;

        // Cache gravity and disable it
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float direction = facingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * dodgeSpeed, 0f);

        yield return new WaitForSeconds(dodgeDuration);

        // Restore gravity
        rb.gravityScale = originalGravity;

        IsInvincible = false;
        State = PlayerState.Normal;

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    void HandleFacing()
    {
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;

        // Rotate the player root on Y (left/right)
        transform.localRotation = Quaternion.Euler(
            0f,
            facingRight ? 0f : 180f,
            0f
        );
    }

    void CheckGrounded()
    {
        IsGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }
}
