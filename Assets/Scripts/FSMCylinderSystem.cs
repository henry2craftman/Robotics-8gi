using UnityEngine;

/// <summary>
/// 스마트 컨베이어 벨트
/// 상태
/// Idle: 물체가 감지가 되지 않아 멈춰있는 상태
/// Running: 센서가 물체를 감지하여 컨베이어가 돌아가는 상태
/// Stop: 작업자가 수동으로 멈추거나(Space) 공정이 완료된 상태
/// Error: 벨트에 이상이 있는 상태
/// </summary>
public class FSMCylinderSystem : MonoBehaviour
{
    enum State
    {
        Idle,
        Running,
        Stop,
        Error
    }
    State state = State.Idle;
    State lastState = State.Idle;
    public CylinderSensor sensor;
    public Transform pusher;
    public Transform target;
    public float speed;
    public bool isEmmergency;
    Vector3 pusherOrigin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print("컨베이어를 작동시킵니다...");
        pusherOrigin = pusher.position;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.Idle:
                print("시스템은 Idle 상태입니다.");

                if(sensor.isDetected)
                {
                    state = State.Running;
                    lastState = State.Idle;
                }
                break;
            case State.Running:
                print("시스템은 Running 상태입니다.");

                // sensor.MovePusher();

                Vector3 dir = target.position - pusher.position;
                float distance = dir.magnitude;

                if(distance < 0.1f || isEmmergency)
                {
                    state = State.Stop;
                    lastState = State.Stop;
                }

                pusher.transform.position += dir.normalized * speed * Time.deltaTime;
                break;
            case State.Stop:
                print("시스템은 Stop 상태입니다.");

                if (!isEmmergency)
                {
                    state = lastState;
                }
                break;
            case State.Error:
                print("시스템은 Error 상태입니다.");

                break;
        }
    }
}
