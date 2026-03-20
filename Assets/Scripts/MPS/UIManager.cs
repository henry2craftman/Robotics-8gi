using UnityEngine;
using UnityEngine.UI;

namespace MPS 
{ 
    public class UIManager : MonoBehaviour
    {
        public TowerLamp towerLamp;
        public void OnCylinderBtnClkEvent(Cylinder cyl)
        {
            if(cyl.solenoidType == Cylinder.SolenoidType.단동형)
            {
                cyl.frontSignal_SOL = !cyl.frontSignal_SOL;
            }
            else
            {
                cyl.frontSignal_SOL = !cyl.frontSignal_SOL;
                cyl.backSignal_SOL = !cyl.backSignal_SOL;
            }
        }

        public void OnLampBtnClkEvent(string color)
        {
            switch(color)
            {
                case "red":
                    towerLamp.redLampSignal = !towerLamp.redLampSignal;
                    break;
                case "yellow":
                    towerLamp.yellowLampSignal = !towerLamp.yellowLampSignal;
                    break;
                case "green":
                    towerLamp.greenLampSignal = !towerLamp.greenLampSignal;
                    break;
            }
        }

        public void OnCWBtnClkEvent(Conveyor conv)
        {
            conv.cWSignal = !conv.cWSignal;
        }

        public void OnCCWBtnClkEvent(Conveyor conv)
        {
            conv.cCWSignal = !conv.cCWSignal;
        }

        public void OnLoaderBtnClkEvent(Loader loader)
        {
            loader.loadSignal = !loader.loadSignal;
        }

        public void OnStartBtnClkEvent(MxComponent mxComponent)
        {
            mxComponent.isStartBtnActive = !mxComponent.isStartBtnActive;
            mxComponent.isStopBtnActive = false;
        }
        public void OnStopBtnClkEvent(MxComponent mxComponent)
        {
            mxComponent.isStopBtnActive = !mxComponent.isStopBtnActive;
            mxComponent.isStartBtnActive = false;
        }
        public void OnEStopBtnClkEvent(MxComponent mxComponent)
        {
            mxComponent.isEStopBtnActive = !mxComponent.isEStopBtnActive;
            mxComponent.isStartBtnActive = false;
            mxComponent.isStopBtnActive = false;
        }
    }
}