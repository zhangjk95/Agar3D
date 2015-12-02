using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public float maxVelocity = 3;

    float camRayLength = 100f;
    int floorMask;
    Rigidbody rb;
    float maxVelocitySqr;

    void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
        rb = GetComponent<Rigidbody>();
        maxVelocitySqr = maxVelocity * maxVelocity;
    }

    void Update()
    {
        transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            Vector3 playerToMouse = floorHit.point - transform.position;
            playerToMouse.y = 0;
            if (playerToMouse.sqrMagnitude > maxVelocitySqr)
            {
                playerToMouse.Normalize();
                playerToMouse = playerToMouse * maxVelocity;
            }
            rb.velocity = playerToMouse;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
