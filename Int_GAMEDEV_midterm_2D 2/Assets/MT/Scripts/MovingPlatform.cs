using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]
    Movement[] movements;
    int current = 0;
    enum State { MOVING, WAITING };
    State state = State.MOVING;
    float waitTimer = 0f;

    [SerializeField]
    bool differentWhenStoodOn = false;

    [SerializeField]
    Movement[] stoodOnMovements;

    Movement[] activePoints;
    bool stoodOn = false;
    bool stepOffFlag = false;

    private void Start()
    {
        activePoints = movements;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (stepOffFlag && Vector2.Distance(PlayerController.instance.transform.position, rb.position) > 2f)
        {
            stepOffFlag = false;
            StepOff();
        }

        switch(state)
        {
            case State.MOVING:
                Vector2 dir = activePoints[current].position - rb.position;
                dir = dir.normalized;
                Vector2 newPos = rb.position + (dir * activePoints[current].speed * Time.deltaTime);
                float clampedX = rb.position.x < activePoints[current].position.x ? Mathf.Clamp(newPos.x, rb.position.x, activePoints[current].position.x) : Mathf.Clamp(newPos.x, activePoints[current].position.x, rb.position.x);
                float clampedY = rb.position.y < activePoints[current].position.y ? Mathf.Clamp(newPos.y, rb.position.y, activePoints[current].position.y) : Mathf.Clamp(newPos.y, activePoints[current].position.y, rb.position.y);
                newPos = new Vector2(clampedX, clampedY);
                rb.MovePosition(newPos);
                if(Vector2.Distance(newPos, activePoints[current].position) <= 0.001f)
                {
                    state = State.WAITING;
                    waitTimer = 0f;
                }
                break;
            case State.WAITING:
                waitTimer += Time.deltaTime;
                if(waitTimer >= activePoints[current].postMoveWaitTime)
                {
                    state = State.MOVING;
                    current++;
                    if (current >= activePoints.Length)
                        current = 0;
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            StandOn();
            stepOffFlag = false;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player")) stepOffFlag = true;
    }

    public void StandOn()
    {
        if (differentWhenStoodOn == false) return;
        if(stoodOn == false)
        {
            stoodOn = true;
            activePoints = stoodOnMovements;
            SetCurrentToClosest();
        }
    }
    public void StepOff()
    {
        if (differentWhenStoodOn == false) return;
        if (stoodOn)
        {
            stoodOn = false;
            activePoints = movements;
            SetCurrentToClosest();
        }
    }

    void SetCurrentToClosest()
    {
        int closest = 0;
        float closestDist = Vector2.Distance(rb.position, activePoints[0].position);
        for (int i = 1; i < activePoints.Length; i++)
        {
            if (Vector2.Distance(rb.position, activePoints[i].position) < closestDist) closest = i;
        }
        current = closest;
    }
}

[System.Serializable]
public struct Movement
{
    public Vector2 position;
    public float speed;
    public float postMoveWaitTime;
}