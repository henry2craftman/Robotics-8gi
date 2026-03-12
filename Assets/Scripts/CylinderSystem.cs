using System.Collections;
using UnityEngine;

public class CylinderSystem : MonoBehaviour
{
    public CylinderSensor sensor;
    public Transform pusher;
    public Transform target;
    public float pusherSpeed = 0.5f;
    bool isForward = true;
    Vector3 originPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originPos = pusher.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(sensor.isDetected)
        {
            // 푸셔 이동...
            sensor.isDetected = false;

            if(isForward)
            {
                StartCoroutine(MovePusherTo(target.position));
            }
            else
            {
                StartCoroutine(MovePusherTo(originPos));
            }

        }
    }

    IEnumerator MovePusherTo(Vector3 to)
    {
        while(true)
        {
            Vector3 dir = to - pusher.position;
            float distance = dir.magnitude;

            if(distance < 0.01f)
            {
                isForward = !isForward;
                break;
            }

            pusher.position += dir * pusherSpeed * Time.deltaTime;

            yield return null;
        }
    }
}
