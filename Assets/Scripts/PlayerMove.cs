using UnityEngine;

// 라이프 사이클(life cycle) 메서드: 스크립트를 가지고 있는 게임오브젝트가 객체화
// 되었을 때, 순차적으로 실행되는 메서드들

/// <summary>
/// 사용자 키보드입력을 받아, 플레이어를 앞뒤좌우 이동시킨다.
/// 속성: 스피드
/// </summary>
public class PlayerMove : MonoBehaviour
{
    public float speed = 2f;

    // 시작시 한번만 실행
    void Start()
    {
        print("시작!");
    }

    // 한 프레임에 한번씩 실행(계속 반복)
    void Update()
    {
        // 사용자 키보드입력을 받아, 플레이어를 앞뒤좌우 이동시킨다.
        if(Input.GetKey(KeyCode.W))
        {
            // Vector3 dir = Vector3.forward; // 월드좌표(절대좌표) 기준의 앞방향
            Vector3 dir = transform.forward;  // 로컬좌표 기준의 앞방향

            transform.position = transform.position + dir * speed * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.S))
        {
            //Vector3 dir = -Vector3.forward;
            Vector3 dir = -transform.forward;

            transform.position = transform.position + dir * speed * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            // Vector3 dir = -Vector3.right;
            Vector3 dir = -transform.right;

            transform.position = transform.position + dir * speed * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.D))
        {
            //Vector3 dir = Vector3.right;
            Vector3 dir = transform.right;

            transform.position = transform.position + dir * speed * Time.deltaTime;
        }
    }
}
