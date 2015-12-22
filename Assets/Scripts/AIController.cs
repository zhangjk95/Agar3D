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
	public float AIVisionField = 50f;
	public float SplitRange = 20f;
	public int FloorScale = 100;
	private float MovementTimer;
	private float AIJudgeTimer;
	private GameObject[] Pickups;
	private ShelterManager[] Shelters;
	private Vector3 MoveDirection;
	//private Vector3 ShelterAvoid;
	private string state;
    //public float deltaTime = 1f;

	void Start () {
		IsTargetBallsinVision = false;
		IsPickUpinVision = false;
		MovementTimer = 0;
		AIJudgeTimer = 0;
		MoveDirection = Vector3.zero;
        //InvokeRepeating("run", deltaTime, deltaTime);
		state = "normal";
	}

	private float Calibrate(float Angle)
	{
		float angle = Angle;
		if (angle < 0)
		{
			angle += 360;
		}
		if (angle > 270)
		{
			angle = 360 - angle;
		}
		if (angle > 180)
		{
			angle -= 180;
		}
		if (angle > 90)
		{
			angle = 180 - angle;
		}
		return angle;
	}

	void Update () {
        if (!isActiveAndEnabled) return;
        updateBalls();
		SmallestBallSize = 100000;
        foreach (var ball in balls) {
			if(ball.size < SmallestBallSize) {
				SmallestBallSize = ball.size;
				SmallestBall = ball;
			}
        }

		IsTargetBallsinVision = false;
		BallManager Persue;
		bool IsEscape = false;
		Persue = null;
		List<BallManager> Escape = new List<BallManager>();
		Vector3 escapeDirection = new Vector3(0, 0, 0);

		foreach (var otherball in otherballs)
		{
			if (Vector3.Distance(otherball.position, SmallestBall.position) < AIVisionField)
			{
				if (otherball.size > SmallestBall.size && otherball.radius - SmallestBall.radius > 0.5)
				{
					Escape.Add(otherball);
					IsTargetBallsinVision = true;
					IsEscape = true;
					break;
				}
			}
		}

		if (state == "normal")
		{
			if (IsEscape)
			{
				Vector3 CaliescapeDirection;
				escapeDirection = new Vector3(0, 0, 0);
				foreach (var escape in Escape)
				{
					escapeDirection += SmallestBall.position - escape.position;
					Debug.DrawLine(SmallestBall.position, escape.position, Color.red);
				}
				CaliescapeDirection = escapeDirection;
				if (SmallestBall.position.x + SmallestBall.radius > FloorScale / 2 || SmallestBall.position.x - SmallestBall.radius < -FloorScale / 2)
				{
					CaliescapeDirection.x = 0;
				}
				if (SmallestBall.position.z + SmallestBall.radius > FloorScale / 2 || SmallestBall.position.z - SmallestBall.radius < -FloorScale / 2)
				{
					CaliescapeDirection.z = 0;
				}
				if (CaliescapeDirection.x == 0 && CaliescapeDirection.z == 0)
				{
					float MinangleH = 367, MinangleV = 367;
					foreach (var escapeball in Escape)
					{
						float angleV = Vector3.Angle(new Vector3(0, 0, 1), escapeball.position - SmallestBall.position);
						angleV = Calibrate(angleV);
						float angleH = Vector3.Angle(new Vector3(1, 0, 0), escapeball.position - SmallestBall.position);
						angleH = Calibrate(angleH);
						if (angleV < MinangleV)
						{
							MinangleV = angleV;
						}
						if(angleH < MinangleH)
						{
							MinangleH = angleH;
						}
					}
					if (MinangleV > MinangleH)
					{
						CaliescapeDirection.z = -escapeDirection.z * 100;
					}
					else
					{
						CaliescapeDirection.x = -escapeDirection.x * 100;
					}
					state = "corner";
				}
				MoveDirection = CaliescapeDirection + SmallestBall.position;
			}

			else
			{
				float MinDistance = 100000, Temp_Distance = 0;
				foreach (var otherball in otherballs)
				{
					Temp_Distance = Vector3.Distance(otherball.position, SmallestBall.position);
					if (Temp_Distance < AIVisionField && SmallestBall.radius - otherball.radius > 0.5 && Temp_Distance < MinDistance)
					{
						MinDistance = Temp_Distance;
						Persue = otherball;
						IsTargetBallsinVision = true;
					}
				}
				if (Persue)
				{
					MoveDirection = Persue.position;
					if (Persue.size < SmallestBall.size / 2)
					{
						if (Vector3.Distance(Persue.position, SmallestBall.position) < SplitRange)
						{
							AIJudgeTimer += Time.deltaTime;
							if (AIJudgeTimer > AIJudgeTime)
							{
                                Vector3 ForecastSplitPoint = new Ray(SmallestBall.position, Persue.position - SmallestBall.position).GetPoint(2 * SmallestBall.radius);
                                if (!checkShelterCollision(ForecastSplitPoint).HasValue)
                                {
                                    Split(Persue.position);
                                }
								AIJudgeTimer = 0;
							}
						}
						else
						{
							AIJudgeTimer = 0;
						}
					}
				}
				else
				{
					Pickups = GameObject.FindGameObjectsWithTag("Pick Up");
					IsPickUpinVision = false;
					foreach (var Pickup in Pickups)
					{
						if (Vector3.Distance(SmallestBall.position, Pickup.transform.position) < AIVisionField)
						{
							MoveDirection = Pickup.transform.position;
							IsPickUpinVision = true;
						}
					}

				}
			}
		}
		else if (state == "corner")
		{
			escapeDirection = new Vector3(0, 0, 0);
			bool EjectorAlive = false;
			foreach (var escape in Escape)
			{
				escapeDirection += SmallestBall.position - escape.position;
				EjectorAlive = true;
				Debug.DrawLine(SmallestBall.position, escape.position, Color.red);
			}
			if (Vector3.Dot(MoveDirection - SmallestBall.position, escapeDirection) > 0) {
				state = "normal";
			}
			if(!EjectorAlive) {
				state = "normal";
			}
		}

        MovementTimer += Time.deltaTime;
		if (!IsPickUpinVision && !IsTargetBallsinVision) {
			if (MovementTimer > Random.Range(27, 34) / 10)
			{
				MoveDirection = new Vector3(Random.Range(-FloorScale / 2, FloorScale / 2), 0, Random.Range(-FloorScale / 2, FloorScale / 2));
				MovementTimer = 0;
			}
		}

		Debug.DrawLine(MoveDirection, SmallestBall.position, Color.yellow);

		Vector3 ForecastPoint = new Ray(SmallestBall.position, MoveDirection - SmallestBall.position).GetPoint(2 * SmallestBall.radius);
		if (checkShelterCollision(ForecastPoint).HasValue) {
			Vector3 shelterPosition = checkShelterCollision(ForecastPoint).Value;
			int avoidsign = -1;
			if (Vector3.Cross (MoveDirection, shelterPosition).y < 0) {
				avoidsign = 1;
			}
			Debug.Log (avoidsign);
			int totalrotation = 0;
			while (checkShelterCollision(ForecastPoint).HasValue) {
				Vector3 AvoidDirection = MoveDirection - SmallestBall.position;
				Quaternion rotation = Quaternion.AngleAxis (avoidsign * 2, new Vector3 (0, 1, 0));
				totalrotation +=2;
				if(totalrotation > 180) {
					break;
				}
				AvoidDirection = rotation * AvoidDirection;
				MoveDirection = SmallestBall.position + AvoidDirection;
				ForecastPoint = new Ray(SmallestBall.position, MoveDirection - SmallestBall.position).GetPoint( SmallestBall.radius);
			}
		}

		Move (MoveDirection, 100f);
		Debug.DrawLine(MoveDirection, SmallestBall.position);

	}

	Vector3? checkShelterCollision(Vector3 ForecastPoint) {
		foreach (var shelter in Shelters) {
			if (shelter.radius < SmallestBall.radius && Vector3.Distance (ForecastPoint, shelter.transform.position) < shelter.radius + SmallestBall.radius ) {
				Debug.DrawLine (shelter.transform.position, SmallestBall.position, Color.blue);
				return shelter.transform.position;
			}
		}
		return null;
	}

    void updateBalls()
    {
        balls = transform.GetComponentsInChildren<BallManager>();
        otherballs = GameObject.FindGameObjectsWithTag("Player")
            .Where((player) => player != gameObject)
            .SelectMany((player) => player.transform.GetComponentsInChildren<BallManager>());
		Shelters = GameObject.Find("Shelters").GetComponentsInChildren<ShelterManager>();
    }
}
