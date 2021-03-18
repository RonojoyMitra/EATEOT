using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class PhysicsSoundShifter : MonoBehaviour
{
    [SerializeField] private bool stoodOn;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (!stoodOn)
        {
            RuntimeManager.StudioSystem.setParameterByName("Physics Weirdness", GetComponent<StudioGlobalParameterTrigger>().value - 0.01f);
            print(GetComponent<StudioGlobalParameterTrigger>().value);
        }
        */
    }
    
    void OnCollisionStay2D(Collision2D other)
    {
        if (!stoodOn)
        {
            stoodOn = true;
        }
        GetComponent<StudioGlobalParameterTrigger>().value += 0.01f;
        //RuntimeManager.StudioSystem.setParameterByName("Physics Weirdness", GetComponent<StudioGlobalParameterTrigger>().value + 0.01f);
        print(GetComponent<StudioGlobalParameterTrigger>().value);
    }
    /*
    void OnCollisionEnter2D(Collision2D other)
    {
        stoodOn = true;
        GetComponent<StudioGlobalParameterTrigger>().value += 0.01f;
        //RuntimeManager.StudioSystem.setParameterByName("Physics Weirdness", 1f);
        print(GetComponent<StudioGlobalParameterTrigger>().value);
    }

    void OnCollisionExit2D(Collision2D other) //when the player is no longer touching the
    {
        stoodOn = false;
        //GetComponent<StudioGlobalParameterTrigger>().value = 0f;
        //RuntimeManager.StudioSystem.setParameterByName("Physics Weirdness", 0f);
        print(GetComponent<StudioGlobalParameterTrigger>().value);
    }
    */
}
