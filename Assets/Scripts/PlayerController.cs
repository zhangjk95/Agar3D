using UnityEngine;
using System.Collections;

public class PlayerController : Controller {

    float camRayLength = 100f;
    int floorMask;
    BallManager ballManager;

    void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
        ballManager = GetComponent<BallManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Split();
        }
        else if (Input.GetKeyDown("r"))
        {
            ballManager.needUpdate = true;
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        var playerToMouse = getPlayerToMouse();
        ballManager.Move(playerToMouse, playerToMouse.magnitude);
    }

    void Split()
    {
        var playerToMouse = getPlayerToMouse();
        if (playerToMouse != Vector3.zero)
        {
            ballManager.Split(playerToMouse);
        }
    }

    Vector3 getPlayerToMouse()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            Vector3 playerToMouse = floorHit.point - transform.position;
            return playerToMouse;
        }
        return Vector3.zero;
    }
}
