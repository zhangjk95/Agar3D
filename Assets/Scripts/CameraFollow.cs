using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    /*public Transform target;
    public float smoothing = 5;

    Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void FixedUpdate()
    {
        Vector3 targetCamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    }*/

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
    }

    void LateUpdate()
    {
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
}
