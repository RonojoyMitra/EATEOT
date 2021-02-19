using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPlatformController : MonoBehaviour
{
    Rigidbody2D rb;

    Vector2 targetPosition;

    [SerializeField]
    AnimationCurve springinessCurve;
    [SerializeField]
    float maxSpringiness;
    [SerializeField]
    float maxSpringinessDistance;

    void Start()
    {
        targetPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float springiness = springinessCurve.Evaluate(Vector2.Distance(transform.position, targetPosition) / maxSpringinessDistance) * maxSpringiness;
        Debug.Log(springiness);

        Vector2 dir = new Vector2(targetPosition.x - rb.position.x, targetPosition.y - rb.position.y);
        dir.Normalize();
        rb.AddForce(dir * springiness, ForceMode2D.Force);
    }
}
