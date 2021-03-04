using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField]
    bool active;
    [SerializeField]
    Door door;

    bool lastFrame = false;

    void Update()
    {
        if(lastFrame != active)
        {
            if (active)
                this.Activate();
            else
                this.Deactivate();
            lastFrame = active;
        }
    }

    void Activate()
    {
        door.Open();
    }
    void Deactivate()
    {
        door.Close();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Box"))
            active = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Box"))
            active = false;
    }
}
