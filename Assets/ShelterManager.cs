using UnityEngine;
using System.Collections;
using System.Linq;

public class ShelterManager : MonoBehaviour {

    private ShelterGenerator generator;

    public float size = 64;
    public float minImpulse = 5;
    public float sizeLoss = 1 / 4;
    public float initialVelocity = 30;

    public Vector3 position
    {
        get { return transform.position; }
        //set { transform.position = value; }
    }

	// Use this for initialization
	void Start () {
        generator = GameObject.Find("GameManager").GetComponent<ShelterGenerator>();
	}
	
	// Update is called once per frame
	void Update () {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players) {
            var balls = player.GetComponentsInChildren<BallManager>();
            if (balls.Any((ball) => ball.size >= size)) {
                Physics.IgnoreLayerCollision(gameObject.layer, balls.First().gameObject.layer, false);
            }
            else
            {
                Physics.IgnoreLayerCollision(gameObject.layer, balls.First().gameObject.layer, true);
            }
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player Ball"))
        {
            Debug.Log("impulse" + collision.impulse.magnitude);
            var ball = collision.gameObject.GetComponent<BallManager>();
            if (collision.impulse.magnitude >= minImpulse && ball.size > size)
            {
                Destroy(gameObject);
                generator.count--;
                var balls = ball.player.GetComponentsInChildren<BallManager>();
                var totalSize = balls.Sum((_ball) => _ball.size);
                var ballObject = ball.gameObject;
                ball.playerManager.count = 16;
                foreach (var oldball in balls)
                {
                    oldball.Enabled = false;
                    Destroy(oldball.gameObject);
                }
                totalSize -= totalSize * sizeLoss;
                for (int i = 0; i < 16; i++)
                {
                    var direction = Quaternion.Euler(0, UnityEngine.Random.value * 360, 0) * (new Vector3(1 / Mathf.Sqrt(2), 0, 1 / Mathf.Sqrt(2)));
                    GameObject newBallObject = Instantiate(ballObject);
                    var newBall = newBallObject.GetComponent<BallManager>();
                    newBall.size = totalSize / 16;
                    newBall.transform.SetParent(ball.transform.parent);
                    newBall.transform.position = position;
                    newBall.movementTimer = newBall.initialMovementTimer;
                    newBall.colliderTimer = newBall.initialColliderTimer;
                    newBall.MovementEnabled = false;
                    newBall.SelfColliderEnabled = false;
                    newBall.number = i;
                    if (i == 0)
                    {
                        newBall.splitFrom = -1;
                        newBall.merged = 4;
                        newBall.splitTimer = 0;
                    }
                    else
                    {
                        newBall.splitFrom = i - (int)Mathf.Pow(2, Mathf.FloorToInt(Mathf.Log(i) / Mathf.Log(2)));
                        newBall.merged = 3 - Mathf.FloorToInt(Mathf.Log(i) / Mathf.Log(2));
                        newBall.splitTimer = newBall.splitTime;
                    }
                    newBall.GetComponent<Rigidbody>().velocity = direction * initialVelocity;
                    newBall.enabled = true;

                    newBall.needUpdate = true;
                }
            }
        }
    }
}
