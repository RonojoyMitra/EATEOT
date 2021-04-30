using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMaster : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject); //setting the master to don't destroy on load
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
