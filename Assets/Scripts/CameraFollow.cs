using UnityEngine;
using System.Collections;
using System.Linq;

public class CameraFollow : MonoBehaviour {

    public Transform Target;
    public float Distance = 100f;
    public float x = 0f;
    public float y = 30f;
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    public float DistanceSpeed = 20.0f;
    public float yMinLimit = -20.0f;
    public float yMaxLimit = 80.0f;
    public float DistanceMinLimit = 4f;
    public float DistanceMaxLimit = 100f;

    void Awake()
    {
        FindTarget();
    }

    void LateUpdate()
    {
        FindTarget();
        if (Target != null)
        {
            x += (float)(Input.GetAxis("Horizontal") * xSpeed * 0.02f);
            y -= (float)(-Input.GetAxis("Vertical") * ySpeed * 0.02f);
            Distance += (float)(-Input.GetAxis("Mouse ScrollWheel") * DistanceSpeed);

            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
            Distance = Mathf.Clamp(Distance, DistanceMinLimit, DistanceMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * (new Vector3(0.0f, 0.0f, -Distance)) + Target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    void FindTarget()
    {
        var ballManagers = transform.parent.GetComponentsInChildren<BallManager>();
        var followBall = ballManagers.Where((ballManager) => ballManagers.All((other) => ballManager.number <= other.number)).First();
        Target = followBall.transform;
    }
}
