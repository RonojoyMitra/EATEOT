using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;    // Singleton identifier so other scripts can get a reference to the player

    Rigidbody2D rb; // the rigidbody attatched to the player

    [Header("Horizontal Movement")]
    [SerializeField]
    AnimationCurve walkCurve;   // The curve that controls how the walking winds up
    [SerializeField]
    float maxWalkSpeed; // The maximum speed the player can walk
    [SerializeField]
    [Range(0f,1f)]
    float grabbingWalkSpeedMulti;   // When holding a box the player walks at a percent of normal speed

    public enum JumpStatus { CAN_JUMP, JUMP_FLAG, HOLDING, FALLING };
    [Header("Jumping")]
    [SerializeField]
    JumpStatus jumpStatus;  // The current status of if the player is jumping
    [SerializeField]
    float jumpInitialForce; // Initial force applied when the player presses the jump key
    [SerializeField]
    float jumpHoldForce;    // The ongoing force applied when the player continues to hold the jump key
    [SerializeField]
    float coyoteTime;   // The amount of time after the player walks off a platform that they can still jump
    bool jumpBuffer;    // Jump buffering allows the player to hit the jump button just before they hit the ground and then jump right as they land
    [SerializeField]
    float jumpBufferTime;   // The time before landing that the player is able to jump
    float jumpBufferTimer;  // Timer for the jump buffer

    bool grounded = false;
    [Header("Ground Checking")]
    [SerializeField]
    Vector2 leftGCOrigin;
    [SerializeField]
    Vector2 rightGCOrigin;
    [SerializeField]
    float gcDistance;

    public enum Direction { LEFT, RIGHT };
    Direction facing = Direction.RIGHT; // The direction the player is facing
    bool grabbing = false;  // Is the player grabbing
    bool grabFlag = false;  // Flag used to mark the player is trying to grab/release something
    [Header("Push and Pull")]
    [SerializeField]
    Transform grabbedBox;   // The box that is currently grabbed
    [SerializeField]
    float wallCheckDistance;    // The distance the player looks ahead of them to check for walls
    [SerializeField]
    float wallCheckOffset;  // The offset from the center of the player to check for walls
    [SerializeField]
    float extraGrabbingWallCheckOffset; // The extra offset used when holding a box

    private void Start()
    {
        instance = this;    // Set the singleton
        rb = GetComponent<Rigidbody2D>();   // Get reference to the rigidbody
    }

    void Update()
    {
        // Walking
        if ((Input.GetAxis("Horizontal") > 0f && !WallCheck(Direction.RIGHT)) || (Input.GetAxis("Horizontal") < 0f && !WallCheck(Direction.LEFT)))
        {
            // gm is the grabbing multiplier
            // If the player is not grabbing they walk at full speed, and if they are they walk at a multiplied speed
            float gm = grabbing ? grabbingWalkSpeedMulti : 1f;
            transform.Translate(Vector3.right * maxWalkSpeed * walkCurve.Evaluate(Input.GetAxis("Horizontal")) * Time.deltaTime * gm);
        }

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
                    if(!grabbing)
                    {
                        jumpStatus = JumpStatus.JUMP_FLAG;
                        jumpBuffer = false;
                    }
                }
                // If the player walks off the edge while they can jump we start coyote time
                else if(grounded == false && !grabbing)
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
            // If the player is falling and they press a jump key they start the jump buffer
            // If they have become grounded they change to the CAN_JUMP state
            case JumpStatus.FALLING:
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))   // Activate the jump buffer if the player hits jump while falling
                {
                    jumpBuffer = true;
                    jumpBufferTimer = jumpBufferTime;
                }
                if (grounded) jumpStatus = JumpStatus.CAN_JUMP;    // if the player hits the ground the return to the Can Jump state
                break;
        }

        // Change the direction the player is facing while they are not grabbing on to a box
        // The player does not change directions while grabbing a box
        if(!grabbing)
        {
            if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && !(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
                facing = Direction.RIGHT;
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && !(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
                facing = Direction.LEFT;
        }

        // Grab input
        if(Input.GetKeyDown(KeyCode.Space) && !grabFlag)
        {
            grabFlag = true;
        }
    }

    void FixedUpdate()
    {
        // Update whether the player is on the ground
        grounded = GroundCheck();

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

        // If the player is trying to grab
        if(grabFlag)
        {
            grabFlag = false;   // turn off the grab flag
            if (!grabbing)
            {
                TryGrab();  // Grab if we're not
            }
            else
            {
                TryRelease();   // Release if we are
            }
        }

        // Box falls if not supported
        if (grabbing)
        {
            if (grabbedBox.GetComponent<BoxController>().IsGrounded == false)
            {
                TryRelease();
            }
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

    // Checks if there is a wall in front of the player
    // This is useful to stop the player from jittering while up against a wall
    // It also takes into acount the width of the box while grabbing one
    bool WallCheck(Direction direction)
    {
        Vector2 o = transform.position; // The origin for the raycast
        // The origin is adjusted based on the width of the player and the width of the box if the player is holding one
        if (!grabbing)
        {
            o += direction == Direction.LEFT ? Vector2.left * wallCheckOffset : Vector2.right * wallCheckOffset;
        }
        else
        {
            if (direction == Direction.LEFT && facing == Direction.LEFT)
                o += Vector2.left * (wallCheckOffset + grabbedBox.GetComponent<BoxCollider2D>().size.x * grabbedBox.lossyScale.x + extraGrabbingWallCheckOffset);
            else if (direction == Direction.LEFT && facing == Direction.RIGHT)
                o += Vector2.left * wallCheckOffset;
            else if (direction == Direction.RIGHT && facing == Direction.RIGHT)
                o += Vector2.right * (wallCheckOffset + grabbedBox.GetComponent<BoxCollider2D>().size.x * grabbedBox.lossyScale.x + extraGrabbingWallCheckOffset);
            else if (direction == Direction.RIGHT && facing == Direction.LEFT)
                o += Vector2.right * wallCheckOffset;
        }
        Vector2 d = direction == Direction.LEFT ? Vector2.left : Vector2.right;
        RaycastHit2D h = Physics2D.Raycast(o, d, wallCheckDistance);
        if (h.collider == null) return false;
        return true;
    }

    // Attempt to grab a box in front of the player
    void TryGrab()
    {
        // Raycast in front of the player
        Vector2 o = transform.position;
        o += facing == Direction.LEFT ? Vector2.left * wallCheckOffset : Vector2.right * wallCheckOffset;
        Vector2 d = facing == Direction.LEFT ? Vector2.left : Vector2.right;
        RaycastHit2D h = Physics2D.Raycast(o, d, wallCheckDistance);
        // If there is no collision detected we just return
        if (h.collider == null)
        {
            return;
        }
        // If the collider is a box we grab it
        if(h.collider.CompareTag("Box"))
        {
            grabbing = true;
            grabbedBox = h.collider.transform;
            grabbedBox.parent = transform;
        }
    }

    // Attempt to release the current grabbed box
    void TryRelease()
    {
        grabbedBox.parent = null;   // Unparent the box
        grabbing = false;   // Mark that we are not grabbing
        grabbedBox = null;  // Release the reference to the grabbed box
    }
}
