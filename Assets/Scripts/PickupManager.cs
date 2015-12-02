using UnityEngine;
using System.Collections;

public class PickupManager : MonoBehaviour
{
	public GameObject Pickup;
    public GameObject Pickups;
	public float spawnTime = 3f;
	public int FloorScale;
	public int MaxCount = 15;
	public int count;	

	void Start ()
	{
		count = 0;
		InvokeRepeating ("Spawn", spawnTime, spawnTime);
	}
	
	
	void Spawn ()
	{
		if (count < MaxCount) {
			int spawnPointX = Random.Range (- FloorScale / 2 + 3, FloorScale / 2 - 3), spawnPointZ = Random.Range (- FloorScale / 2 + 3, FloorScale / 2 - 3);
            GameObject newPickup = Instantiate(Pickup, new Vector3(spawnPointX, 0, spawnPointZ), new Quaternion(0, 0, 0, 0)) as GameObject;
            newPickup.transform.SetParent(Pickups.transform);
			count ++;
		}
	}
}
