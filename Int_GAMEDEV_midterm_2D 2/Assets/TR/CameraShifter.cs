using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShifter : MonoBehaviour
{
	private CameraMovement myCamMove;

	public float defaultMaxDeltaX, defaultMaxDeltaYPos, defaultMaxDeltaYNeg;
    // Start is called before the first frame update
    void Start()
    {
	    myCamMove = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
	    if (other.CompareTag("Player"))
	    {
		    myCamMove.ChangeMaxDeltas(0f, 0f, 0f);
	    }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
	    if (other.CompareTag("Player"))
	    {
		    myCamMove.ChangeMaxDeltas(defaultMaxDeltaX, defaultMaxDeltaYPos, defaultMaxDeltaYNeg);
	    }
    }
}
