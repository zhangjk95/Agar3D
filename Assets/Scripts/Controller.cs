using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    protected void Move(Vector3 mousePoint, float velocity)
    {
        var ballManagers = transform.GetComponentsInChildren<BallManager>();
        foreach (var ballManager in ballManagers)
        {
            Vector3 ballToMouse = mousePoint - ballManager.transform.position;
            ballManager.Move(ballToMouse, velocity);
        }
    }

    protected void Move(Vector3 mousePoint)
    {
        var ballManagers = transform.GetComponentsInChildren<BallManager>();
        foreach (var ballManager in ballManagers)
        {
            Vector3 ballToMouse = mousePoint - ballManager.transform.position;
            ballManager.Move(ballToMouse, ballToMouse.magnitude);
        }
    }

    protected void Split(Vector3 mousePoint)
    {
        var ballManagers = transform.GetComponentsInChildren<BallManager>();
        foreach (var ballManager in ballManagers)
        {
            Vector3 ballToMouse = mousePoint - ballManager.transform.position;
            ballManager.Split(ballToMouse);
        }
    }
}
