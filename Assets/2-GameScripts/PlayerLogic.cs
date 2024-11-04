using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 15f;
    [SerializeField] float climbSpeed = 10f;
    [SerializeField] Vector2 deathKick = new Vector2(10f, 10f);
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;
    [SerializeField] float wallSlideSpeed = 2f;
    [SerializeField] float wallCheckDistance = 0.5f;
    [SerializeField] Vector2 wallJumpForce = new Vector2(15f, 15f);
    [SerializeField] LayerMask wallLayer;

    bool isWallSliding;
    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    float gravityScaleAtStart;
    bool isAlive = true;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        gravityScaleAtStart = myRigidbody.gravityScale;
    }

    void Update()
    {
        if (!isAlive) { return; }
        Run();
        FlipSprite();
        ClimbLadder();
        WallSlide();
        Die();
    }

    void OnFire(InputValue inputValue)
    {
        if (!isAlive) { return; }
        Instantiate(bullet, gun.position, transform.rotation);
    }

    //TO DO : Acceleration System
    void OnMove(InputValue value)
    {
        if (!isAlive) { return; }
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (!isAlive) { return; }
        if (value.isPressed)
        {
            if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
            {
                myRigidbody.velocity += new Vector2(0f, jumpSpeed);
            }
            else if (isWallSliding)
            {
                myRigidbody.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpForce.x, wallJumpForce.y);
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                isWallSliding = false;
            }
        }
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            if (!isWallSliding)
            {
                transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
            }
        }
    }


    void ClimbLadder()
    {

        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myRigidbody.gravityScale = gravityScaleAtStart;
            myAnimator.SetBool("isClimbing", false);
            return;
        }

        Vector2 climbVelocity = new Vector2(myRigidbody.velocity.x, moveInput.y * climbSpeed);
        myRigidbody.velocity = climbVelocity;
        myRigidbody.gravityScale = 0f;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("isClimbing", playerHasVerticalSpeed);
    }

    void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            Debug.Log("Player has touched a hazard!");
            isAlive = false;
            myAnimator.SetTrigger("Dying");
            myRigidbody.velocity = deathKick;
            FindObjectOfType<GameSession>().ProcessPlayerDeath();

        }
    }

    void WallSlide()
    {
        if (IsWallSliding())
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, -wallSlideSpeed);
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
        myAnimator.SetBool("isWallSliding", isWallSliding);
    }

    bool IsWallSliding()
    {
        bool touchingWall = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, wallCheckDistance, wallLayer);
        bool notGrounded = !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
        bool movingDown = myRigidbody.velocity.y < 0;
        return touchingWall && notGrounded && movingDown;
    }

}
