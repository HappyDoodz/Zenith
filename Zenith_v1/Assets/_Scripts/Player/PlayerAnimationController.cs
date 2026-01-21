using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    Animator anim;
    PlayerController2D controller;

    Rigidbody2D rb;

    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateMovementStates();
    }

    void UpdateMovementStates()
    {
        float speed = Mathf.Abs(rb.linearVelocity.x);

        anim.SetFloat("Speed", speed);
        anim.SetBool("IsRunning", speed > 0.1f && controller.IsGrounded);
        anim.SetBool("IsGrounded", controller.IsGrounded);
        anim.SetBool("IsCrouching", controller.IsCrouching);
        anim.SetBool("IsDashing", controller.State == PlayerController2D.PlayerState.Dodging);
    }

    // ====== CALLED BY GAMEPLAY ======

    public void SetMeleeAttacking(bool value)
    {
        anim.SetBool("IsMeleeAttacking", value);
    }

    public void TriggerGrenade()
    {
        anim.SetTrigger("ThrowGrenade");
    }
}
