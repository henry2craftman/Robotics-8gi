using Newtonsoft.Json;
using System.IO;
using UnityEngine;

/// <summary>
/// robotController의 steps데이터를 robotConfig.json에 저장하고, 불러온다.
/// 속성: robotController, configFilePath
/// </summary>
[RequireComponent(typeof(RobotController))] // 종속성을 강제로 부여
public class RobotConfigManager : MonoBehaviour
{
    RobotController robotController;
    [SerializeField] string configFilePath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        robotController = GetComponent<RobotController>();

        using (FileStream fs = new FileStream(configFilePath, FileMode.OpenOrCreate))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string json = sr.ReadToEnd();
                RobotController.RobotConfig config = JsonConvert.DeserializeObject<RobotController.RobotConfig>(json);

                if (config != null)
                {
                    robotController.steps = config.steps;
                }
                else
                {
                    Debug.LogWarning($"step 데이터가 비어있습니다. {configFilePath}파일을 확인해 주세요.");
                }
            }
        }
    }

    /// <summary>
    /// robotConfig.json -> RobotConfig 객체화
    /// 객체 -> RobotController.steps
    /// </summary>
    public void OnLoadBtnClkEvent()
    {
        using (FileStream fs = new FileStream(configFilePath, FileMode.OpenOrCreate))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string json = sr.ReadToEnd();
                RobotController.RobotConfig config = JsonConvert.DeserializeObject<RobotController.RobotConfig>(json);

                if (config != null)
                {
                    robotController.steps = config.steps;

                    Debug.Log("로봇 설정정보를 성공적으로 불러왔습니다.");
                }
                else
                {
                    Debug.LogWarning($"step 데이터가 비어있습니다. {configFilePath}파일을 확인해 주세요.");
                }
            }
        }
    }

    /// <summary>
    /// Teach 버튼을 클릭해서 저장된 steps 정보(Serialization)
    /// RobotConfig 객체화 -> robotConfig.json
    /// </summary>
    public void OnSaveBtnClkEvent()
    {
        RobotController.RobotConfig config = new RobotController.RobotConfig();
        config.steps = robotController.steps;

        using(FileStream fs = new FileStream(configFilePath, FileMode.Open))
        {
            using(StreamWriter sr = new StreamWriter(fs))
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);

                sr.Write(json);

                Debug.Log("로봇 설정정보가 성공적으로 저장되었습니다.");
            }
        }
    }
}
