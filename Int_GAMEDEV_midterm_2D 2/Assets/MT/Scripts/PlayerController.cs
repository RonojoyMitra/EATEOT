using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;

    [Header("Horizontal Movement")]
    [SerializeField]
    AnimationCurve walkCurve;
    [SerializeField]
    float maxWalkSpeed;

    [Header("Gravity")]
    [SerializeField]
    float gravityAcceloration;
    float gravityForce = 0f;
    [SerializeField]
    float maxGravityForce;
    [SerializeField]
    Vector2 leftGroundCheckOrigin, rightGroundCheckOrigin;
    [SerializeField]
    float groundCheckDistance;

    enum JumpState { CAN_JUMP, JUMPING, FALLING };
    [Header("Jumping")]
    [SerializeField]
    JumpState jumpState;
    bool jumpFlag;
    [SerializeField]
    float initialJumpForce;
    [SerializeField]
    float jumpDeadening, jumpDeadeningSlow;
    float jumpDeadeningForce;
    float jumpForce;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            jumpFlag = true;
    }

    void FixedUpdate()
    {
        if (!OnGround() && (jumpState == JumpState.CAN_JUMP))
        {
            jumpState = JumpState.FALLING;
        }

        if (jumpState == JumpState.FALLING)
            Gravity();

        if (jumpFlag && jumpState == JumpState.CAN_JUMP)
        {
            jumpFlag = false;
            Jump();
        }
        if (jumpState == JumpState.JUMPING)
        {
            JumpDeaden();
        }

        rb.MovePosition(TargetPosition());

        if (OnGround() && jumpState == JumpState.FALLING)
        {
            jumpState = JumpState.CAN_JUMP;
        }
    }

    void Jump()
    {
        jumpState = JumpState.JUMPING;
        jumpForce = initialJumpForce;
        jumpDeadeningForce = 0f;
    }

    void JumpDeaden()
    {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            jumpDeadeningForce += jumpDeadeningSlow * Time.deltaTime;
        }
        else
        {
            jumpDeadeningForce += jumpDeadening * Time.deltaTime;
        }
        jumpForce -= jumpDeadeningForce;
        if (jumpForce < 0)
        {
            jumpState = JumpState.FALLING;
        }
    }

    void Gravity()
    {
        if (!OnGround())
        {
            gravityForce += Time.deltaTime * gravityAcceloration;
        }
        else
        {
            gravityForce = 0f;
        }
        if (gravityForce > maxGravityForce) gravityForce = maxGravityForce;
    }

    bool OnGround()
    {
        // Raycast from both left and right bottom corners to check if we are on a platform
        RaycastHit2D l = Physics2D.Raycast(rb.position + leftGroundCheckOrigin, Vector2.down, groundCheckDistance);
        RaycastHit2D r = Physics2D.Raycast(rb.position + rightGroundCheckOrigin, Vector2.down, groundCheckDistance);
        if (l.collider != null)
        {
            if (l.collider.CompareTag("Ground")) return true;
        }
        if (r.collider != null)
            if (r.collider.CompareTag("Ground")) return true;
        return false;
    }

    Vector2 TargetPosition()
    {
        // Get the base position of the player
        Vector2 t = rb.position;

        // Apply horizontal movement from walking
        float w = maxWalkSpeed * walkCurve.Evaluate(Input.GetAxis("Horizontal")) * Time.deltaTime;
        t += Vector2.right * w;

        if(jumpState == JumpState.JUMPING)
        {
            t += Vector2.up * jumpForce * Time.deltaTime;
        }

        // Apply the gravity force
        if (jumpState == JumpState.FALLING) 
            t += Vector2.down * gravityForce;
        
        // Return the target position
        return t;
    }
}
