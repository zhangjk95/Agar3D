using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : Controller {

    IEnumerable<BallManager> balls, otherballs;

	void Start () {

	}
	
	void Update () {
        updateBalls();
        foreach (var ball in balls)
        {
            Debug.Log(ball.position); //球的位置
            Debug.Log(ball.size); //球的大小
        }
        Move(new Vector3(0, 0, 0), 100f);
	}

    void updateBalls()
    {
        balls = transform.GetComponentsInChildren<BallManager>();
        otherballs = GameObject.FindGameObjectsWithTag("Player")
            .Where((player) => player != gameObject)
            .SelectMany((player) => player.transform.GetComponentsInChildren<BallManager>());
    }
}
