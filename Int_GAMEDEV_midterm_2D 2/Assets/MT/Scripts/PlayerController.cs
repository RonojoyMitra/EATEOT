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

    public enum JumpStatus { CAN_JUMP, JUMP_FLAG, HOLDING, FALLING };
    [Header("Jumping")]
    [SerializeField]
    JumpStatus jumpStatus;
    [SerializeField]
    float jumpInitialForce;
    [SerializeField]
    float jumpHoldForce;
    [SerializeField]
    float coyoteTime;
    bool jumpBuffer;    // Jump buffering allows the player to hit the jump button just before they hit the ground and then jump right as they land
    [SerializeField]
    float jumpBufferTime;
    float jumpBufferTimer;

    [Header("Ground Checking")]
    [SerializeField]
    Vector2 leftGCOrigin;
    [SerializeField]
    Vector2 rightGCOrigin;
    [SerializeField]
    float gcDistance;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Walking
        transform.Translate(Vector3.right * maxWalkSpeed * walkCurve.Evaluate(Input.GetAxis("Horizontal")) * Time.deltaTime);

        // Makes sure the jump buffer timer is ticking
        if(jumpBuffer)
        {
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer <= 0f)
            {
                jumpBuffer = false;
            }
        }

        // Jump status switching
        switch(jumpStatus)
        {
            // If you can jump and you press a jump key set the jump flag so physics can handle the jump
            case JumpStatus.CAN_JUMP:
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || jumpBuffer)
                {
                    jumpStatus = JumpStatus.JUMP_FLAG;
                    jumpBuffer = false;
                }
                // If the player walks off the edge while they can jump we start coyote time
                else if(GroundCheck() == false)
                {
                    StartCoroutine(StartCoyoteTime());
                }
                break;
            // If you're in the state of holding down the jump and you release both the jump keys you lose all additional jump forces
            // You also start falling if your velocity is negative
            case JumpStatus.HOLDING:
                if((!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.UpArrow)) || rb.velocity.y <= 0)
                {
                    jumpStatus = JumpStatus.FALLING;
                }
                break;
            case JumpStatus.FALLING:
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))   // Activate the jump buffer if the player hits jump while falling
                {
                    jumpBuffer = true;
                    jumpBufferTimer = jumpBufferTime;
                }
                if (GroundCheck()) jumpStatus = JumpStatus.CAN_JUMP;    // if the player hits the ground the return to the Can Jump state
                break;
        }
    }

    void FixedUpdate()
    {
        // If the jump flag is set we set the jump state to holding and apply an impulse
        if (jumpStatus == JumpStatus.JUMP_FLAG)
        {
            // Zero the velocity because the player can jump buffer and they may still technically have velocity from falling
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            jumpStatus = JumpStatus.HOLDING;    // set the new status to holding
            rb.AddForce(Vector2.up * jumpInitialForce, ForceMode2D.Impulse);    // apply the initial force
        }
        else if(jumpStatus == JumpStatus.HOLDING)   // While the player is holding we just apply a constant force so they get a little more height
        {
            rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
        }
    }

    // Allows the player to jump for a brief moment after the walk off a platform
    IEnumerator StartCoyoteTime()
    {
        // Wait for a number of seconds
        // This is the period which they should still be able to jump
        yield return new WaitForSeconds(coyoteTime);
        // Once the coyote time is over
        // If the player is still able to jump and they are still not on the ground the are falling
        if (GroundCheck() == false && jumpStatus == JumpStatus.CAN_JUMP)
        {
            jumpStatus = JumpStatus.FALLING;
        }
    }

    // Detect if the player is standing on the ground.
    // Sends raycasts from the lower left and right of the player to check for anything tagged "Ground".
    bool GroundCheck()
    {
        RaycastHit2D l = Physics2D.Raycast(rb.position + leftGCOrigin, Vector2.down, gcDistance);
        RaycastHit2D r = Physics2D.Raycast(rb.position + rightGCOrigin, Vector2.down, gcDistance);
        if (l.collider != null)
        {
            if (l.collider.CompareTag("Ground"))
                return true;
        }
        if (r.collider != null)
        {
            if (r.collider.CompareTag("Ground"))
                return true;
        }
        return false;
    }
}
