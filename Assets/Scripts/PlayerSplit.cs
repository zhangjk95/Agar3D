using UnityEngine;
using System.Collections;

public class PlayerSplit : MonoBehaviour {

	public float initialVelocity;
    public float initialMovementTimer;
    public float initialColliderTimer;
    public float minSize;

	private GameObject Player;
    private PlayerManager playerManager;
    private float camRayLength = 100f;
    private int floorMask;

	void Awake() {
		floorMask = LayerMask.GetMask("Floor");
        Player = gameObject;
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
	}

	void Update() {
        if (Input.GetKeyDown("space"))
        {
            Split();
        }
        else if (Input.GetKeyDown("r"))
        {
            Destroy(Player);
            GameObject newPlayer = Instantiate(Player);
            newPlayer.GetComponent<GameController>().enabled = true;
        }
	}

	void Split() {
        lock (playerManager)
        {
            if (playerManager.count >= playerManager.maxCount || Player.GetComponent<GameController>().size / 2 < minSize) return;

            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit floorHit;
            if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
            {
                Vector3 playerToMouse = floorHit.point - transform.position;
                playerToMouse.y = 0;
                playerToMouse.Normalize();

                Player.GetComponent<GameController>().size /= 2;

                GameObject newPlayer = Instantiate(Player);
                newPlayer.transform.SetParent(Player.transform.parent);
                newPlayer.GetComponent<GameController>().movementTimer = initialMovementTimer;
                newPlayer.GetComponent<GameController>().colliderTimer = initialColliderTimer;
                newPlayer.GetComponent<GameController>().number = playerManager.GetComponent<PlayerManager>().count++;
                newPlayer.GetComponent<Rigidbody>().velocity = playerToMouse * initialVelocity;
                newPlayer.GetComponent<GameController>().enabled = true;

                Player.GetComponent<GameController>().needUpdate = true;
                newPlayer.GetComponent<GameController>().needUpdate = true;
            }
        }
	}
}
