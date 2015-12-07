using UnityEngine;
using System.Collections;

public class PlayerController : Controller {

    float camRayLength = 100f;
    int floorMask;

    void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Split();
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        var mousePoint = getMousePoint();
        if (mousePoint != null)
        {
            Move(mousePoint.Value);
        }
    }

    void Split()
    {
        var mousePoint = getMousePoint();
        if (mousePoint != null)
        {
            Split(mousePoint.Value);
        }
    }

    Vector3? getMousePoint()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            return floorHit.point;
        }
        return null;
    }
}
