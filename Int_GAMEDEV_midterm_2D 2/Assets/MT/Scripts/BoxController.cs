using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]
    bool debugging = false;

    public bool IsGrounded { get { return grounded; } }
    bool grounded = false;
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

    private void FixedUpdate()
    {
        grounded = GroundCheck();
    }

    // Detect if the player is standing on the ground.
    // Sends raycasts from the lower left and right of the player to check for anything tagged "Ground".
    bool GroundCheck()
    {
        if(debugging)
        {
            Debug.DrawRay(transform.position + (Vector3)leftGCOrigin, Vector2.down);
            Debug.DrawRay(transform.position + (Vector3)rightGCOrigin, Vector2.down);
        }

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
