using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPlatform : MonoBehaviour
{
    [SerializeField]
    float maxRotation;

    void LateUpdate()
    {
        Vector3 e = transform.eulerAngles;
        e.z = ClampAngle(e.z, -maxRotation, maxRotation);
        transform.eulerAngles = e;
    }

    float ClampAngle(float angle, float min, float max)
    {
        while (min < -180f) min += 360f;
        while (max < -180f) max += 360f;
        while (angle < -180f) angle += 360f;
        while (min > 180f) min -= 360f;
        while (max > 180f) max -= 360f;
        while (angle > 180f) angle -= 360f;

        if (angle > max) return max;
        if (angle < min) return min;
        return angle;
    }
}
