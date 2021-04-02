using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEventTrigger : MonoBehaviour
{
    private AudioManager myManager;

    public float myTargetRed, myTargetGreen, myTargetBlue;
    // Start is called before the first frame update
    void Start()
    {
        myManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (myManager.targetRed < myTargetRed)
            {
                myManager.targetRed = myTargetRed;
            }
            if (myManager.targetGreen < myTargetGreen)
            {
                myManager.targetGreen = myTargetGreen;
            }
            if (myManager.targetBlue < myTargetBlue)
            {
                myManager.targetBlue = myTargetBlue;
            }
            Destroy(gameObject);
        }
    }
}
