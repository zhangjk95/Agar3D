using UnityEngine;
using System.Collections;
using System.Linq;

public class GameController : MonoBehaviour {
    public string PlayerName;
    public int number;
    public float size;
    public float movementTimer;
    public float colliderTimer;
    public float eatTimer;
    public float resizeTimer;
    public float incSize;
    public float resizeDuration;
    public float eatDuration;
    public bool needUpdate = false;
    public CameraFollow cameraFollow;
    public Vector3 eatDirection;

    private PickupManager PM;
    private GameObject Player;
    private GameObject playerManager;

    private float radius
    {
        get
        {
            return Mathf.Pow(size, 1f / 3);
        }
    }
    private float displayRadius;
    private float oldRadius;
    private float newRadius;
    private bool resizing;

	// Use this for initialization
	void Start () {
        if (GameObject.FindGameObjectsWithTag("Player").Any((player) => player.GetComponent<GameController>().PlayerName == PlayerName && player.GetComponent<GameController>().number == number && player.GetComponent<GameController>().size > size))
        {
            Destroy(gameObject);
            return;
        }

        PM = GameObject.Find("GameManager").GetComponent<PickupManager>();
        Player = gameObject;
        playerManager = GameObject.Find("PlayerManager");
        displayRadius = radius;
        transform.localScale = new Vector3(displayRadius, displayRadius, displayRadius);
        transform.GetChild(0).GetComponent<Cloth>().enabled = true;
        GetComponent<PlayerSplit>().enabled = true;
        GetComponent<SphereCollider>().isTrigger = true;
        GetComponent<SphereCollider>().enabled = true;
        if (number == 0)
        {
            cameraFollow.Target = transform;
        }
	}
	
	// Update is called once per frame
	void Update () {
        movementTimer -= Time.deltaTime;
        if (movementTimer <= 0 && !GetComponent<PlayerMovement>().enabled)
        {
            GetComponent<PlayerMovement>().enabled = true;
        }

        colliderTimer -= Time.deltaTime;
        if (colliderTimer <= 0 && GetComponent<SphereCollider>().isTrigger)
        {
            GetComponent<SphereCollider>().isTrigger = false;
        }

        eatTimer -= Time.deltaTime;
        if (eatTimer >= 0 && transform.GetChild(1).gameObject.activeSelf)
        {
            if (eatTimer >= eatDuration / 2) {
                transform.GetChild(1).localPosition = Vector3.Lerp(Vector3.zero, eatDirection, (eatDuration - eatTimer) / (eatDuration / 2));
            }
            else
            {
                transform.GetChild(1).localPosition = Vector3.Lerp(eatDirection, Vector3.zero, (eatDuration / 2 - eatTimer) / (eatDuration / 2));
            }
        }
        else if (transform.GetChild(1).gameObject.activeSelf)
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }

        resizeTimer -= Time.deltaTime;
        if (resizeTimer >= 0 && resizing)
        {
            displayRadius = Mathf.Lerp(oldRadius, newRadius, (resizeDuration - resizeTimer) / resizeDuration);
            transform.localScale = new Vector3(displayRadius, displayRadius, displayRadius);
        }
        else if (resizing)
        {
            resizing = false;
            needUpdate = true;
        }
	}

    void LateUpdate()
    {
        if (needUpdate)
        {
            displayRadius = radius;
            transform.localScale = new Vector3(displayRadius, displayRadius, displayRadius);
            transform.GetChild(0).GetComponent<Cloth>().enabled = true;
            var velocity = GetComponent<Rigidbody>().velocity;
            Destroy(Player);
            GameObject newPlayer = Instantiate(Player);
            newPlayer.transform.SetParent(Player.transform.parent);
            newPlayer.GetComponent<GameController>().needUpdate = false;
            newPlayer.GetComponent<GameController>().enabled = true;
            newPlayer.GetComponent<GameController>().eatDirection = eatDirection;
            newPlayer.GetComponent<Rigidbody>().velocity = velocity;
        }
    }

	void OnTriggerEnter (Collider other) {
		if (other.gameObject.CompareTag ("Pick Up")) {
			PM.count --;
            Destroy(other.gameObject);
            ShowEatAnimation();

            lock (playerManager)
            {
                //Player = GameObject.FindGameObjectsWithTag("Player").Where((player) => player.GetComponent<GameController>().PlayerName == PlayerName).First();
                size += incSize;

                if (radius - displayRadius > 0.5)
                {
                    ShowResizeAnimation();
                }
            }
		}
	}

    void ShowEatAnimation()
    {
        transform.GetChild(1).GetComponent<ParticleSystem>().Clear(true);
        transform.GetChild(1).GetComponent<ParticleSystem>().Play(true);
        eatDirection = Quaternion.Euler(0, Random.value * 360, 0) * (new Vector3(1, 1, 1)/* * displayRadius / 4*/);
        eatTimer = eatDuration;
        transform.GetChild(1).gameObject.SetActive(true);
    }

    void ShowResizeAnimation()
    {
        oldRadius = displayRadius;
        newRadius = radius;
        resizeTimer = resizeDuration;
        resizing = true;
        transform.GetChild(0).GetComponent<Cloth>().enabled = false;
    }
}
