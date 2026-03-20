using System.Collections;
using UnityEngine;
using static Cylinder;

/// <summary>
/// PLC의 출력신호(ex. Y20)를 받아 솔레노이드에 신호가 들어온다.
/// 솔레노이드 신호에 따라 실린더 로드가 전진, 후진한다.
/// 속성: 후방LS의 PLC신호, 전방LS의 PLC신호, LS0과 LS1의 MeshRenderer
///       SOL0의 PLC신호, SOL1의 PLC 신호
///       실린더 Rod의 Transform, 앞방향 maxPos, 뒷방향 minPos(이동 축의 최소, 최대값)
///       실린더 이동속도(공압), 리턴스피드(단동 솔레노이드에서만 사용)
///       단동, 복동 열거형
/// </summary>
public class Cylinder : MonoBehaviour
{
    public enum SolenoidType
    {
        단동형,
        복동형
    }

    [Header("PLC 신호들")]
    public bool backSignal_LS;
    public bool frontSignal_LS;
    public bool backSignal_SOL;
    public bool frontSignal_SOL;

    [Header("기타 설정들")]
    public SolenoidType solenoidType = SolenoidType.단동형;
    public Transform rod;
    public float maxPos;
    public float minPos;
    public float speed = 2; // 공압 조절 밸브에 따른 속도
    public float returnSpeed = 3; // 단동 솔레노이드의 복귀 속도
    public MeshRenderer mrBackLS;
    public MeshRenderer mrFrontLS;
    public bool isMoving;
    public bool isBack; // Rod가 뒤에있는지, 앞에 있는지 확인


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnOnLS(true, true);

        StartCoroutine(MoveForwardBySigal());
        StartCoroutine(MoveBackwardBySigal());
    }

    void TurnOnLS(bool isBackLS, bool isOn)
    {
        switch(isBackLS)
        {
            case true: // Back
                if(isOn)
                {
                    mrBackLS.material.color = new Color(1, 0, 0, 0.7f);
                    backSignal_LS = true;
                }
                else
                {
                    mrBackLS.material.color = new Color(0, 0, 0, 0.7f);
                    backSignal_LS = false;
                }

                break;
            case false: // Front
                if (isOn)
                {
                    mrFrontLS.material.color = new Color(1, 0, 0, 0.7f);
                    frontSignal_LS = true;
                }
                else
                {
                    mrFrontLS.material.color = new Color(0, 0, 0, 0.7f);
                    frontSignal_LS = false;
                }
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 키입력으로 PLC Mock 신호 주기
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            backSignal_LS = !backSignal_LS; // 버튼을 누를 때 마다 토글
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            frontSignal_LS = !frontSignal_LS; // 버튼을 누를 때 마다 토글
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            backSignal_SOL = !backSignal_SOL; // 버튼을 누를 때 마다 토글
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            frontSignal_SOL = !frontSignal_SOL; // 버튼을 누를 때 마다 토글

            if(frontSignal_SOL)
            {
                Vector3 dir = new Vector3(rod.localPosition.x, maxPos, rod.localPosition.z);
                StartCoroutine(MoveCylinder(dir));
            }
            else
            {
                Vector3 dir = new Vector3(rod.localPosition.x, minPos, rod.localPosition.z);
                StartCoroutine(MoveCylinder(dir));
            }
        }
    }

    // PLC 신호는 Logic에 의해 계속 켜져있음 -> 
    IEnumerator MoveCylinder(Vector3 to)
    {
        if(!isMoving)
        {
            isMoving = true;

            while (true)
            {
                Vector3 dir = to - rod.localPosition;
                float distance = dir.magnitude;

                if (distance < 0.1f)
                {
                    isMoving = false;

                    if (frontSignal_SOL) // 앞쪽 끝에 왔을 때
                    {
                        TurnOnLS(false, true); // Front ON
                        TurnOnLS(true, false); // Back OFF

                        isBack = false;
                    }
                    else if(!frontSignal_SOL || (solenoidType == SolenoidType.복동형 && backSignal_SOL))
                    {
                        TurnOnLS(true, true);   // Back ON
                        TurnOnLS(false, false); // Front OFF

                        isBack = true;
                    }

                    break;
                }

                rod.localPosition += dir.normalized * speed * Time.deltaTime;

                yield return null;
            }
        }
        else
        {
            TurnOnLS(true, false);  // Back LS OFF
            TurnOnLS(false, false); // Front LS OFF
        }
    }

    IEnumerator MoveForwardBySigal()
    {
        while(true)
        {
            if(solenoidType == SolenoidType.단동형)
            {
                yield return new WaitUntil(() => frontSignal_SOL && isBack);
            }
            else
            {
                yield return new WaitUntil(() => frontSignal_SOL && isBack);
            }

            Vector3 dir = new Vector3(rod.localPosition.x, maxPos, rod.localPosition.z);
            yield return MoveCylinder(dir);
        }
    }

    IEnumerator MoveBackwardBySigal()
    {
        while (true)
        {
            if (solenoidType == SolenoidType.단동형)
            {
                yield return new WaitUntil(() => !frontSignal_SOL && !isBack);
            }
            else
            {
                yield return new WaitUntil(() => backSignal_SOL && !isBack);
            }

            Vector3 dir = new Vector3(rod.localPosition.x, minPos, rod.localPosition.z);
            yield return MoveCylinder(dir);
        }
    }
}
