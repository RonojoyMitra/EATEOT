using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public float targetRed, targertGreen, targetBlue, currentRed, currentGreen, currentBlue;
    void Awake()
    {
        //DontDestroyOnLoad(gameObject); //Setting the audiomanager object to dontdestroyonload so we can choose to have sounds transition smoothly across scenes
        //the above line has been commented out until I know how exactly we'll be shifting between scenes.
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentRed < targetRed)
        {
            currentRed += 0.01f;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Red Weirdness", currentRed);
        }
        if (currentGreen < targertGreen)
        {
            currentGreen += 0.01f;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Green Weirdness", currentGreen);
        }
        if (currentBlue < targetBlue)
        {
            currentBlue += 0.01f;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Blue Weirdness", currentBlue);
        }
    }
}
