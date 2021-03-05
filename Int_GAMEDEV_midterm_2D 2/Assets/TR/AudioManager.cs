using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject); //Setting the audiomanager object to dontdestroyonload so we can choose to have sounds transition smoothly across scenes
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
