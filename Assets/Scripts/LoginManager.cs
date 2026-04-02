using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1. 아이디와 패스워드가 일치하면 로그인을 한다.
/// 2. 회원가입 버튼을 누르면 회원가입 패널로 넘어간다(Login패널OFF, PW패널ON)
/// 3. 아이디가 존재하지 않고, 비밀번호가 비밀번호 체크와 일치하는지 확인한다.
/// 4. 정규표현식을 통해 비밀번호가 유효하면 회원가입을 한다.
/// </summary>
public class LoginManager : MonoBehaviour
{
    [Header("로그인 패널")] // 어트리뷰트
    public GameObject signInPanel;
    public TMP_InputField signInIDInput;
    public TMP_InputField signInPWInput;
    public Button signInBtn;
    public Button exitBtn;
    public Button signUpBtn;

    [Header("회원가입 패널")]
    public GameObject signUpPanel;
    public GameObject verificationPanel;
    public TMP_Text verificationTxt;
    public TMP_InputField signUpIDInput;
    public TMP_InputField signUpPWInput;
    public TMP_InputField signUpPWCheckInput;
    public TMP_InputField nameInput;
    public TMP_InputField addressInput;
    public TMP_InputField phoneInput;
    public Button oKBtn;
    public Button cancelBtn;


    [Header("아이디 비밀번호 리스트")]
    public Dictionary<string, string> memberList = new Dictionary<string, string>();

    private void Awake()
    {
        memberList.Add("testID", "aB12345!");

        signInBtn.onClick.AddListener(OnSignInBtnClkEvent);
        exitBtn.onClick.AddListener(OnExitBtnClkEvent);
        signUpBtn.onClick.AddListener(OnSignUpBtnClkEvent);

        oKBtn.onClick.AddListener(OnOKBtnClkEvent);
        cancelBtn.onClick.AddListener(OnCancelBtnClkEvent);
    }

    public void OnSignInBtnClkEvent()
    {
        FirebaseAuthManager.instance.LogIn(signInIDInput.text, signInPWInput.text);

        //var member = memberList.FirstOrDefault(m => m.Key == signInIDInput.text);

        //if(member.Value == signInPWInput.text)
        //{
        //    print("로그인에 성공하였습니다!");
        //}
        //else
        //{
        //    Debug.LogWarning("로그인에 실패하였습니다.");
        //}
    }

    public void OnExitBtnClkEvent()
    {
        FirebaseAuthManager.instance.LogOut();

        Debug.LogWarning("프로그램을 종료합니다.");

        Application.Quit();
    }

    public void OnSignUpBtnClkEvent()
    {
        signUpPanel.SetActive(true);
        signInPanel.SetActive(false);
    }

    public void OnCancelBtnClkEvent()
    {
        signInPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }

    public void OnOKBtnClkEvent()
    {
        //var member = memberList.FirstOrDefault(m => m.Key == signUpIDInput.text);
        //print(member.Value);
        //if (member.Key == signUpIDInput.text)
        //{
        //    Debug.LogWarning("이미 존재하는 아이디 입니다. 아이디를 다시 입력해 주세요.");
        //}
        //else
        //{
            if(nameInput.text.Length == 0 || addressInput.text.Length == 0 || phoneInput.text.Length == 0)
            {
                Debug.LogWarning("필수 정보를 입력해 주세요.");
                return;
            }

            if (signUpPWInput.text != signUpPWCheckInput.text)
            {
                Debug.LogWarning("비밀번호가 다릅니다. 비밀번호를 확인해 주세요.");
            }
            else
            {
                bool isValid = IsValidPassword(signUpPWInput.text);

                if(!isValid)
                {
                    Debug.LogWarning("대/소 문자 1개 이상, 특수문자 1개 이상 포함하여 8자 이상 입력해 주세요.");
                }
                else
                {
                    // memberList.Add(signUpIDInput.text, signUpPWInput.text);

                    FirebaseAuthManager.instance.SignUp(signUpIDInput.text, signUpPWInput.text,
                        nameInput.text, addressInput.text, phoneInput.text);

                    // Debug.Log("회원가입에 성공하였습니다!");
                    StartCoroutine(PopUpVerificationPanel());

                    signInPanel.SetActive(true);
                    signUpPanel.SetActive(false);
                }
            }
        //}
    }

    IEnumerator PopUpVerificationPanel()
    {
        verificationTxt.text = $"Check verification message\n{signUpIDInput.text}";

        verificationPanel.SetActive(true);

        yield return new WaitForSeconds(3);

        verificationPanel.SetActive(false);
    }

    // 정규표현식 메서드
    public bool IsValidPassword(string password)
    {
        // 1. 대/소문자 포함 (?=.*[a-zA-Z])
        // 2. 특수문자 포함 (?=.*[\W_]) -> 문자/숫자가 아닌 것 또는 언더바
        // 3. 8자 이상 .{8,}
        string pattern = @"^(?=.*[a-zA-Z])(?=.*[\W_]).{8,}$";

        if (string.IsNullOrEmpty(password)) return false;

        return Regex.IsMatch(password, pattern);
    }
}
