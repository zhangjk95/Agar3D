using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : Controller {

    IEnumerable<BallManager> balls, otherballs;
	private float SmallestBallSize;
	private BallManager SmallestBall;
	private bool IsTargetBallsinVision;
	public float AIVisionField = 30f;
	private float MovementTimer;
	void Start () {
		IsTargetBallsinVision = false;
		MovementTimer = 0;
		Move (new Vector3(Random.Range(1, 30), Random.Range(1, 30), Random.Range(1, 30)), 100f);
	}
	
	void Update () {
        updateBalls();
		SmallestBallSize = 100000;
        foreach (var ball in balls) {
			if(ball.size < SmallestBallSize) {
				SmallestBallSize = ball.size;
				SmallestBall = ball;
			}
        }

		IsTargetBallsinVision = false;
		foreach (var otherball in otherballs) {
			if(Vector3.Distance(otherball.position, SmallestBall.position) < AIVisionField) {
				if(otherball.size > SmallestBall.size) {
					Move ((SmallestBall.position - otherball.position) * 5, 100f);
					IsTargetBallsinVision = true;
				}
				else if(otherball.size < SmallestBall.size) {
					Move ((otherball.position - SmallestBall.position) * 5, 100f);
					if(otherball.size < SmallestBall.size/2) {
						Split (SmallestBall.position);
					}
					IsTargetBallsinVision = true;
				}

			}
		}
		MovementTimer += Time.deltaTime;
		if (!IsTargetBallsinVision && MovementTimer > 1) {
			Move (new Vector3(Random.Range(1, 30), Random.Range(1, 30), Random.Range(1, 30)), 100f);
			MovementTimer = 0;
		}
	}

    void updateBalls()
    {
        balls = transform.GetComponentsInChildren<BallManager>();
        otherballs = GameObject.FindGameObjectsWithTag("Player")
            .Where((player) => player != gameObject)
            .SelectMany((player) => player.transform.GetComponentsInChildren<BallManager>());
    }
}
