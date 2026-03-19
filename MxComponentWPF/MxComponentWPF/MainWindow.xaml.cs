using ActUtlType64Lib;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MxComponentWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ActUtlType64 mxComponent;
        bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();

            mxComponent = new ActUtlType64();
        }

        private void ConnectBtnClkEvent(object sender, RoutedEventArgs e)
        {
            int iRet = mxComponent.Open();

            if (iRet == 0)
            {
                isConnected = true;

                MessageBox.Show("성공적으로 연결되었습니다!", "Connect", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void DisconnectBtnClkEvent(object sender, RoutedEventArgs e)
        {
            int iRet = mxComponent.Close();

            if(iRet == 0)
            {
                isConnected = false;

                MessageBox.Show("연결해지 성공!", "Disconnect", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                switch(iRet)
                {
                    case 0x1802001:
                        MessageBox.Show("디바이스 에러\n부정확한 디바이스 문자열입니다." +
                            "\n디바이스명을 검토하십시오 .", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    default:
                        MessageBox.Show(iRet.ToString("X"), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;

                }
            }
        }

        private void GetDeviceBtnClkEvent(object sender, RoutedEventArgs e)
        {
            int output;
            int iRet = mxComponent.GetDevice(deviceNameTB.Text, out output);
            
            if(iRet == 0)
            {
                MessageBox.Show(iRet.ToString($"{deviceNameTB.Text}의 값: {output}"),
                    "GetDevice", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void SetDeviceBtnClkEvent(object sender, RoutedEventArgs e)
        {
            // "1", "0" -> true
            // "dfsa" -> false

            int output;
            bool isParsed = Int32.TryParse(valueTB.Text, out output);

            if (!isParsed)
            {
                MessageBox.Show("숫자를 입력해 주세요.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            int iRet = mxComponent.SetDevice(deviceNameTB.Text, output);

            if (iRet == 0)
            {
                MessageBox.Show($"{deviceNameTB.Text}에 {output}이 성공적으로 입력되었습니다.",
                    "SetDevice", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void RDBBtnClkEvent(object sender, RoutedEventArgs e)
        {
            // 3개의 인풋들이 모두 입력이 되지 않으면 리턴
            if(deviceNameTB2.Text == "" || blockSizeTB.Text == "")
            {
                MessageBox.Show("정보입력 후 버튼을 눌러주세요.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            int blockSize;
            bool isParsed = Int32.TryParse(blockSizeTB.Text, out blockSize);

            if (!isParsed)
            {
                MessageBox.Show("블록 개수를 정수형으로 입력해 주세요.", "Error",
        MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            int[] blockValues = new int[blockSize]; // ex. { 64, 256 }
            int iRet = mxComponent.ReadDeviceBlock(deviceNameTB2.Text, blockSize, out blockValues[0]);

            if (iRet == 0)
            {
                deviceValues.Text = "";
                foreach(int value in blockValues)
                {
                    deviceValues.Text += value + " ";
                }
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error",
        MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void WDBBtnClkEvent(object sender, RoutedEventArgs e)
        {
            if(deviceNameTB2.Text == "" || blockSizeTB.Text == "" || deviceValues.Text == "")
            {
                MessageBox.Show("텍스트 박스에 정보를 입력 후 버튼을 눌러주세요.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            int blockSize;
            bool isParsed = Int32.TryParse(blockSizeTB.Text, out blockSize);

            if(!isParsed)
            {
                MessageBox.Show("블록의 개수를 양의 정수로 입력해 주세요.", "Error",
    MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            // 65 555 82
            // 1. " " 기준으로 문자열을 나눠서 배열에 담기 -> Split(" ")
            int[] values = new int[blockSize];

            string[] valueStr = new string[blockSize]; // { "65", "555", "82" }
            valueStr = deviceValues.Text.Split(" ");

            // values = Array.ConvertAll(valueStr, int.Parse);
            for (int i = 0; i < values.Length; i++)
            {
                isParsed = Int32.TryParse(valueStr[i], out values[i]);

                if (!isParsed)
                {
                    MessageBox.Show("블록에 들어갈 입력을 양의 정수형으로 입력해 주세요.", "Error",
    MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    return;
                }
            }

            int iRet = mxComponent.WriteDeviceBlock(deviceNameTB2.Text, blockSize, ref values[0]);

            if(iRet == 0)
            {
                MessageBox.Show($"{deviceNameTB2.Text}부터 {blockSizeTB.Text}개의 블록에 {deviceValues.Text}가" +
    $" 성공적으로 입력되었습니다.", "Write Device Block",
MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // 창이 닫힐 때 실행되는 이벤트 메서드
        private void DisconnectBtnClkEvent(object sender, EventArgs e)
        {
            int iRet = mxComponent.Close();

            if (iRet == 0)
            {
                MessageBox.Show("연결해지 성공!", "Disconnect", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Read Device Random: "X10,D100,Y20"
        private void RDRBtnClkEvent(object sender, RoutedEventArgs e)
        {
            if(deviceNameTB3.Text == "" || blockSizeTB3.Text == "")
            {
                MessageBox.Show("내용을 입력해 주세요.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            int blockSize;
            bool isParsed = Int32.TryParse(blockSizeTB3.Text, out blockSize);

            if (!isParsed)
            {
                MessageBox.Show("블록에 들어갈 입력을 양의 정수형으로 입력해 주세요.", "Error",
MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            string temp = deviceNameTB3.Text.Replace("\\n", "\n");
            string[] deviceNames = temp.Split("\n");
            if(deviceNames.Length != blockSize)
            {
                MessageBox.Show("디바이스 이름의 개수와 블록사이즈를 일치시켜 주세요.", "Error",
MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            int[] values = new int[blockSize];
            string valueStr = "";
            int iRet = mxComponent.ReadDeviceRandom(temp, blockSize, out values[0]);

            foreach (int v in values)
            {
                valueStr += v.ToString() + " ";
            }

            if (iRet == 0)
            {
                MessageBox.Show($"값들을 성공적으로 읽어왔습니다.\n" +
                    $"{deviceNameTB3.Text}: {valueStr}", "Error",
MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                if(iRet == 0x1802001)
                {
                    MessageBox.Show("디바이스 에러\n메소드에 지정된 디바이스 문자열이 부정확합니다." +
                        "D0\\nD1\\nD2 형식으로 입력해 주세요.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                }
                else
                {
                    MessageBox.Show(iRet.ToString("X"), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

            }
        }

        // Write Device Random
        private void WDRBtnClkEvent(object sender, RoutedEventArgs e)
        {

        }
    }
}