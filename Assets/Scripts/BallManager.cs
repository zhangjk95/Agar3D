using UnityEngine;
using System;
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
    public float splitTime = 5;
    public float mergeDuration = 5;

    private GameObject gameManager;
    private PickupManager pickupManager;
    public Transform player;
    private GameObject playerBall;
    public PlayerManager playerManager;
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
    public Vector3 positionBeforeMerge;
    public float distanceBeforeMerge;
    public float movementTimer;
    public float colliderTimer;
    public float eatTimer;
    public float resizeTimer;
    public float splitTimer;
    public float mergeTimer;
    public int splitFrom = -1;
    public int merged = 0;

    public bool SelfColliderEnabled
    {
        get { return selfColliderEnabled; }
        set 
        { 
            selfColliderEnabled = value;
            Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer, !value);
        }
    }

    public bool MovementEnabled
    {
        get { return movementEnabled; }
        set { movementEnabled = value; }
    }

    public Vector3 position
    {
        get { return transform.position; }
        //set { transform.position = value; }
    }

    private float radius
    {
        get
        {
            return Mathf.Pow(size, 1f / 3);
        }
    }

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
        SelfColliderEnabled = false;
        MovementEnabled = false;
    }

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

        if (merged == 0 && splitFrom != -1 && ballExists(splitFrom))
        {
            var mergedBall = getBallByNumber(splitFrom);

            splitTimer -= Time.deltaTime;
            if (splitTimer < 0 && mergeTimer < 0)
            {
                mergeTimer = mergeDuration;
                GetComponent<SphereCollider>().isTrigger = true;
                MovementEnabled = false;
                positionBeforeMerge = position;
                distanceBeforeMerge = (position - mergedBall.position).magnitude;
            }

            mergeTimer -= Time.deltaTime;
            if (splitTimer < 0)
            {
                if (mergeTimer >= 0)
                {
                    var distance = Mathf.Lerp(distanceBeforeMerge, 0, (mergeDuration - mergeTimer) / mergeDuration);
                    transform.position = new Ray(mergedBall.position, positionBeforeMerge - mergedBall.position).GetPoint(distance);
                }
                else
                {
                    mergedBall.size += size;
                    mergedBall.merged--;
                    if (mergedBall.radius - mergedBall.displayRadius > 0.5)
                    {
                        mergedBall.ShowResizeAnimation();
                    }
                    lock (playerManager)
                    {
                        playerManager.count--;
                        Destroy(gameObject);
                    }
                }
            }
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
            newPlayer.transform.position = position;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        lock (gameManager)
        {
            if (!(other.gameObject == null) && other.gameObject.CompareTag("Pick Up"))
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
    }

    void OnTriggerStay(Collider other)
    {
        lock (gameManager)
        {
            if (!(other.gameObject == null) && other.gameObject.CompareTag("Player Ball"))
            {
                var otherBall = other.GetComponent<BallManager>();
                if (otherBall.player.GetComponentsInChildren<BallManager>().Any((ball) => ball.mergeTimer > 0)) return;
                if (otherBall.transform.parent != player && radius > other.GetComponent<BallManager>().radius + (position - otherBall.position).magnitude)
                {
                    int end = otherBall.adjustNumber();
                    if (end != -1 && otherBall.ballExists(end))
                    {
                        otherBall.getBallByNumber(end).merged--;
                    }
                    otherBall.playerManager.count--;
                    if (otherBall.playerManager.count == 0)
                    {
                        otherBall.player.gameObject.SetActive(false);
                    }
                    Destroy(other.gameObject);

                    size += otherBall.size;

                    if (radius - displayRadius > 0.5)
                    {
                        ShowResizeAnimation();
                    }
                }
            }
        }
    }

    int adjustNumber()
    {
        var tmp = player.GetComponentsInChildren<BallManager>().FirstOrDefault((ball) => ball.splitFrom == number &&
            !player.GetComponentsInChildren<BallManager>().Any((otherBall) => otherBall.splitFrom == number && otherBall.merged > ball.merged));
        if (tmp != null)
        {            
            int end = tmp.adjustNumber();
            tmp.number = number;
            tmp.splitTimer = splitTimer;
            tmp.splitFrom = splitFrom;
            return end;
        }
        else
        {
            return splitFrom;
        }
    }

    void ShowEatAnimation()
    {
        charge.GetComponent<ParticleSystem>().Clear(true);
        charge.GetComponent<ParticleSystem>().Play(true);
        eatDirection = Quaternion.Euler(0, UnityEngine.Random.value * 360, 0) * (new Vector3(1, 1, 1));
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

    public BallManager getBallByNumber(int number)
    {
        lock (playerManager)
        {
            return player.GetComponentsInChildren<BallManager>().First((ball) => ball.number == number);
        }  
    }

    bool ballExists(int number)
    {
        lock (playerManager)
        {
            return player.GetComponentsInChildren<BallManager>().Any((ball) => ball.number == number);
        }  
    }

    int getNewBallNumber()
    {
        lock (playerManager)
        {
            for (int i = 0; i < playerManager.maxCount; i++)
            {
                if (!ballExists(i)) return i;
            }
            return -1;
        }
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
    }

    public void Split(Vector3 direction)
    {
        lock (playerManager)
        {
            if (player.GetComponentsInChildren<BallManager>().Any((ball) => ball.mergeTimer > 0)) return;
            if (playerManager.count >= playerManager.maxCount || size / 2 < minSize) return;

            direction.y = 0;
            direction.Normalize();

            size /= 2;
            merged++;

            GameObject newPlayer = Instantiate(playerBall);
            newPlayer.transform.SetParent(player);
            newPlayer.transform.position = position;
            var newBallManager = newPlayer.GetComponent<BallManager>();
            newBallManager.movementTimer = initialMovementTimer;
            newBallManager.colliderTimer = initialColliderTimer;
            newBallManager.MovementEnabled = false;
            newBallManager.SelfColliderEnabled = false;
            newBallManager.number = getNewBallNumber();
            playerManager.count++;
            newBallManager.splitFrom = number;
            newBallManager.merged = 0;
            newBallManager.splitTimer = splitTime;
            newPlayer.GetComponent<Rigidbody>().velocity = direction * initialVelocity;
            newBallManager.enabled = true;

            needUpdate = true;
            newBallManager.needUpdate = true;
        }
    }
}
