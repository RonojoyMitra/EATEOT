using UnityEngine;

public class sc_movement : MonoBehaviour
{
    public LayerMask groundMask;
    public SpriteRenderer thisSprite;
    public Rigidbody2D thisRigid2D;
    public BoxCollider2D thisCollider2D;
    public float force = 10;
    public float jumpForce = 10;
    public Vector3 movementVector;
    public float maxDistance = 1f;
    public float speed = 1f;


    void Start()
    {
        GetComponent<Rigidbody2D>();
        thisSprite = GetComponent<SpriteRenderer>();
    }

    public bool moving;


    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A))
        {
            //  thisRigid2D.AddForce(-Vector2.right * force * Time.fixedDeltaTime, ForceMode2D.Impulse);
            transform.Translate(-speed * Time.deltaTime, 0, 0);
            if (thisSprite.flipX == true)
            {
                thisSprite.flipX = false;
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            // thisRigid2D.AddForce(Vector2.right * force * Time.fixedDeltaTime, ForceMode2D.Impulse);
            transform.Translate(speed * Time.deltaTime, 0, 0);
            if (thisSprite.flipX == false)
            {
                thisSprite.flipX = true;
            }
        }

        //Ray myRay = new Ray(transform.position, Vector2.down);
        //bool onGround = Physics2D.Raycast(myRay.origin, myRay.direction, maxDistance);
        //Debug.DrawRay(myRay.origin, myRay.direction * maxDistance, Color.white);

        //if (onGround){
        //    print("onGround");
        //    thisSprite.color = Color.red;
        //}
        //else {
        //    thisSprite.color = Color.green;
        //}
    }

    public bool onGround()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 0.8f;

        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundMask);
        if (hit.collider != null)
        {
            return true;
        }

        return false;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (onGround())
            {
                thisRigid2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Mushroom")){
            Destroy(other.gameObject);
        }
    }
}

