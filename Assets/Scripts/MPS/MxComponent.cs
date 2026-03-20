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
// 입력 디바이스(14개)
// X0    START BTN
// X1	 STOP BTN
// X2	 E-STOP BTN
// X10	 LS0
// X11	 LS1
// X12	 LS2
// X13	 LS3
// X14	 LS4
// X15	 LS5
// X16	 LS6
// X17	 LS7
// X20	 LOADER SENSOR
// X21	 PROX SENSOR
// X22	 METAL SENSOR
// 
// 출력 디바이스(12개)
// Y0   SOL0 - 공급전진
// Y1	SOL1 - 공급후진
// Y2	SOL2 - 가공전후진
// Y3	SOL3 - 송출전진
// Y4	SOL4 - 송출후진
// Y5	SOL5 - 배출전진
// Y6	SOL6 - 배출후진  
// Y10	RED LAMP
// Y11	YELLOW LAMP
// Y12	GREEN LAMP
// Y13	CONV CW
// Y14	CONV CCW
// Y15	LOADER     
/// </summary>
/// 
namespace MPS
{
    public class MxComponent : MonoBehaviour
    {
        Stopwatch stopwatch = new Stopwatch();
        CancellationTokenSource cts;

        [Header("PLC 데이터 관련")]
        ActUtlType64 mxComponent;
        public bool isConnected;
        public float updateInterval = 1;
        public int xDeviceBlockNum = 3;
        public int yDeviceBlockNum = 2;
        private int[] plcXData = new int[3];
        bool[,] plcYData = new bool[2, 16];

        [Header("출력 가상장비 리스트")]
        public Cylinder cylinder1; // 공급(양솔)
        public Cylinder cylinder2; // 가공(단솔)
        public Cylinder cylinder3; // 송출(양솔)
        public Cylinder cylinder4; // 배출(양솔)
        public TowerLamp towerLamp;
        public Conveyor conveyor;
        public Loader loader;

        [Header("입력 가상장비 리스트")]
        public bool isStartBtnActive;
        public bool isStopBtnActive;
        public bool isEStopBtnActive;
        public Sensor loaderSensor;
        public Sensor proxSensor;
        public Sensor metalSensor;

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
            if(isConnected)
            {
                //stopwatch.Reset();
                //stopwatch.Start();

                ApplyYData();

                ApplyXData();

                //stopwatch.Stop();
                //Debug.Log($"[Apply Data] {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        private void ApplyXData()
        {
            // { 5, 126, 5 }
            //                  true -> 1 / false -> 0
            string btnStr = $"{(isEStopBtnActive == true ? 1 : 0)}{(isStopBtnActive == true ? 1 : 0)}" +
                            $"{(isStartBtnActive == true ? 1 : 0)}";
            int btnBlock = Convert.ToInt32(btnStr, 2); // "101" -> 5
            plcXData[0] = btnBlock; // 5

            btnStr = $"{(cylinder4.frontSignal_LS == true ? 1 : 0)}{(cylinder4.backSignal_LS == true ? 1 : 0)}" +
                     $"{(cylinder3.frontSignal_LS == true ? 1 : 0)}{(cylinder3.backSignal_LS == true ? 1 : 0)}" +
                     $"{(cylinder2.frontSignal_LS == true ? 1 : 0)}{(cylinder2.backSignal_LS == true ? 1 : 0)}" +
                     $"{(cylinder1.frontSignal_LS == true ? 1 : 0)}{(cylinder1.backSignal_LS == true ? 1 : 0)}";
            btnBlock = Convert.ToInt32(btnStr, 2);
            plcXData[1] = btnBlock; // 126

            btnStr = $"{(metalSensor.sensorSignal == true ? 1 : 0)}{(proxSensor.sensorSignal == true ? 1 : 0)}" +
                     $"{(loaderSensor.sensorSignal == true ? 1 : 0)}";
            btnBlock = Convert.ToInt32(btnStr, 2);
            plcXData[2] = btnBlock; // 5
        }

        private void ApplyYData()
        {
            cylinder1.backSignal_SOL  = plcYData[0,0]; // Y00
            cylinder1.frontSignal_SOL = plcYData[0,1]; // Y01
            cylinder2.frontSignal_SOL = plcYData[0,2]; // Y02 : 가공실린더(단솔 = 신호하나)
            cylinder3.backSignal_SOL  = plcYData[0,3]; // Y03
            cylinder3.frontSignal_SOL = plcYData[0,4]; // Y04
            cylinder4.backSignal_SOL  = plcYData[0,5]; // Y05
            cylinder4.frontSignal_SOL = plcYData[0,6]; // Y06

            towerLamp.redLampSignal   = plcYData[1,0]; // Y10
            towerLamp.yellowLampSignal= plcYData[1,1]; // Y11
            towerLamp.greenLampSignal = plcYData[1,2]; // Y12
            conveyor.cWSignal         = plcYData[1,3]; // Y13
            conveyor.cCWSignal        = plcYData[1,4]; // Y14
            loader.loadSignal         = plcYData[1,5]; // Y15
        }

        public void Open()
        {
            int iRet = mxComponent.Open();

            if (iRet == 0)
            {
                isConnected = true;

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

                if(iRet == 0)
                {
                    isConnected = true;

                    UpdatePLCData(mxComponentAsync, cts);
                }
                else
                {
                    Debug.LogWarning("연결이 실패하였습니다. " + iRet.ToString("X"));
                }
            });
        }

        public void Close()
        {
            int iRet = mxComponent.Close();

            if (iRet == 0)
            {
                isConnected = false;

                Debug.Log("기기가 성공적으로 연결해지 되었습니다!");
            }
            else
            {
                Debug.LogWarning("연결해지가 실패하였습니다. " + iRet.ToString("X"));
            }
        }

        public void CloseByNewThread()
        {
            isConnected = false;
        }

        IEnumerator UpdatePLCData()
        {
            yield return new WaitForEndOfFrame();

            while(isConnected)
            {
                //stopwatch.Reset();
                //stopwatch.Start();
                ReadDeviceBlock("Y0", 2);
                //stopwatch.Stop();
                //Debug.Log($"[Read Device] {stopwatch.ElapsedMilliseconds}ms");

                //stopwatch.Reset();
                //stopwatch.Start();
                WriteDeviceBlock("X0", 3, ref plcXData);
                //stopwatch.Stop();
                //Debug.Log($"[Write Device] {stopwatch.ElapsedMilliseconds}ms");

                yield return new WaitForSeconds(updateInterval);
            }
        }

        // 단일 스레드 원칙(STA, Single-Threaded Apartment)
        // 객체지향언어 스레드 사용시, 객체를 만들때는 각 스레드에서 만들어야 한다.
        // MxComponent 객체 생성안됨... ->  STA(Single-Threaded Apartment) -> 각 스레드에서 객체를 따로 관리할 수 있도록
        async Task UpdatePLCData(ActUtlType64 mxComponent, CancellationTokenSource cts)
        {
            while (isConnected)
            {
                try
                {
                    ReadDeviceBlock(mxComponent, "Y0", 2);

                    WriteDeviceBlock(mxComponent, "X0", 3, ref plcXData);

                    await Task.Delay((int)updateInterval);
                }
                catch(Exception e)
                {
                    Debug.Log(e);
                }
            }

            int iRet = mxComponent.Close();

            if (iRet == 0)
            {
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

            if(iRet == 0)
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
            // { 85, 47 } -> 85를 2진수로 0000 0000 0101 0101
            int j = 0;
            foreach (int d in data)
            {
                bool[] block = new bool[16];

                for (int i = 0; i < block.Length; i++)
                {                   // 비트연산 + 조건문
                    bool isBitSet = ((d & (1 << i)) != 0); // 비트 비교 연산

                    plcYData[j, i] = isBitSet;
                }

                j++;
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
            CloseByNewThread();

            cts.Cancel();
            cts.Dispose();
        }
    }
}