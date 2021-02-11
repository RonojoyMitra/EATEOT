using UnityEngine;

public class sc_camera : MonoBehaviour
{
    public Transform spr_MC;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // transform.position = new Vector3(player_MUSHROOM.position.x, player_MUSHROOM.position.y, transform.position.z);

        Vector3 pos = transform.position;

        if (transform.position.x > spr_MC.transform.position.x + 1)
            pos.x = spr_MC.transform.position.x + 1;
        if (transform.position.x < spr_MC.transform.position.x - 1)
            pos.x = spr_MC.transform.position.x - 1;
        if (transform.position.y > spr_MC.transform.position.y + 1)
            pos.y = spr_MC.transform.position.y + 1;
        if (transform.position.y < spr_MC.transform.position.y - 1)
            pos.y = spr_MC.transform.position.y - 1;

        transform.position = pos;

    }
}
