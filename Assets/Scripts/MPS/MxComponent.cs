using UnityEngine;
using ActUtlType64Lib;
using System.Collections;
using System.Collections.Generic;
using System;
using Sensor = MPS.Sensor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
/// PLC의 디바이스 커멘트(디바이스맵) 기반의 신호를 확인 후 디지털 트윈에 연동
/// 0. Start Button: 설비의 초록색 시작신호, 1개의 입력신호
///    Stop Button: 설비의 빨간색 중지신호, 1개의 입력신호
///    E-Stop Button: 긴급 중지신호, 1개의 입력신호
/// 1. Connect 버튼을 클릭하여 기기와 연결: -> Open
/// 2. Disconnect 버튼을 클릭하여 기기와 연결 해제:  -> Close
/// 3. PLC의 입력, 출력 디바이스를 실시간으로 업데이트 -> 디지털 트윈과 실제 기기를 연동하기 위해
/// 3-1. 공급, 가공, 송출, 배출 각 4개, 총 15개의 입출력들(가공 실린더는 3개)
/// 3-2. Loader: 1개의 출력신호
/// 3-3. TowerLamp: 3개의 출력신호
/// 3-4. Sensors: 3개의 입력신호(Loader Sensor, 근접센서, 금속센서 각 1개)
/// 3-5. Conveyor: 2개의 출력신호(CW, CCW)
/// 
// 입력 디바이스(14개 + 1개)
// X0    START BTN
// X1	 STOP BTN
// X2	 E-STOP BTN
// X10	 LS0 - 공급후방
// X11	 LS1 - 공급전방
// X12	 LS2 - 가공후방
// X13	 LS3 - 가공전방
// X14	 LS4 - 송출후방
// X15	 LS5 - 송출전방
// X16	 LS6 - 배출후방
// X17	 LS7 - 배출전방
// X20	 LOADER SENSOR
// X21	 PROX SENSOR
// X22	 METAL SENSOR
// X23   UR16e E-STOP   // 입력인 이유: ROBOT 컨트롤러의 긴급정지 -> PLC에도 영향
// 
// 출력 디바이스(12개 + 3개)
// Y0   SOL0 - 공급후진
// Y1	SOL1 - 공급전진
// Y2	SOL2 - 가공전후진
// Y3	SOL3 - 송출후진
// Y4	SOL4 - 송출전진
// Y5	SOL5 - 배출후진
// Y6	SOL6 - 배출전진   
// Y10	RED LAMP
// Y11	YELLOW LAMP
// Y12	GREEN LAMP
// Y13	CONV CW
// Y14	CONV CCW
// Y15	LOADER
// Y20  UR16e START
// Y21  UR16e CYCLE
// Y22  UR16e STOP
/// </summary>

namespace MPS
{
    public class MxComponent : MonoBehaviour
    {
        Stopwatch stopwatch = new Stopwatch();
        CancellationTokenSource cts;

        [Header("PLC 데이터 관련")]
        ActUtlType64 mxComponent;
        public bool isConnected;
        public float updateInterval = 1f; // 단위: 초
        public int xDeviceBlockNum = 3;
        public int yDeviceBlockNum = 3;

        // --- [FIX 4] race condition 방지를 위한 lock 객체 및 버퍼 분리 ---
        private readonly object dataLock = new object();
        private int[] plcXData = new int[3];
        private bool[,] plcYData = new bool[3, 16];

        [Header("출력 장비 리스트")]
        public Cylinder cylinder1; // 공급(양솔)
        public Cylinder cylinder2; // 가공(단솔)
        public Cylinder cylinder3; // 송출(양솔)
        public Cylinder cylinder4; // 배출(양솔)
        public TowerLamp towerLamp;
        public Conveyor conveyor;
        public Loader loader;
        public RobotController robot1;

        [Header("입력 장비 리스트")]
        public bool isStartBtnActive;
        public bool isStopBtnActive;
        public bool isEStopBtnActive;
        public Sensor loaderSensor;
        public Sensor proxSensor;
        public Sensor metalSensor;

        // --- [FIX 2] 현재 활성 연결 방식을 추적하는 열거형 ---
        private enum ConnectionMode { None, Main, Async }
        private ConnectionMode currentConnectionMode = ConnectionMode.None;

        // 객체 초기화 용도
        private void Awake()
        {
            mxComponent = new ActUtlType64();
            mxComponent.ActLogicalStationNumber = 0;

            plcXData = new int[xDeviceBlockNum];
            plcYData = new bool[yDeviceBlockNum, 16];

            cts = new CancellationTokenSource();
        }

        private void Update()
        {
            if (isConnected)
            {
                ApplyYData();
                ApplyXData();
            }
        }

        private void ApplyXData()
        {
            // --- [FIX 5] PLC 디바이스 주소 기준: X0=bit0(START), X1=bit1(STOP), X2=bit2(ESTOP) ---
            // LSB(오른쪽)가 낮은 주소이므로 bit0=START, bit1=STOP, bit2=ESTOP 순으로 구성
            int btnBlock = (isStartBtnActive ? (1 << 0) : 0)    // X00
                         | (isStopBtnActive  ? (1 << 1) : 0)    // X01
                         | (isEStopBtnActive ? (1 << 2) : 0);   // X02

            int lsBlock = (cylinder1.backSignal_LS  ? (1 << 0) : 0)  // X10
                        | (cylinder1.frontSignal_LS ? (1 << 1) : 0)  // X11
                        | (cylinder2.backSignal_LS  ? (1 << 2) : 0)  // X12
                        | (cylinder2.frontSignal_LS ? (1 << 3) : 0)  // X13
                        | (cylinder3.backSignal_LS  ? (1 << 4) : 0)  // X14
                        | (cylinder3.frontSignal_LS ? (1 << 5) : 0)  // X15
                        | (cylinder4.backSignal_LS  ? (1 << 6) : 0)  // X16
                        | (cylinder4.frontSignal_LS ? (1 << 7) : 0); // X17

            int sensorBlock = (loaderSensor.sensorSignal ? (1 << 0) : 0)  // X20
                            | (proxSensor.sensorSignal   ? (1 << 1) : 0)  // X21
                            | (metalSensor.sensorSignal  ? (1 << 2) : 0); // X22

            if (robot1 != null)
            {
                sensorBlock = (loaderSensor.sensorSignal ? (1 << 0) : 0)  // X20
                            | (proxSensor.sensorSignal ? (1 << 1) : 0)    // X21
                            | (metalSensor.sensorSignal ? (1 << 2) : 0)   // X22
                            | (robot1.eStopSignal ? (1 << 3) : 0);        // X23
            }

            // --- [FIX 4] lock으로 보호 ---
            lock (dataLock)
            {
                plcXData[0] = btnBlock;
                plcXData[1] = lsBlock;
                plcXData[2] = sensorBlock;
            }
        }

        private void ApplyYData()
        {
            // --- [FIX 4] lock으로 보호 ---
            bool[,] snapshot;
            lock (dataLock)
            {
                snapshot = (bool[,])plcYData.Clone();
            }

            cylinder1.backSignal_SOL   = snapshot[0, 0]; // Y00
            cylinder1.frontSignal_SOL  = snapshot[0, 1]; // Y01
            cylinder2.frontSignal_SOL  = snapshot[0, 2]; // Y02 : 가공실린더(단솔)
            cylinder3.backSignal_SOL   = snapshot[0, 3]; // Y03
            cylinder3.frontSignal_SOL  = snapshot[0, 4]; // Y04
            cylinder4.backSignal_SOL   = snapshot[0, 5]; // Y05
            cylinder4.frontSignal_SOL  = snapshot[0, 6]; // Y06

            towerLamp.redLampSignal    = snapshot[1, 0]; // Y10
            towerLamp.yellowLampSignal = snapshot[1, 1]; // Y11
            towerLamp.greenLampSignal  = snapshot[1, 2]; // Y12
            conveyor.cWSignal          = snapshot[1, 3]; // Y13
            conveyor.cCWSignal         = snapshot[1, 4]; // Y14
            loader.loadSignal          = snapshot[1, 5]; // Y15

            if(robot1 != null)
            {
                robot1.startSignal         = snapshot[2, 0]; // Y20
                robot1.cycleSignal         = snapshot[2, 1]; // Y21
                robot1.stopSignal          = snapshot[2, 2]; // Y22
            }
        }

        public void Open()
        {
            int iRet = mxComponent.Open();

            if (iRet == 0)
            {
                isConnected = true;
                currentConnectionMode = ConnectionMode.Main; // [FIX 2]

                StartCoroutine(UpdatePLCData());

                Debug.Log("기기가 성공적으로 연결되었습니다!");
            }
            else
            {
                Debug.LogWarning("연결이 실패하였습니다. " + iRet.ToString("X"));
            }
        }

        ActUtlType64 mxComponentAsync;
        public void OpenByNewThread()
        {
            Task.Run(() =>
            {
                mxComponentAsync = new ActUtlType64();

                int iRet = mxComponentAsync.Open();

                if (iRet == 0)
                {
                    isConnected = true;
                    currentConnectionMode = ConnectionMode.Async; // [FIX 2]

                    Debug.Log("기기가 성공적으로 연결되었습니다!");

                    UpdatePLCData(mxComponentAsync, cts);
                }
                else
                {
                    Debug.LogWarning("연결이 실패하였습니다. " + iRet.ToString("X"));
                }
            });
        }

        // --- [FIX 2, 3] 활성 연결 모드에 따라 올바른 객체를 닫도록 수정 ---
        public void Close()
        {
            ActUtlType64 target = currentConnectionMode == ConnectionMode.Async
                                  ? mxComponentAsync
                                  : mxComponent;

            if (target == null)
            {
                Debug.LogWarning("닫을 연결이 없습니다.");
                return;
            }

            int iRet = target.Close();

            if (iRet == 0)
            {
                isConnected = false;
                currentConnectionMode = ConnectionMode.None;
                Debug.Log("기기가 성공적으로 연결해지 되었습니다!");
            }
            else
            {
                Debug.LogWarning("연결해지가 실패하였습니다. " + iRet.ToString("X"));
            }
        }

        // --- [FIX 3] Async 연결 해제 시 isConnected만 내리면 UpdatePLCData 루프 종료 후 Close 호출됨 ---
        public void CloseByNewThread()
        {
            isConnected = false;
            // UpdatePLCData(ActUtlType64, CancellationTokenSource) 루프가 종료되면서 내부에서 Close() 호출
        }

        IEnumerator UpdatePLCData()
        {
            yield return new WaitForEndOfFrame();

            while (isConnected)
            {
                ReadDeviceBlock("Y0", yDeviceBlockNum);
                WriteDeviceBlock("X0", xDeviceBlockNum, ref plcXData);

                yield return new WaitForSeconds(updateInterval);
            }
        }

        // STA(Single-Threaded Apartment): MxComponent 객체는 생성한 스레드에서만 사용
        async Task UpdatePLCData(ActUtlType64 mxComponent, CancellationTokenSource cts)
        {
            while (isConnected)
            {
                try
                {
                    ReadDeviceBlock(mxComponent, "Y0", yDeviceBlockNum);

                    int[] xSnapshot;
                    lock (dataLock)
                    {
                        xSnapshot = (int[])plcXData.Clone();
                    }
                    WriteDeviceBlock(mxComponent, "X0", xDeviceBlockNum, ref xSnapshot);

                    // --- [FIX 6] updateInterval은 초 단위이므로 ms로 변환 ---
                    // --- [FIX 7] CancellationToken 전달 ---
                    await Task.Delay((int)(updateInterval * 1000), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("PLC 업데이트 루프가 취소되었습니다.");
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            // --- [FIX 3] Async 모드 종료 시 실제 Close 호출 ---
            int iRet = mxComponent.Close();

            if (iRet == 0)
            {
                currentConnectionMode = ConnectionMode.None;
                Debug.Log("기기가 성공적으로 연결해지 되었습니다!");
            }
            else
            {
                Debug.LogWarning("연결해지가 실패하였습니다. " + iRet.ToString("X"));
            }
        }

        // PLC -> PC
        public void ReadDeviceBlock(string startDevice, int blockNum)
        {
            int[] data = new int[blockNum];

            int iRet = mxComponent.ReadDeviceBlock(startDevice, blockNum, out data[0]);

            if (iRet == 0)
            {
                ConvertYData(data);
            }
            else
            {
                Debug.LogWarning("ERROR: " + iRet.ToString("X"));
            }
        }

        public void ReadDeviceBlock(ActUtlType64 mxComponent, string startDevice, int blockNum)
        {
            int[] data = new int[blockNum];

            int iRet = mxComponent.ReadDeviceBlock(startDevice, blockNum, out data[0]);

            if (iRet == 0)
            {
                ConvertYData(data);
            }
            else
            {
                Debug.LogWarning("ERROR: " + iRet.ToString("X"));
            }
        }

        private void ConvertYData(int[] data)
        {
            // --- [FIX 1] 불필요한 bool[] block 제거, 직접 plcYData에 기록 ---
            // --- [FIX 4] lock으로 보호 ---
            lock (dataLock)
            {
                for (int j = 0; j < data.Length; j++)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        plcYData[j, i] = (data[j] & (1 << i)) != 0;
                    }
                }
            }
        }

        // PC -> PLC
        public void WriteDeviceBlock(string startDevice, int blockNum, ref int[] data)
        {
            mxComponent.WriteDeviceBlock(startDevice, blockNum, ref data[0]);
        }

        public void WriteDeviceBlock(ActUtlType64 mxComponent, string startDevice, int blockNum, ref int[] data)
        {
            mxComponent.WriteDeviceBlock(startDevice, blockNum, ref data[0]);
        }

        private void OnDestroy()
        {
            isConnected = false;

            cts.Cancel();
            cts.Dispose();
        }
    }
}