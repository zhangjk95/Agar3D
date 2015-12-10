using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : Controller {

    IEnumerable<BallManager> balls, otherballs;
	private float SmallestBallSize;
	private BallManager SmallestBall;
	private bool IsTargetBallsinVision;
	private bool IsPickUpinVision;
	public float AIJudgeTime = 2.0f;
	public float AIVisionField = 30f;
	public float SplitRange = 20f;
	public int FloorScale = 100;
	private float MovementTimer;
	private float AIJudgeTimer;
	private GameObject[] Pickups;

	void Start () {
		IsTargetBallsinVision = false;
		IsPickUpinVision = false;
		MovementTimer = 0;
		AIJudgeTimer = 0;
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
		BallManager Persue, Escape;
		Persue = Escape = null;
		foreach (var otherball in otherballs) {
			if (Vector3.Distance (otherball.position, SmallestBall.position) < AIVisionField) {
				if (otherball.size > SmallestBall.size) {
					Escape = otherball;
					IsTargetBallsinVision = true;
					break;
				}
			}
		}

		if (Escape) {
			Move ((SmallestBall.position - Escape.position) * 5, 100f);
		}

		else  {
			float MinDistance = 100000, Temp_Distance = 0;
			foreach (var otherball in otherballs) {
				Temp_Distance = Vector3.Distance (otherball.position, SmallestBall.position);
				if (Temp_Distance < AIVisionField && otherball.size < SmallestBall.size && Temp_Distance < MinDistance) {
					MinDistance = Temp_Distance;
					Persue = otherball;
					IsTargetBallsinVision = true;
				}
			}
			if (Persue) {
				Move (Persue.position, 100f);
				if(Persue.size < SmallestBall.size/2) {
					if(Vector3.Distance(Persue.position, SmallestBall.position) < SplitRange) {
						AIJudgeTimer += Time.deltaTime;
						if(AIJudgeTimer > AIJudgeTime) {
							Split (Persue.position);
							AIJudgeTimer = 0;
						}
					}
					else {
						AIJudgeTimer = 0;
					}
				}
			}
			else {
				Pickups = GameObject.FindGameObjectsWithTag ("Pick Up");
				IsPickUpinVision = false;
				foreach (var Pickup in Pickups) {
					if (Vector3.Distance (SmallestBall.position, Pickup.transform.position) < AIVisionField) {
						Move (Pickup.transform.position, 100f);
						IsPickUpinVision = true;
					}
				}
				
			}
		}


		MovementTimer += Time.deltaTime;
		if (!IsPickUpinVision && !IsTargetBallsinVision && MovementTimer > Random.Range(27, 34)/10) {
			Move (new Vector3(Random.Range(- FloorScale/2, FloorScale/2), Random.Range(- FloorScale/2, FloorScale/2), 0), 100f);
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
