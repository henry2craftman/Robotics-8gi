using UnityEngine;

/// <summary>
/// Object를 Target 방향으로 이동시킨다.
/// 속성: Object의 transform, Target의 transform, 속도
/// </summary>
public class SequenceManager : MonoBehaviour
{
    public Transform obj;
    public Transform targetA;
    public Transform[] targets; // 여러 타겟들을 담는 배열
    public float speed;
    Vector3 originPos;
    public bool isTargetA = true;
    int option = 0; // target들의 옵션
    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originPos = obj.position;

        animator = obj.GetComponent<Animator>(); // 내 GameObject의 컴포넌트 가져오기

        animator.SetInteger("AnimationID", 4);
    }

    // Update is called once per frame
    void Update()
    {
        //if (isTargetA)
        //    MoveObjectToTarget(targetA.position);
        //else
        //    MoveObjectToTarget(originPos);

        MoveObjectToTarget(targets[option].position);
    }

    private void MoveObjectToTarget(Vector3 targetPos)
    {
        // Object를 Target 방향으로 이동시킨다.
        // 벡터(vector): 방향과 크기를 가진 값(ex_속도)
        // 스칼라(Scalar): 크기만 가진 값(ex_속력)
        Vector3 dir = targetPos - obj.position;

        Vector3 xyDir = new Vector3(dir.x, 0, dir.z);
        float distance = xyDir.magnitude; // 거리: 벡터의 크기

        if (distance < 0.1f)
        {
            //isTargetA = false;
            isTargetA = !isTargetA; // Toggle: bool은 toogle 가능 true <-> false

            option++;
            if (option >= targets.Length)
            {
                option = targets.Length - 1; // 마지막 인덱스로 고정

                // animator.SetInteger("AnimationID", 10); // 바로 앉기
                Invoke("StartAnimation", 2.0f);
                //InvokeRepeating("CreateBox", 3, 1); // 3초 후 1초마다 함수를 반복
            }

            return;
        }

        obj.forward = xyDir.normalized; // 진행 방향의 크기가 1인 벡터를 앞으로 바꾸기

        obj.position += xyDir.normalized * speed * Time.deltaTime;
    }

    void StartAnimation()
    {
        animator.SetInteger("AnimationID", 7);
    }

    void CreateBox()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.AddComponent<Rigidbody>();
    }
}
