using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The transform that the camera is trying to focus on. This can be changed so the camera can pan or move to another object for effect before moving back to the player.")]
    Transform target;
    [SerializeField]
    [Tooltip("The max distance allowed between the target and the camera is the X direction.")]
    float maxDeltaX;
    [SerializeField]
    [Tooltip("The max distance allowed between the target and the camera is the Y direction.")]
    float maxDeltaYPos;
    [SerializeField]
    [Tooltip("The max distance allowed between the target and the camera is the Y direction.")]
    float maxDeltaYNeg;
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("The time value used while lerping toward the target. A higher value will have the camera move toward the target faster.")]
    float lerpPercent;

    Transform currentAnchor = null;

    float anchorTimer = 0f;

    // This is called in LateUpdate to avoid any jittering from the camera's movement happening at an arbitrary time relative to other objects' movement
    void LateUpdate()
    {
        Transform mem = target;

        if(currentAnchor != null)
        {
            target = currentAnchor;
        }

        Vector3 t = transform.position;

        if (target.position.x > transform.position.x + maxDeltaX)
            t.x = target.position.x - maxDeltaX;
        if (target.position.x < transform.position.x - maxDeltaX)
            t.x = target.position.x + maxDeltaX;
        if (target.position.y > transform.position.y + maxDeltaYPos)
            t.y = target.position.y - maxDeltaYPos;
        if (target.position.y < transform.position.y - maxDeltaYNeg)
            t.y = target.position.y + maxDeltaYNeg;

        transform.position = Vector3.Lerp(transform.position, t, lerpPercent);
        target = mem;

        anchorTimer += Time.deltaTime;
        if(anchorTimer > 0.25f)
        {
            currentAnchor = null;
        }
    }

    public void Anchor(Transform anchor)
    {
        currentAnchor = anchor;
        anchorTimer = 0f;
    }
}
