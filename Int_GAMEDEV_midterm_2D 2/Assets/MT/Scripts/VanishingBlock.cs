using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishingBlock : MonoBehaviour
{
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Vanish()
    {
        animator.SetTrigger("Vanish");
    }
}
