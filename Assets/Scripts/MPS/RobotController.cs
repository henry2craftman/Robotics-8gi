using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 로봇의 Step 정보들을 저장, PLC의 출력신호 -> 로봇
///                            + 디지털 트윈도 동시에 로봇 시퀀스 수행
/// 속성: 로봇의 IK-toolkit, Step 정보 저장을 위한 컨테이너
/// </summary>
public class RobotController : MonoBehaviour
{
    [Serializable]
    public struct Step
    {
        public int id;
        public Vector3 position;
        public Quaternion rotation;
        public bool isSuctionOn;
        public float duration;
        public float speed;
    }

    public IK_toolkit ik_toolkit;
    public List<Step> steps = new List<Step>();
    bool isXPlusOn, isXMinusOn;
    bool isYPlusOn, isYMinusOn;
    bool isZPlusOn, isZMinusOn;
    int xPos, yPos, zPos;
    bool isXRotPlusOn, isYRotPlusOn, isZRotPlusOn;
    bool isXRotMinusOn, isYRotMinusOn, isZRotMinusOn;
    int xRot, yRot, zRot;
    public float multiplier = 0.01f;
    public float rotMultiplier = 1f;

    [Header("UI")]
    public TMP_InputField xPosInput;
    public TMP_InputField yPosInput, zPosInput;
    public TMP_InputField xRotInput, yRotInput, zRotInput;
    public TMP_InputField durationInput, SpeedInput;
    public Toggle suctionToggle;
    public EventTrigger xRotPlusET, xRotMinusET;
    public EventTrigger yRotPlusET, yRotMinusET;
    public EventTrigger zRotPlusET, zRotMinusET;
    private Vector3 originPos;
    public bool signleSignal;
    public bool cycleSignal;
    public bool stopSignal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddEventTriggerListner(EventTriggerType.PointerDown, OnXRotPlusBtnDownEvent, ref xRotPlusET);
        AddEventTriggerListner(EventTriggerType.PointerUp, OnXRotPlusBtnUpEvent, ref xRotPlusET);
        AddEventTriggerListner(EventTriggerType.PointerDown, OnYRotPlusBtnDownEvent, ref yRotPlusET);
        AddEventTriggerListner(EventTriggerType.PointerUp, OnYRotPlusBtnUpEvent, ref yRotPlusET);
        AddEventTriggerListner(EventTriggerType.PointerDown, OnZRotPlusBtnDownEvent, ref zRotPlusET);
        AddEventTriggerListner(EventTriggerType.PointerUp, OnZRotPlusBtnUpEvent, ref zRotPlusET);

        AddEventTriggerListner(EventTriggerType.PointerDown, OnXRotMinusBtnDownEvent, ref xRotMinusET);
        AddEventTriggerListner(EventTriggerType.PointerUp, OnXRotMinusBtnUpEvent, ref xRotMinusET);
        AddEventTriggerListner(EventTriggerType.PointerDown, OnYRotMinusBtnDownEvent, ref yRotMinusET);
        AddEventTriggerListner(EventTriggerType.PointerUp, OnYRotMinusBtnUpEvent, ref yRotMinusET);
        AddEventTriggerListner(EventTriggerType.PointerDown, OnZRotMinusBtnDownEvent, ref zRotMinusET);
        AddEventTriggerListner(EventTriggerType.PointerUp, OnZRotMinusBtnUpEvent, ref zRotMinusET);

        originPos = ik_toolkit.ik.transform.localPosition;
    }

    /// <summary>
    /// 버튼의 이벤트 트리거에 커스텀 메서드를 특정 이벤트 타입에 연결하는 메서드
    /// </summary>
    /// <param name="eventType">이벤트 타입</param>
    /// <param name="call">연결하고자 하는 메서드</param>
    /// <param name="trigger">연결하고자 하는 버튼의 이벤트 트리거</param>
    void AddEventTriggerListner(EventTriggerType eventType, 
        System.Action<PointerEventData> call, ref EventTrigger trigger)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;

        entry.callback.AddListener((data) => call((PointerEventData)data));
        trigger.triggers.Add(entry);
    }

    // Update is called once per frame
    void Update()
    {
        if (signleSignal && !isMoving)
        {
            StartCoroutine(CoStartSequence());
        }
        else if (cycleSignal && !isMoving)
        {
            StartCoroutine(CoStartSequence());
        }
        else if (stopSignal)
        {
            isMoving = false;
        }


        if (isMoving) return;

        UpdatePosition();
        UpdateRotation();

        ShowPosition();
        ShowRotation();
    }

    /// <summary>
    /// UI 버튼의 Position 변경 값을 받아 로봇의 End-Effector가 움직인다.
    /// </summary>
    void UpdatePosition()
    {
        if(isXPlusOn) xPos = 1;
        else if(isXMinusOn) xPos = -1;
        else xPos = 0;

        if (isYPlusOn) yPos = 1;
        else if (isYMinusOn) yPos = -1;
        else yPos = 0;

        if (isZPlusOn) zPos = 1;
        else if (isZMinusOn) zPos = -1;
        else zPos = 0;

        ik_toolkit.ik.localPosition += new Vector3(xPos, yPos, zPos) * multiplier;
    }

    void ShowPosition()
    {
        xPosInput.text = ik_toolkit.ik.localPosition.x.ToString();
        yPosInput.text = ik_toolkit.ik.localPosition.y.ToString();
        zPosInput.text = ik_toolkit.ik.localPosition.z.ToString();
    }

    void ShowRotation()
    {
        xRotInput.text = ik_toolkit.ik.localRotation.eulerAngles.x.ToString();
        yRotInput.text = ik_toolkit.ik.localRotation.eulerAngles.y.ToString();
        zRotInput.text = ik_toolkit.ik.localRotation.eulerAngles.z.ToString();
    }

    void UpdateRotation()
    {
        if (isXRotPlusOn) xRot = 1;
        else if (isXRotMinusOn) xRot = -1;
        else xRot = 0;

        if (isYRotPlusOn) yRot = 1;
        else if (isYRotMinusOn) yRot = -1;
        else yRot = 0;

        if (isZRotPlusOn) zRot = 1;
        else if (isZRotMinusOn) zRot = -1;
        else zRot = 0;

        ik_toolkit.ik.localRotation *= Quaternion.Euler(xRot * rotMultiplier,
                                                        yRot * rotMultiplier,
                                                        zRot * rotMultiplier);
    }

    /// <summary>
    /// Event Trigger 기능을 사용하여 버튼이 누른 순간을 확인
    /// </summary>
    public void OnXPlusBtnDownEvent()
    {
        isXPlusOn = true;
    }

    /// <summary>
    /// 버튼을 떼는 순간을 포착
    /// </summary>
    public void OnXPlusBtnUpEvent()
    {
        isXPlusOn = false;
    }

    public void OnYPlusBtnDownEvent()
    {
        isYPlusOn = true;
    }

    public void OnYPlusBtnUpEvent()
    {
        isYPlusOn = false;
    }

    public void OnZPlusBtnDownEvent()
    {
        isZPlusOn = true;
    }

    public void OnZPlusBtnUpEvent()
    {
        isZPlusOn = false;
    }

    /// <summary>
    /// Event Trigger 기능을 사용하여 버튼이 누른 순간을 확인
    /// </summary>
    public void OnXMinusBtnDownEvent()
    {
        isXMinusOn = true;
    }

    /// <summary>
    /// 버튼을 떼는 순간을 포착
    /// </summary>
    public void OnXMinusBtnUpEvent()
    {
        isXMinusOn = false;
    }

    public void OnYMinusBtnDownEvent()
    {
        isYMinusOn = true;
    }

    public void OnYMinusBtnUpEvent()
    {
        isYMinusOn = false;
    }

    public void OnZMinusBtnDownEvent()
    {
        isZMinusOn = true;
    }

    public void OnZMinusBtnUpEvent()
    {
        isZMinusOn = false;
    }

    public void OnXRotPlusBtnDownEvent(PointerEventData data)
    {
        isXRotPlusOn = true;
    }

    public void OnXRotPlusBtnUpEvent(PointerEventData data)
    {
        isXRotPlusOn = false;
    }
    public void OnYRotPlusBtnDownEvent(PointerEventData data)
    {
        isYRotPlusOn = true;
    }

    public void OnYRotPlusBtnUpEvent(PointerEventData data)
    {
        isYRotPlusOn = false;
    }
    public void OnZRotPlusBtnDownEvent(PointerEventData data)
    {
        isZRotPlusOn = true;
    }

    public void OnZRotPlusBtnUpEvent(PointerEventData data)
    {
        isZRotPlusOn = false;
    }

    public void OnXRotMinusBtnDownEvent(PointerEventData data)
    {
        isXRotMinusOn = true;
    }

    public void OnXRotMinusBtnUpEvent(PointerEventData data)
    {
        isXRotMinusOn = false;
    }
    public void OnYRotMinusBtnDownEvent(PointerEventData data)
    {
        isYRotMinusOn = true;
    }

    public void OnYRotMinusBtnUpEvent(PointerEventData data)
    {
        isYRotMinusOn = false;
    }
    public void OnZRotMinusBtnDownEvent(PointerEventData data)
    {
        isZRotMinusOn = true;
    }

    public void OnZRotMinusBtnUpEvent(PointerEventData data)
    {
        isZRotMinusOn = false;
    }

    int stepCnt;
    private bool isMoving;

    /// <summary>
    /// 버튼을 누르면 현재 로봇의 정보가 Step으로 저장된다.
    /// </summary>
    public void OnTeachBtnClkEvent()
    {
        Step step = new Step();
        step.position = ik_toolkit.ik.localPosition;
        step.rotation = ik_toolkit.ik.localRotation;
        bool isParsed = float.TryParse(durationInput.text, out step.duration);

        if(!isParsed)
        {
            Debug.LogAssertion("Duration은 양의 정수 또는 실수형으로 입력 후 다시 시도해 주세요.");
            return;
        }

        step.isSuctionOn = suctionToggle.isOn;

        isParsed = float.TryParse(SpeedInput.text, out step.speed);

        if (!isParsed)
        {
            Debug.LogAssertion("Speed는 양의 정수 또는 실수형으로 입력 후 다시 시도해 주세요.");
            return;
        }

        Debug.Log($"{stepCnt}번째 Step이 성공적으로 저장되었습니다.");
        
        step.id = stepCnt++;

        steps.Add(step);
    }

    /// <summary>
    /// 버튼을 누르면, Step 리스트가 초기화 된다.
    /// </summary>
    public void OnDeleteBtnClkEvent()
    {
        steps.Clear();
    }

    /// <summary>
    /// 버튼을 누르면, Step 리스트를 순회하며 로봇이 1번 운전한다.
    /// </summary>
    public void OnStartBtnClkEvent()
    {
        // 초기 위치 -> step 0
        // step 0: 앞쪽 이동

        // step 0 -> step 1
        // Coroutine함수 사용 -> Vector3.Lerp(A, B, t)
        // yield return new waitForSeconds(duration - t);

        isMoving = true;

        StartCoroutine(CoStartSequence());
    }

    /// <summary>
    /// steps 리스트를 순회하며, 로봇을 움직인다.
    /// </summary>
    /// <returns></returns>
    IEnumerator CoStartSequence()
    {
        // 1. 오리진으로 복귀
        Vector3 curPos = ik_toolkit.ik.transform.localPosition;
        yield return CoMove(curPos, originPos, 1);

        // 2. 첫 포지션으로 이동
        yield return CoMove(originPos, steps[0].position, steps[0].speed);

        yield return new WaitForSeconds(steps[0].duration - steps[0].speed);

        // 3. steps리스트를 순회하며 position이동
        for (int i = 0; i < steps.Count; i++)
        {
            if ((i + 1) == steps.Count)
                break;

            yield return CoMove(steps[i].position, steps[i + 1].position,
                steps[i].speed);

            yield return new WaitForSeconds(steps[0].duration - steps[0].speed);
        }

        isMoving = false;
    }

    IEnumerator CoMove(Vector3 from, Vector3 to, float t)
    {
        float curTime = 0;

        while (true)
        {
            curTime += Time.deltaTime;

            if (curTime > t)
                break;

            ik_toolkit.ik.localPosition = Vector3.Lerp(from, to, curTime / t);

            yield return null;
        }
    }
    //IEnumerator MoveStep()

    //{
    //    Step temp = steps[0];
    //    temp.position = Vector3.zero;
    //    steps[0] = temp; // 통째로 갈아 끼워야함

    //    // 데이터가 적고 참조가 빈번하면 struct 사용  -> 그렇지 않으면 class 사용(참조타입)
    //}

    /// <summary>
    /// 버튼을 누르면,  Step 리스트를 순회하며 로봇이 계속 반복 운전한다.
    /// </summary>
    public void OnCycleBtnClkEvent()
    {

    }

    public void OnStopBtnClkEvent()
    {
        isMoving = false;
    }
}
