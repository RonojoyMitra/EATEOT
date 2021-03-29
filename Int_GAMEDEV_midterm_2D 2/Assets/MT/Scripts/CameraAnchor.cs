using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnchor : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Camera.main.GetComponent<CameraMovement>().Anchor(transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Camera.main.GetComponent<CameraMovement>().Deanchor(transform);
        }
    }
}
