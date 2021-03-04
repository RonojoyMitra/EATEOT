using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishingBlock : MonoBehaviour
{
    Animator animator;

    float timer = 0f;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
    }

    public void Vanish()
    {
        if (timer > 0f)
            return;
        animator.SetTrigger("Vanish");
        timer = 0.5f;
    }
}
