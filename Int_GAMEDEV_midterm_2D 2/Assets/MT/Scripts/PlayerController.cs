using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Debugging options
    public enum DebugMode { OFF, DRAW_RAYS, DRAW_RAYS_WITH_DISTANCE };
    [SerializeField]
    [Tooltip("The type of debugging you are attempting to do.")]
    DebugMode debugMode = DebugMode.OFF;

    // Debugging colors
    Color groundCheckColor = new Color(165f/255f, 42f / 255f, 42f / 255f);
    Color wallCheckColor = Color.magenta;
    Color grabColor = Color.yellow;

    public static PlayerController instance;    // Singleton identifier so other scripts can get a reference to the player

    Rigidbody2D rb; // the rigidbody attatched to the player

    [Header("Horizontal Movement")]
    [SerializeField]
    [Tooltip("This curve defines how the player's walking ramps up as they increase their horionztal input. Time of -1 means full left input, time of 1 means full right input. Value -1 means full left walking, value of 1 means full right walking.")]
    AnimationCurve walkCurve;
    [SerializeField]
    [Tooltip("The speed the player will walk when they are holding their horizontal input at maximum.")]
    float maxWalkSpeed;
    [SerializeField]
    [Range(0f,1f)]
    [Tooltip("The value the player's walking speed is multiplied by while holding a box.")]
    float grabbingWalkSpeedMulti;

    public enum JumpStatus { CAN_JUMP, JUMP_FLAG, HOLDING, FALLING };
    [Header("Jumping")]
    [SerializeField]
    [Tooltip("The current status of the player's jumping.")]
    JumpStatus jumpStatus;
    [SerializeField]
    [Tooltip("The initial impulse force applied to the player when they press the jump key.")]
    float jumpInitialForce;
    [SerializeField]
    [Tooltip("The ongoing force applied to the player when they hold the jump key.")]
    float jumpHoldForce;
    [SerializeField]
    [Tooltip("The time in seconds that the player is able to jump after walking off a platform.")]
    float coyoteTime;
    bool jumpBuffer;    // Jump buffering allows the player to hit the jump button just before they hit the ground and then jump right as they land
    [SerializeField]
    [Tooltip("The time in seconds before landing that the player is able to input a jump command and have it still work when they land.")]
    float jumpBufferTime;
    float jumpBufferTimer;  // Timer for the jump buffer

    bool grounded = false;
    [Header("Ground Checking")]
    [SerializeField]
    [Tooltip("The offset from the player that is used as the origin for the left ground checking raycast.")]
    Vector2 leftGCOrigin;
    [SerializeField]
    [Tooltip("The offset from the player that is used as the origin for the right ground checking raycast.")]
    Vector2 rightGCOrigin;
    [SerializeField]
    [Tooltip("The length of the raycasts used when checking for ground.")]
    float gcDistance;

    public enum Direction { LEFT, RIGHT };
    Direction facing = Direction.RIGHT; // The direction the player is facing
    bool grabbing = false;  // Is the player grabbing
    bool grabFlag = false;  // Flag used to mark the player is trying to grab/release something
    bool autograbFlag = false; // This is a flag used for autograbbing. It is only grabs and doesn't release
    Transform grabbedBox;   // The box that is currently grabbed
    [Header("Push and Pull")]
    [SerializeField]
    [Tooltip("The distance ahead of the player to check if there is a wall.")]
    float wallCheckDistance;
    [SerializeField]
    [Tooltip("The offset from the player to the origin of the raycast used to check for walls.")]
    float wallCheckOffset;
    [SerializeField]
    [Tooltip("The extra offset allotted to wall checking while you are holding a box. This is used because the box is not always directly next to the player.")]
    float extraGrabbingWallCheckOffset;

    [Header("Auto Grab")]
    [SerializeField]
    [Tooltip("The time in seconds before the player attempts to autograb a box it is walking into.")]
    float timeToAutograb;
    float autograbTimer = 0f;

    [Header("Spring")]
    [SerializeField]
    float springForce;

    [Header("Animation")]
    [SerializeField]
    [Range(0f, 1f)]
    float percentageMaxWalkSpeedToWalkAnimation;

    // These are the tags that when applied to an object allows the player to jump off of them
    public static string[] groundTags = { "Ground", "Box", "Physics platform", "VanishingBlock", "MovingPlatform" };

    // The animator for the player
    Animator animator;

    [SerializeField] private FMOD.Studio.EventInstance walkInstance;
    [SerializeField] private FMOD.Studio.EventInstance dragInstance;

    private void Start()
    {
        instance = this;    // Set the singleton
        rb = GetComponent<Rigidbody2D>();   // Get reference to the rigidbody
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Walk();
        Autograb();
        UpdateJumpBuffer();
        UpdateJumpStatus();
        UpdateFacing();
        CheckGrabInput();
        UpdateAnimationDirection();

        UpdateAnimationPushing();
    }

    void FixedUpdate()
    {
        UpdateGrounded();
        JumpPhysics();
        Grab();
        CheckBoxFalling();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Spring")) Spring();
    }

    #region Player Inputs
    /// <summary>
    /// Controls the players basic horizontal movement.
    /// </summary>
    void Walk()
    {
        // if the player is moving right, there is a wall on their right, and they are either jumping or falling
        if(Input.GetAxis("Horizontal") > 0f && WallCheck(Direction.RIGHT) && (jumpStatus == JumpStatus.HOLDING || jumpStatus == JumpStatus.FALLING))
        {
            // Save the object that is to the right of the player
            GameObject gm = GetWallCheckObject(Direction.RIGHT);
            // If it is a box
            if(gm != null && gm.CompareTag("Box"))
            {
                // move it
                gm.transform.Translate(Vector3.right * maxWalkSpeed * walkCurve.Evaluate(Input.GetAxis("Horizontal")) * Time.deltaTime);
            }
        }
        // same but for the left
        else if(Input.GetAxis("Horizontal") < 0f && WallCheck(Direction.LEFT) && (jumpStatus == JumpStatus.HOLDING || jumpStatus == JumpStatus.FALLING))
        {
            GameObject gm = GetWallCheckObject(Direction.LEFT);
            if(gm != null && gm.CompareTag("Box"))
            {
                // Still translated right because the walkcurve will evaluate as negative
                gm.transform.Translate(Vector3.right * maxWalkSpeed * walkCurve.Evaluate(Input.GetAxis("Horizontal")) * Time.deltaTime);
            }
        }

        // If the player is inputing a horizontal direction and there is not a wall in that direction
        if ((Input.GetAxis("Horizontal") > 0f && !WallCheck(Direction.RIGHT)) || (Input.GetAxis("Horizontal") < 0f && !WallCheck(Direction.LEFT)))
        {
            /* gm is the grab multiplier.
             * This is an adjustment made to walking speed generally to slow the player while they are grabbing something to add a sense of force.
             * Then we simply Translate the player's position.
             * We attempt to avoid any strange physics jittering by checking for walls before reaching this step instead of using a physics movement.
             */
            float gm = grabbing ? grabbingWalkSpeedMulti : 1f;
            transform.Translate(Vector3.right * maxWalkSpeed * walkCurve.Evaluate(Input.GetAxis("Horizontal")) * Time.deltaTime * gm);
            if (Mathf.Abs(walkCurve.Evaluate(Input.GetAxis("Horizontal"))) >
                percentageMaxWalkSpeedToWalkAnimation * maxWalkSpeed)
            {
                walkInstance.start();
                animator.SetBool("Walking", true);
            }
            else
            {
                walkInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                animator.SetBool("Walking", false);
            }
        }
        else
        {
            walkInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            animator.SetBool("Walking", false);
        }
    }

    /// <summary>
    /// Checks if the player has pressed the grab key.
    /// </summary>
    void CheckGrabInput()
    {
        // Sets the grab flag to true if the player pressed the grab flag and it isn't already set to true.
        if (Input.GetKeyDown(KeyCode.Space) && !grabFlag)
        {
            grabFlag = true;
        }
    }

    /// <summary>
    /// Controls the player's jump status.
    /// This keeps track of if the player is currenlty able to jump, has recently pressed the jump key, is currently jumping while holding the jump key, or is falling.
    /// </summary>
    void UpdateJumpStatus()
    {
        switch (jumpStatus)
        {
            // This case runs if the player is currently able to jump
            case JumpStatus.CAN_JUMP:
                // This checks if the player has pressed either jump key or is currenlty jump buffering
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || jumpBuffer)
                {
                    // This checks if the player is grabbing as they can not jump while holding a box
                    if (!grabbing)
                    {
                        // If everything checks out we can change the jump status to jump flag so that physics can handle the jump during the next FixedUpdate
                        jumpStatus = JumpStatus.JUMP_FLAG;
                        // We turn jumpBuffer off just in case it was used
                        jumpBuffer = false;
                    }
                }
                // If the player walks off the edge while they can jump we start coyote time
                else if (grounded == false && !grabbing)
                {
                    StartCoroutine(StartCoyoteTime());
                }
                break;
            // This case runs if the player is currently jumping and holding down a jump key
            case JumpStatus.HOLDING:
                // This checks if the player has released both jump keys or their velocity is negative meaning they are moving downard
                if ((!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.UpArrow)) || rb.velocity.y <= 0)
                {
                    // In this case we change the player to be marked as falling
                    jumpStatus = JumpStatus.FALLING;
                }
                break;
            // This case runs if the player is falling
            case JumpStatus.FALLING:
                // If the player presses a jump key while falling we start a jump buffer so they jump as soon as they land
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    jumpBuffer = true;
                    // We always set the timer because repressing the jump key should reset the timer
                    jumpBufferTimer = jumpBufferTime;
                }
                // If the player becomes grounded their status resets to can jump
                if (grounded) jumpStatus = JumpStatus.CAN_JUMP;
                FMODUnity.RuntimeManager.PlayOneShot("Landing", transform.position); //lands
                break;
        }
    }

    /// <summary>
    /// Updates the direction the player is facing.
    /// </summary>
    void UpdateFacing()
    {
        // The players direction is not changed if they are grabbing because the must continue to face the box.
        if (!grabbing)
        {
            // If the player is pressing either right movement key and neither left movement keys they are facing right
            if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) &&
                !(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
                facing = Direction.RIGHT;
            // If the player is pressing either left movement key and neither right movement keys they are facing left
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) &&
                !(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
                facing = Direction.LEFT;
        }
    }

    /// <summary>
    /// This method controls the autograbbing feature.
    /// This feature allows the player to grab onto a box by walking into it for a long enough period of time.
    /// </summary>
    void Autograb()
    {
        // Assume that if the time is negative we should not autograb
        if (timeToAutograb < 0f) return;

        // if the player is trying to move and there is a wall in the way
        if ((Input.GetAxis("Horizontal") > 0f && WallCheck(Direction.RIGHT)) ||
            (Input.GetAxis("Horizontal") < 0f && WallCheck(Direction.LEFT)))
        {
            autograbTimer += Time.deltaTime;    // Keep track of how long they have been trying to walk
        }
        else
        {
            autograbTimer = 0f; // Otherwise reset the timer
        }

        // If the timer is up
        if (autograbTimer >= timeToAutograb)
        {
            grabFlag = true;    // mark that we should try to grab at the next possible point
            autograbTimer = 0f; // Reset the autograb timer
        }
    }
    #endregion

    #region Physics Checks
    /// <summary>
    /// Checks if the box the player is holding should fall.
    /// </summary>
    void CheckBoxFalling()
    {
        // We only need to check if the player is holding a box
        if (grabbing)
        {
            // If the box's controller says it is not grounded we release the box
            if (grabbedBox.GetComponent<BoxController>().IsGrounded == false)
            {
                TryRelease();
                return;
            }
            // If the player's velocity and the box's vertical velocities are too different we release the box
            if (grabbedBox.GetComponent<Rigidbody2D>().velocity.y - rb.velocity.y > 0.1f || grabbedBox.GetComponent<Rigidbody2D>().velocity.y - rb.velocity.y < -0.1f)
            {
                TryRelease();
                return;
            }
        }
    }

    /// <summary>
    /// Checks if the player is standing on an object that is tagged with any valid ground tag.
    /// </summary>
    /// <returns>True if the player is on ground and false otherwise.</returns>
    bool GroundCheck()
    {
        /* We raycast from both the bottom left and bottom right of the player
         * This allows better accuracy as the player should be grounded as long as any part of their body is on the ground
         * This does mean that the player can not stand properly on a platform that is thinner than them.
         */
        RaycastHit2D l = Physics2D.Raycast(rb.position + leftGCOrigin, Vector2.down, gcDistance);
        RaycastHit2D r = Physics2D.Raycast(rb.position + rightGCOrigin, Vector2.down, gcDistance);

        // If debugging is on we should draw the ground checking rays
        switch (debugMode)
        {
            case DebugMode.DRAW_RAYS:
                Debug.DrawRay(rb.position + leftGCOrigin, Vector3.down, groundCheckColor);
                Debug.DrawRay(rb.position + rightGCOrigin, Vector3.down, groundCheckColor);
                break;
            case DebugMode.DRAW_RAYS_WITH_DISTANCE:
                Debug.DrawRay(rb.position + leftGCOrigin, Vector3.down * gcDistance, groundCheckColor);
                Debug.DrawRay(rb.position + rightGCOrigin, Vector3.down * gcDistance, groundCheckColor);
                break;
        }

        /* We have to check if the collider exists first or we risk a NullReferenceException if we don't hit anything with our raycast.
         * As long as there is a collider, we run a GroundTagCheck call to see if it has any of the tags that are considered ground.
         */
        if (l.collider != null)
        {
            if (GroundTagCheck(l.collider.tag))
            {
                if(l.collider.CompareTag("VanishingBlock"))
                {
                    l.collider.GetComponent<VanishingBlock>().Vanish();
                }
                if (r.collider != null)
                    if (r.collider.CompareTag("VanishingBlock"))
                        r.collider.GetComponent<VanishingBlock>().Vanish();
                return true;
            }
        }
        if (r.collider != null)
        {
            if (GroundTagCheck(r.collider.tag))
            {
                if (r.collider.CompareTag("VanishingBlock"))
                {
                    r.collider.GetComponent<VanishingBlock>().Vanish();
                }
                if (l.collider != null)
                    if (l.collider.CompareTag("VanishingBlock"))
                        l.collider.GetComponent<VanishingBlock>().Vanish();
                return true;
            }
        }
        // If neither raycast finds a valid ground collider we return false.
        return false;
    }

    /// <summary>
    /// Checks if there is a wall in front of the player.
    /// </summary>
    /// <param name="direction">The direction in front of the player to check.</param>
    /// <returns>True if there is a collider in the given direction and false otherwise.</returns>
    bool WallCheck(Direction direction)
    {
        // The origin for the raycast
        Vector2 o = transform.position;

        // The origin is adjusted based on the width of the player and the width of the box if the player is holding one
        if (!grabbing)
        {
            // If the player is not grabbing we just add a vector equal to the wallCheckOffset in the direction we are checking
            o += direction == Direction.LEFT ? Vector2.left * wallCheckOffset : Vector2.right * wallCheckOffset;
        }
        else
        {
            /* If the player is grabbing we have to check if we are grabbing in the direction of the box.
             * If the direction we are checking matches the direction we are facing then we know we are checking in the direction of the box.
             * If we are checking in the direction of the box we add a distance equal to
             * the base wallCheckOffset + the width of the box (after accounting for the scale of the box) + the extraGrabbgingWallCheckOffset.
             * Otherwise we just add a distance equal to the normal wallCheckOffset.
             */
            if (direction == Direction.LEFT && facing == Direction.LEFT)
                o += Vector2.left * (wallCheckOffset + grabbedBox.GetComponent<BoxCollider2D>().size.x * grabbedBox.lossyScale.x + extraGrabbingWallCheckOffset);
            else if (direction == Direction.LEFT && facing == Direction.RIGHT)
                o += Vector2.left * wallCheckOffset;
            else if (direction == Direction.RIGHT && facing == Direction.RIGHT)
                o += Vector2.right * (wallCheckOffset + grabbedBox.GetComponent<BoxCollider2D>().size.x * grabbedBox.lossyScale.x + extraGrabbingWallCheckOffset);
            else if (direction == Direction.RIGHT && facing == Direction.LEFT)
                o += Vector2.right * wallCheckOffset;
        }
        // This is just the vector representation of direction
        Vector2 d = direction == Direction.LEFT ? Vector2.left : Vector2.right;
        // We do the raycast
        RaycastHit2D h = Physics2D.Raycast(o, d, wallCheckDistance);

        // Debugging
        switch (debugMode)
        {
            case DebugMode.DRAW_RAYS:
                Debug.DrawRay(o, d, wallCheckColor);
                break;
            case DebugMode.DRAW_RAYS_WITH_DISTANCE:
                Debug.DrawRay(o, d * wallCheckDistance, wallCheckColor);
                break;
        }

        // If the raycast didn't hit anything we return false
        if (h.collider == null) return false;

        if (h.collider.isTrigger == true) return false;
        // If the object we hit is a box we can push through it
        if (grabbing && h.collider.CompareTag("Box"))
        {
            if (direction == Direction.RIGHT) return WallCheck(direction, o + new Vector2(grabbedBox.GetComponent<BoxCollider2D>().size.x, 0f));
            if (direction == Direction.LEFT) return WallCheck(direction, o - new Vector2(grabbedBox.GetComponent<BoxCollider2D>().size.x, 0f));
        }
        // If it did hit something we return true
        return true;
    }

    bool WallCheck(Direction direction, Vector2 origin)
    {
        // The origin for the raycast
        Vector2 o = origin;

        // This is just the vector representation of direction
        Vector2 d = direction == Direction.LEFT ? Vector2.left : Vector2.right;
        // We do the raycast
        RaycastHit2D h = Physics2D.Raycast(o, d, wallCheckDistance);

        // Debugging
        switch (debugMode)
        {
            case DebugMode.DRAW_RAYS:
                Debug.DrawRay(o, d, wallCheckColor);
                break;
            case DebugMode.DRAW_RAYS_WITH_DISTANCE:
                Debug.DrawRay(o, d * wallCheckDistance, wallCheckColor);
                break;
        }

        // If the raycast didn't hit anything we return false
        if (h.collider == null) return false;
        if (h.collider.isTrigger == true) return false;
        // If the object we hit is a box we can push through it
        if (grabbing && h.collider.CompareTag("Box"))
        {
            if (direction == Direction.RIGHT) return WallCheck(direction, o + new Vector2(grabbedBox.GetComponent<BoxCollider2D>().size.x, 0f));
            if (direction == Direction.LEFT) return WallCheck(direction, o - new Vector2(grabbedBox.GetComponent<BoxCollider2D>().size.x, 0f));
        }
        // If it did hit something we return true
        return true;
    }

    GameObject GetWallCheckObject(Direction direction)
    {
        // The origin for the raycast
        Vector2 o = transform.position;

        // The origin is adjusted based on the width of the player and the width of the box if the player is holding one
        if (!grabbing)
        {
            // If the player is not grabbing we just add a vector equal to the wallCheckOffset in the direction we are checking
            o += direction == Direction.LEFT ? Vector2.left * wallCheckOffset : Vector2.right * wallCheckOffset;
        }
        else
        {
            /* If the player is grabbing we have to check if we are grabbing in the direction of the box.
             * If the direction we are checking matches the direction we are facing then we know we are checking in the direction of the box.
             * If we are checking in the direction of the box we add a distance equal to
             * the base wallCheckOffset + the width of the box (after accounting for the scale of the box) + the extraGrabbgingWallCheckOffset.
             * Otherwise we just add a distance equal to the normal wallCheckOffset.
             */
            if (direction == Direction.LEFT && facing == Direction.LEFT)
                o += Vector2.left * (wallCheckOffset + grabbedBox.GetComponent<BoxCollider2D>().size.x * grabbedBox.lossyScale.x + extraGrabbingWallCheckOffset);
            else if (direction == Direction.LEFT && facing == Direction.RIGHT)
                o += Vector2.left * wallCheckOffset;
            else if (direction == Direction.RIGHT && facing == Direction.RIGHT)
                o += Vector2.right * (wallCheckOffset + grabbedBox.GetComponent<BoxCollider2D>().size.x * grabbedBox.lossyScale.x + extraGrabbingWallCheckOffset);
            else if (direction == Direction.RIGHT && facing == Direction.LEFT)
                o += Vector2.right * wallCheckOffset;
        }
        // This is just the vector representation of direction
        Vector2 d = direction == Direction.LEFT ? Vector2.left : Vector2.right;
        // We do the raycast
        RaycastHit2D h = Physics2D.Raycast(o, d, wallCheckDistance);

        // Debugging
        switch (debugMode)
        {
            case DebugMode.DRAW_RAYS:
                Debug.DrawRay(o, d, wallCheckColor);
                break;
            case DebugMode.DRAW_RAYS_WITH_DISTANCE:
                Debug.DrawRay(o, d * wallCheckDistance, wallCheckColor);
                break;
        }

        // If the raycast didn't hit anything we return false
        if (h.collider == null) return null;

        return h.collider.gameObject;
    }

    #endregion

    #region Actions
    void Spring()
    {
        FMODUnity.RuntimeManager.PlayOneShot("Spring", transform.position); //plays the noise of the spring going off at the current location.
        FMODUnity.RuntimeManager.PlayOneShotAttached("Spring Voiced", gameObject); //plays the noise of the character reacting to the spring, following the character
        rb.angularVelocity = 0f;
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * springForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// This method controls the logic of all grab actions. 
    /// This includes both normal player input grabs as well as other grabs like autograbs.
    /// </summary>
    void Grab()
    {
        // If the player has pressed the grab button
        if (grabFlag)
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
        // If the autograb flag has been triggered and we are not currently grabbing we attempt to grab
        if (autograbFlag && !grabbing)
        {
            TryGrab();
        }
    }

    /// <summary>
    /// Applies forces to the player's rigidbody to cause them to jump.
    /// This method also has minor control over the player's jump status but only to mark that the jump flag has been acted on.
    /// </summary>
    void JumpPhysics()
    {
        // If the jump flag is set we set the jump state to holding and apply an impulse
        if (jumpStatus == JumpStatus.JUMP_FLAG)
        {
            /* Zero the velocity because the player can jump buffer and they may still technically have velocity from falling
             * We also need to zero the angular velocity just to be safe
             */
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            // We set that the jump flag was read and move the player into the holding status
            jumpStatus = JumpStatus.HOLDING;
            // Apply an impulse force to give the player an initial boost to their jump
            rb.AddForce(Vector2.up * jumpInitialForce, ForceMode2D.Impulse);
            FMODUnity.RuntimeManager.PlayOneShotAttached("Jumping", gameObject);
        }
        // If the player is still holding the jump key we can slow their fall by applying a force to them
        else if (jumpStatus == JumpStatus.HOLDING)
        {
            rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
        }
    }

    /// <summary>
    /// Attempt to grab a box in front of the player
    /// </summary>
    void TryGrab()
    {
        // Raycast in front of the player
        Vector2 o = transform.position;
        o += facing == Direction.LEFT ? Vector2.left * wallCheckOffset : Vector2.right * wallCheckOffset;
        Vector2 d = facing == Direction.LEFT ? Vector2.left : Vector2.right;

        // Debugging
        switch (debugMode)
        {
            case DebugMode.DRAW_RAYS:
                Debug.DrawRay(o, d, grabColor);
                break;
            case DebugMode.DRAW_RAYS_WITH_DISTANCE:
                Debug.DrawRay(o, d * wallCheckDistance, grabColor);
                break;
        }

        RaycastHit2D h = Physics2D.Raycast(o, d, wallCheckDistance);
        // If there is no collision detected we just return
        if (h.collider == null)
        {
            return;
        }
        // If the collider is a box we grab it
        if (h.collider.CompareTag("Box"))
        {
            grabbing = true;                        // Mark that we are now grabbing
            grabbedBox = h.collider.transform;      // Store the box that we are grabbing
            grabbedBox.parent = transform;          // Set the grabbed box as our child so it moves with us
        }
    }

    /// <summary>
    /// Attempts to release the box we are currently grabbing.
    /// Should not be able to fail.
    /// </summary>
    void TryRelease()
    {
        grabbedBox.parent = null;   // Unparent the box
        grabbing = false;   // Mark that we are not grabbing
        grabbedBox = null;  // Release the reference to the grabbed box
    }
    #endregion

    #region Timers
    /// <summary>
    /// This method keeps the timer for the jump buffer.
    /// </summary>
    void UpdateJumpBuffer()
    {
        if (jumpBuffer)
        {
            // jumpBufferTimer starts out at a max value and then ticks down until it hits zero
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer <= 0f)
            {
                jumpBuffer = false;
            }
        }
    }

    /// <summary>
    /// This coroutine handles coyote time.
    /// When the player walks off of a platform while they can jump,
    /// instead of just beginning to fall,
    /// this coroutine is called to keep them in the CAN_JUMP state until they lose the ability to use coyote time.
    /// </summary>
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
    #endregion

    #region Animation
    void UpdateAnimationDirection()
    {
        animator.SetInteger("Facing", facing == Direction.RIGHT ? 1 : -1);
    }

    void UpdateAnimationPushing()
    {
        if(!grabbing)
        {
            animator.SetInteger("Pushing", 0);
            return;
        }

        // If the box is to the right and you are pushing
        if(Input.GetAxis("Horizontal") > 0f)
        {
            if (grabbedBox.position.x > transform.position.x)
                animator.SetInteger("Pushing", 1);
            else
                animator.SetInteger("Pushing", -2);
        }
        else if(Input.GetAxis("Horizontal") < 0f)
        {
            if (grabbedBox.position.x < transform.position.x)
                animator.SetInteger("Pushing", -1);
            else
                animator.SetInteger("Pushing", 2);
        }
    }
    #endregion

    #region Miscellaneous
    /// <summary>
    /// Updates the grounded boolean.
    /// This is done to reduce the amount of times that the GroundCheck function is called.
    /// Instead of calling it every time we need to know if the player is grounded,
    /// the FixedUpdate method will call this once per update and store the result for other methods to read.
    /// </summary>
    void UpdateGrounded()
    {
        // Update whether the player is on the ground
        grounded = GroundCheck();
    }

    /// <summary>
    /// Compares the tag of a GameObject to the list of valid "ground" tags.
    /// This is important because this is used to decide if the player is grounded and can jump,
    /// but not all objects the player should be able to jump off of have the same tag as some are ground elements and others are things like boxes.
    /// </summary>
    /// <param name="tag">The tag to be checked against the list</param>
    /// <returns>True if the tag matched a valid ground tag and false otherwise.</returns>
    bool GroundTagCheck(string tag)
    {
        // We just compare the tag to each element in the groundTags array and return true if we find a match
        for (int i = 0; i < groundTags.Length; i++)
        {
            if (tag == groundTags[i]) return true;
        }
        return false;
    }
    #endregion
}
