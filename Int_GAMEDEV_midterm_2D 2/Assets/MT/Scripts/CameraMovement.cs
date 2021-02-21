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
    float maxDeltaY;
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("The time value used while lerping toward the target. A higher value will have the camera move toward the target faster.")]
    float lerpPercent;

    // This is called in LateUpdate to avoid any jittering from the camera's movement happening at an arbitrary time relative to other objects' movement
    void LateUpdate()
    {
        Vector3 t = transform.position;
        if (target.position.x > transform.position.x + maxDeltaX)
            t.x = target.position.x - maxDeltaX;
        if (target.position.x < transform.position.x - maxDeltaX)
            t.x = target.position.x + maxDeltaX;
        if (target.position.y > transform.position.y + maxDeltaY)
            t.y = target.position.y - maxDeltaY;
        if (target.position.y < transform.position.y - maxDeltaY)
            t.y = target.position.y + maxDeltaY;

        transform.position = Vector3.Lerp(transform.position, t, lerpPercent);
    }
}
