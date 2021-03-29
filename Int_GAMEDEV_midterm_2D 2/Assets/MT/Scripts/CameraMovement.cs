using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector3 perfectPosition;
    
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

    private List<Transform> anchors = new List<Transform>();

    private void Start()
    {
        perfectPosition = transform.position;
    }

    // This is called in LateUpdate to avoid any jittering from the camera's movement happening at an arbitrary time relative to other objects' movement
    void LateUpdate()
    {
        Transform mem = target;
        float memDX = maxDeltaX;
        float memDYP = maxDeltaYPos;
        float memDYN = maxDeltaYNeg;

        if(anchors.Count > 0)
        {
            target = anchors[anchors.Count - 1];
            maxDeltaX = 0f;
            maxDeltaYNeg = 0f;
            maxDeltaYPos = 0f;
        }

        Vector3 t = perfectPosition;

        if (target.position.x > perfectPosition.x + maxDeltaX)
            t.x = target.position.x - maxDeltaX;
        if (target.position.x < perfectPosition.x - maxDeltaX)
            t.x = target.position.x + maxDeltaX;
        if (target.position.y > perfectPosition.y + maxDeltaYPos)
            t.y = target.position.y - maxDeltaYPos;
        if (target.position.y < perfectPosition.y - maxDeltaYNeg)
            t.y = target.position.y + maxDeltaYNeg;

        perfectPosition = Vector3.Lerp(perfectPosition, t, lerpPercent);
        target = mem;
        maxDeltaX = memDX;
        maxDeltaYNeg = memDYN;
        maxDeltaYPos = memDYP;
        
        Debug.Log(perfectPosition);
        
        Vector3 pixelPosition = new Vector3(perfectPosition.x - (float)(perfectPosition.x%0.03125), perfectPosition.y - (float)(perfectPosition.y%0.03125), -10);

        transform.position = pixelPosition;
    }

    public void Anchor(Transform anchor)
    {
        anchors.Add(anchor);
    }

    public void Deanchor(Transform anchor)
    {
        anchors.Remove(anchor);
    }
}
