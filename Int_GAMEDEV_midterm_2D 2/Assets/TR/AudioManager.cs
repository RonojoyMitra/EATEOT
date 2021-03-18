using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
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
        
    }
}
