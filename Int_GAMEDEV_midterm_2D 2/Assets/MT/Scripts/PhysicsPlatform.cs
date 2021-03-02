using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPlatform : MonoBehaviour
{
    [SerializeField]
    DistanceJoint2D rotationLock;

    private void FixedUpdate()
    {
        rotationLock.connectedAnchor = new Vector2(transform.position.x - 1.5f, transform.position.y);
    }
}
