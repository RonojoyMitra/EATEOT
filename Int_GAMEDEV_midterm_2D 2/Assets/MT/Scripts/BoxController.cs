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
    [Tooltip("The offset from the box that is used as the origin for the left ground checking raycast.")]
    Vector2 leftGCOrigin;
    [SerializeField]
    [Tooltip("The offset from the box that is used as the origin for the right ground checking raycast.")]
    Vector2 rightGCOrigin;
    [SerializeField]
    [Tooltip("The distance used to check if the box is on the ground.")]
    float gcDistance;

    string[] groundTags;

    private void Start()
    {
        groundTags = PlayerController.groundTags;
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        grounded = GroundCheck();

        /*if (lPlatform != null)
            lPlatform.SendMessage("RegisterBox", gameObject);
        if (rPlatform != null && lPlatform != rPlatform)
            rPlatform.SendMessage("RegisterBox", gameObject);*/
    }

    #region Physics Checks
    /// <summary>
    /// Detects if the box is on an object tagged as ground or any other "ground" tag.
    /// </summary>
    /// <returns>True if the box is on a valid object, and false otherwise.</returns>
    bool GroundCheck()
    {
        if(debugging)
        {
            Debug.DrawRay(transform.position + (Vector3)leftGCOrigin, Vector2.down);
            Debug.DrawRay(transform.position + (Vector3)rightGCOrigin, Vector2.down);
        }

        if(transform.parent != null)
           if(transform.parent.CompareTag("Player") == false)
                transform.parent = null;

        RaycastHit2D l = Physics2D.Raycast(rb.position + leftGCOrigin, Vector2.down, gcDistance);
        RaycastHit2D r = Physics2D.Raycast(rb.position + rightGCOrigin, Vector2.down, gcDistance);
        if (l.collider != null)
        {
            if (GroundTagCheck(l.collider.tag))
            {
                if(l.collider.CompareTag("Box") && transform.parent == null)
                {
                    transform.parent = l.collider.transform;
                }
                return true;
            }
        }
        if (r.collider != null)
        {
            if (GroundTagCheck(r.collider.tag))
            {
                if(r.collider.CompareTag("Box") && transform.parent == null)
                {
                    transform.parent = r.collider.transform;
                }
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Miscellanious
    /// <summary>
    /// Compares the tag of a GameObject to the list of valid "ground" tags.
    /// This is a copy of the GroundTagCheck method found in the PlayerController.
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
