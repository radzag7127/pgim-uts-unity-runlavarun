using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 9f;
    [SerializeField] float jumpSpeed = 14f;
    [SerializeField] float climbSpeed = 9f;
    [SerializeField] Vector2 deathKick = new Vector2(10f, 10f);
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;
    [SerializeField] float wallSlideSpeed = 2f;
    [SerializeField] float wallCheckDistance = 0.5f;
    [SerializeField] Vector2 wallJumpForce = new Vector2(6f, 14f);
    [SerializeField] LayerMask wallLayer;
    [SerializeField] float wallJumpCooldown = 0.2f;

    bool isWallSliding;
    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    float gravityScaleAtStart;
    bool isAlive = true;
    bool hasWallJumped = false;
    bool isWallJumping = false;
    float wallJumpFlipDelay = 0.1f;
    float wallJumpFlipTimer = 0f;
    float wallJumpTimer = 0f;
    int maxJumpCount = 2;
    int jumpCount = 0;

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

        if (hasWallJumped)
        {
            wallJumpFlipTimer -= Time.deltaTime;
            if (wallJumpFlipTimer <= 0f)
            {
                hasWallJumped = false;
            }
        }

        // Handle wall jump cooldown
        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0f)
            {
                isWallJumping = false;
            }
        }

        // Reset jump count when grounded
        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            jumpCount = 0;
        }
    }

    void OnFire(InputValue inputValue)
    {
        if (!isAlive) { return; }
        Instantiate(bullet, gun.position, transform.rotation);
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) { return; }
        Vector2 input = value.Get<Vector2>();

        // Detect if the player changed direction
        if (Mathf.Sign(input.x) != Mathf.Sign(moveInput.x) && input.x != 0)
        {
            hasWallJumped = false;
        }

        moveInput = input;
    }

    void OnJump(InputValue value)
    {
        if (!isAlive) { return; }
        if (value.isPressed)
        {
            if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
            {
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpSpeed);
                jumpCount = 1;
            }
            else if (isWallSliding)
            {
                float horizontalForce = -Mathf.Sign(transform.localScale.x) * wallJumpForce.x; // Push away from wall
                myRigidbody.velocity = new Vector2(horizontalForce, wallJumpForce.y);
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                isWallSliding = false;
                hasWallJumped = true;
                wallJumpFlipTimer = wallJumpFlipDelay;
                jumpCount = 1;

                // Start wall jump cooldown
                isWallJumping = true;
                wallJumpTimer = wallJumpCooldown;
            }
            else if (jumpCount < maxJumpCount)
            {
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpSpeed);
                jumpCount++;
            }
        }
    }

    void Run()
    {
        // Only allow running if not in wall jump cooldown
        if (!isWallJumping)
        {
            Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.velocity.y);
            myRigidbody.velocity = playerVelocity;

            bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
            myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
        }
    }

    void FlipSprite()
    {
        if (hasWallJumped) { return; } // Prevent flipping during delay

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
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
    }

    bool IsWallSliding()
    {
        Vector2 direction = Vector2.right * transform.localScale.x;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);
        bool touchingWall = hit.collider != null;
        bool notGrounded = !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
        bool movingDown = myRigidbody.velocity.y < 0;
        return touchingWall && notGrounded && movingDown;
    }
}
