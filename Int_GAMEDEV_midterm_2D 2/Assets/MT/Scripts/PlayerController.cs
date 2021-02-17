using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

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

    bool grounded = false;
    [Header("Ground Checking")]
    [SerializeField]
    Vector2 leftGCOrigin;
    [SerializeField]
    Vector2 rightGCOrigin;
    [SerializeField]
    float gcDistance;

    public enum Direction { LEFT, RIGHT };
    Direction facing = Direction.RIGHT;
    bool grabbing = false;
    bool grabFlag = false;
    [Header("Push and Pull")]
    [SerializeField]
    Transform grabbedBox;
    [SerializeField]
    float wallCheckDistance;
    [SerializeField]
    float wallCheckOffset;
    [SerializeField]
    float extraGrabbingWallCheckOffset;

    private void Start()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Walking
        if((Input.GetAxis("Horizontal") > float.Epsilon && !WallCheck(Direction.RIGHT)) || (Input.GetAxis("Horizontal") < Mathf.Epsilon && !WallCheck(Direction.LEFT)))
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
            case JumpStatus.FALLING:
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))   // Activate the jump buffer if the player hits jump while falling
                {
                    jumpBuffer = true;
                    jumpBufferTimer = jumpBufferTime;
                }
                if (grounded) jumpStatus = JumpStatus.CAN_JUMP;    // if the player hits the ground the return to the Can Jump state
                break;
        }

        if(!grabbing)
        {
            if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && !(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
                facing = Direction.RIGHT;
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && !(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
                facing = Direction.RIGHT;
        }

        // Grab input
        if(Input.GetKeyDown(KeyCode.Space) && !grabFlag)
        {
            grabFlag = true;
        }
    }

    void FixedUpdate()
    {
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

        if(grabFlag)
        {
            grabFlag = false;
            if (!grabbing)
            {
                TryGrab();
            }
            else
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

    bool WallCheck(Direction direction)
    {
        Vector2 o = transform.position;
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
        Debug.DrawRay(o, Vector3.up);
        Vector2 d = direction == Direction.LEFT ? Vector2.left : Vector2.right;
        RaycastHit2D h = Physics2D.Raycast(o, d, wallCheckDistance);
        if (h.collider == null) return false;
        return true;
    }

    void TryGrab()
    {
        Vector2 o = transform.position;
        o += facing == Direction.LEFT ? Vector2.left * wallCheckOffset : Vector2.right * wallCheckOffset;
        Vector2 d = facing == Direction.LEFT ? Vector2.left : Vector2.right;
        RaycastHit2D h = Physics2D.Raycast(o, d, wallCheckDistance);
        if (h.collider == null)
        {
            return;
        }
        if(h.collider.CompareTag("Box"))
        {
            grabbing = true;
            grabbedBox = h.collider.transform;
            grabbedBox.parent = transform;
        }
    }

    void TryRelease()
    {
        grabbedBox.parent = null;
        grabbing = false;
        grabbedBox = null;
    }
}
