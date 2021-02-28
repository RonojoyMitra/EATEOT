using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]
    Movement[] movements;
    int index = 0;
    float timer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= movements[index].duration)
        {
            index++;
            timer = 0f;
            if (index >= movements.Length)
                index = 0;
        }

        rb.MovePosition(Vector2.Lerp(StartingPos(), TargetPos(), timer / movements[index].duration));
    }

    Vector2 StartingPos()
    {
        return movements[index].position;
    }
    Vector2 TargetPos()
    {
        if (index >= movements.Length - 1)
            return movements[0].position;
        return movements[index + 1].position;
    }
}

[System.Serializable]
public struct Movement
{
    public Vector2 position;
    public float duration;
}