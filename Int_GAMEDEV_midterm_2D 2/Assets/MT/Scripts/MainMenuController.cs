using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// THIS IS JUST A TEST SCRIPT
/// </summary>
public class MainMenuController : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SceneController.LoadScene(2);
        }
    }
}
