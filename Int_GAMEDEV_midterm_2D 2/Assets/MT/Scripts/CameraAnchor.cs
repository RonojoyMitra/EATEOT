using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnchor : MonoBehaviour
{
    [SerializeField]
    public float distance = 10f;

    void Update()
    {
        if(Vector3.Distance(PlayerController.instance.transform.position, transform.position) < distance)
        {
            Camera.main.GetComponent<CameraMovement>().Anchor(transform);
        }
    }
}
