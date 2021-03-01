using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    Vector3 closedPosition;
    [SerializeField]
    Vector3 openPosition;
    [SerializeField]
    float lerpValue;

    [SerializeField]
    bool locked = true;

    void Update()
    {
        if (locked)
            transform.position = Vector3.Lerp(transform.position, closedPosition, lerpValue);
        else
            transform.position = Vector3.Lerp(transform.position, openPosition, lerpValue);
    }

    public void Open()
    {
        locked = false;
    }
    public void Close()
    {
        locked = true;
    }
}
