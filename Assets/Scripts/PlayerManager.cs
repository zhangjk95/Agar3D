using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour {

    public string PlayerName;
    public int count = 1;
    public int maxCount = 16;
    public bool[] balls;

    void Awake()
    {
        balls = new bool[maxCount];
    }
	
	// Update is called once per frame
	void Update () {
	    
	}
}
