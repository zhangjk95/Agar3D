using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : Controller {

    IEnumerable<Transform> balls, otherballs;

	void Start () {

	}
	
	void Update () {
        updateBalls();
        Move(new Vector3(0, 0, 0), 100f);
	}

    void updateBalls()
    {
        balls = transform.GetComponentsInChildren<BallManager>().Select((ballManager) => ballManager.transform);
        otherballs = GameObject.FindGameObjectsWithTag("Player")
            .Where((player) => player != gameObject)
            .SelectMany((player) => player.transform.GetComponentsInChildren<BallManager>().Select((ballManager) => ballManager.transform));
    }
}
