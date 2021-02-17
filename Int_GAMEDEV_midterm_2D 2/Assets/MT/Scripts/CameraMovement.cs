using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    float maxDeltaX, maxDeltaY;
    [SerializeField]
    [Range(0f, 1f)]
    float lerpPercent;

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
