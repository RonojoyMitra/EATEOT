using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sc_start : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public string levelName;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)){
            SceneManager.LoadScene(levelName);
        }
        
    }
}
