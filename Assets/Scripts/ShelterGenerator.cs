using UnityEngine;
using System.Collections;

public class ShelterGenerator : MonoBehaviour {

    public GameObject Shelter;
    public GameObject Shelters;
	public float spawnTime = 3f;
	public int FloorScale;
	public int MaxCount = 5;
	public int count;

	void Start ()
	{
		count = 0;
		InvokeRepeating ("Spawn", spawnTime, spawnTime);
	}
	
	
	void Spawn ()
	{
		if (count < MaxCount) {
			int spawnPointX = Random.Range (- FloorScale / 3 + 3, FloorScale / 3 - 3), spawnPointZ = Random.Range (- FloorScale / 3 + 3, FloorScale / 3 - 3);
            GameObject newShelter = Instantiate(Shelter, new Vector3(spawnPointX, 0, spawnPointZ), new Quaternion(0, 0, 0, 0)) as GameObject;
            newShelter.transform.SetParent(Shelters.transform);
			count ++;
		}
	}
}
