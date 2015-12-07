using UnityEngine;
using System.Collections;
using System.Linq;

public class BallManager : MonoBehaviour {
    public string PlayerName = "user";
    public int number = 0;
    public float size = 64;
    public float incSize = 100;
    public float resizeDuration = 0.1f;
    public float eatDuration = 2;
    public bool needUpdate = false;
    public float maxVelocity = 10;
    public float initialVelocity = 30;
    public float initialMovementTimer = 1;
    public float initialColliderTimer = 1;
    public float minSize = 16;
    public Controller controller;

    private GameObject gameManager;
    private PickupManager pickupManager;
    private Transform player;
    private GameObject playerBall;
    private PlayerManager playerManager;
    private Rigidbody rigidBody;
    private GameObject cloth;
    private GameObject charge;
    private float displayRadius;
    private float oldRadius;
    private float newRadius;
    private bool resizing;
    public bool movementEnabled;
    public bool selfColliderEnabled;
    public Vector3 eatDirection;
    public float movementTimer;
    public float colliderTimer;
    public float eatTimer;
    public float resizeTimer;

    public bool SelfColliderEnabled
    {
        get { return selfColliderEnabled; }
        set 
        { 
            selfColliderEnabled = value;
            GetComponent<SphereCollider>().isTrigger = !value;
        }
    }

    public bool MovementEnabled
    {
        get { return movementEnabled; }
        set { movementEnabled = value; }
    }

    private float radius
    {
        get
        {
            return Mathf.Pow(size, 1f / 3);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (transform.parent.GetComponentsInChildren<BallManager>().Any((other) => other.number == number && other.size > size))
        {
            Destroy(gameObject);
            return;
        }

        gameManager = GameObject.Find("GameManager");
        pickupManager = gameManager.GetComponent<PickupManager>();
        player = transform.parent;
        playerBall = gameObject;
        playerManager = player.Find("PlayerManager").GetComponent<PlayerManager>();
        rigidBody = GetComponent<Rigidbody>();
        cloth = transform.Find("Cloth").gameObject;
        charge = transform.Find("Charge_01.1").gameObject;

        displayRadius = radius;
        transform.localScale = new Vector3(displayRadius, displayRadius, displayRadius);
        cloth.GetComponent<Cloth>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;
        controller.enabled = true;
        SelfColliderEnabled = false;
        MovementEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        movementTimer -= Time.deltaTime;
        if (movementTimer <= 0 && !MovementEnabled)
        {
            MovementEnabled = true;
        }

        colliderTimer -= Time.deltaTime;
        if (colliderTimer <= 0 && !SelfColliderEnabled)
        {
            SelfColliderEnabled = true;
        }

        eatTimer -= Time.deltaTime;
        if (eatTimer >= 0 && charge.activeSelf)
        {
            if (eatTimer >= eatDuration / 2)
            {
                charge.transform.localPosition = Vector3.Lerp(Vector3.zero, eatDirection, (eatDuration - eatTimer) / (eatDuration / 2));
            }
            else
            {
                charge.transform.localPosition = Vector3.Lerp(eatDirection, Vector3.zero, (eatDuration / 2 - eatTimer) / (eatDuration / 2));
            }
        }
        else if (charge.activeSelf)
        {
            charge.SetActive(false);
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

        transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    void LateUpdate()
    {
        if (needUpdate)
        {
            displayRadius = radius;
            transform.localScale = new Vector3(displayRadius, displayRadius, displayRadius);
            cloth.GetComponent<Cloth>().enabled = true;
            var velocity = rigidBody.velocity;
            Destroy(playerBall);
            GameObject newPlayer = Instantiate(playerBall);
            newPlayer.transform.SetParent(playerBall.transform.parent);
            newPlayer.GetComponent<BallManager>().needUpdate = false;
            newPlayer.GetComponent<BallManager>().enabled = true;
            newPlayer.GetComponent<BallManager>().eatDirection = eatDirection;
            newPlayer.GetComponent<Rigidbody>().velocity = velocity;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up"))
        {
            pickupManager.count--;
            Destroy(other.gameObject);
            ShowEatAnimation();

            size += incSize;

            if (radius - displayRadius > 0.5)
            {
                ShowResizeAnimation();
            }
        }
    }

    void ShowEatAnimation()
    {
        charge.GetComponent<ParticleSystem>().Clear(true);
        charge.GetComponent<ParticleSystem>().Play(true);
        eatDirection = Quaternion.Euler(0, Random.value * 360, 0) * (new Vector3(1, 1, 1));
        eatTimer = eatDuration;
        charge.SetActive(true);
    }

    void ShowResizeAnimation()
    {
        oldRadius = displayRadius;
        newRadius = radius;
        resizeTimer = resizeDuration;
        resizing = true;
        cloth.GetComponent<Cloth>().enabled = false;
    }

    public void Move(Vector3 direction, float velocity)
    {
        if (MovementEnabled)
        {
            velocity = Mathf.Clamp(velocity, 0, maxVelocity);
            direction.y = 0;
            direction.Normalize();
            direction *= velocity;
            rigidBody.velocity = direction;
            rigidBody.angularVelocity = Vector3.zero;
        }
        else
        {
            Debug.Log("Movement disabled");
        }
    }

    public void Split(Vector3 direction)
    {
        lock (playerManager)
        {
            if (playerManager.count >= playerManager.maxCount || size / 2 < minSize) return;

            direction.y = 0;
            direction.Normalize();

            size /= 2;

            GameObject newPlayer = Instantiate(playerBall);
            newPlayer.transform.SetParent(player);
            var newBallManager = newPlayer.GetComponent<BallManager>();
            newBallManager.movementTimer = initialMovementTimer;
            newBallManager.colliderTimer = initialColliderTimer;
            newBallManager.MovementEnabled = false;
            newBallManager.SelfColliderEnabled = false;
            newBallManager.number = playerManager.count++;
            newPlayer.GetComponent<Rigidbody>().velocity = direction * initialVelocity;
            newBallManager.enabled = true;

            needUpdate = true;
            newBallManager.needUpdate = true;
        }
    }
}
