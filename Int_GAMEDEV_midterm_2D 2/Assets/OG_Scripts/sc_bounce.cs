using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class sc_bounce : MonoBehaviour
{
    public Rigidbody2D thisRigid2D;
    public float launchForce;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("bounce")){
            thisRigid2D.velocity = Vector2.up * launchForce;
        }
    }
}
