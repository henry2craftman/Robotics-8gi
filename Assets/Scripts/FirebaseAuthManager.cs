using Firebase.Auth;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static FirebaseAuthManager;

/// <summary>
/// Firebase를 사용하여 회원가입, 로그인, 로그아웃 기능을 사용
/// 속성: 로그인, 로그아웃, 회원가입, FirebaseAuth, FirebaseUser
/// </summary>

public class FirebaseAuthManager : MonoBehaviour
{
    [Serializable]
    public class User
    {
        public string email;
        public string name;
        public string address;
        public string phoneNumber;
        public string role;
    }
    public User userInfo = new User();

    public static FirebaseAuthManager instance; // 싱글턴 객체

    public bool isLoggedIn = false;

    FirebaseAuth auth;  // Firebase Auth 정보 + 기능
    public FirebaseUser user;  // 유저 정보
    bool isSignedUp = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // 나 자신을 참조
        }
    }

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged; // 이벤트 헨들러

        StartCoroutine(CheckSignUp());
        StartCoroutine(CheckLoggedIn());
    }

    IEnumerator CheckLoggedIn()
    {
        while (true)
        {
            yield return new WaitUntil(() => isLoggedIn == true);

            SceneManager.LoadScene(1); // "MPS + UR16e"

            isLoggedIn = false;
        }
    }

    // 로그인 상태 변경감지 메서드
    void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        if (auth.CurrentUser != user)
        {
            bool isSignIn = (user != auth.CurrentUser) && (auth.CurrentUser != null);

            user = auth.CurrentUser;

            if (isSignIn)
            {
                print($"로그인이 되었습니다. {user.UserId}"); // Authentication 고유 아이디
            }
            else if(!isSignIn && user != null)
            {
                print($"로그아웃 되었습니다. {user.UserId}");
            }
        }
    }

    public void LogIn(string id, string pw)
    {
        auth.SignInWithEmailAndPasswordAsync(id, pw).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                print(task.Exception);
            }
            else if (task.IsFaulted)
            {
                print(task.Exception);
            }
            else if (task.IsCompleted)
            {
                user = task.Result.User;

                if (!user.IsEmailVerified)
                {
                    print($"이메일 확인이 필요합니다.");
                }
                else
                {
                    FirebaseDBManager.instance.RequestUserInfo(user);

                    print($"로그인이 성공적으로 완료되었습니다! {user.UserId}");

                    isLoggedIn = true;
                }
            }
        });
    }

    public void LogOut()
    {
        auth.SignOut();
    }

    public void SignUp(string id, string pw, string name, string address, string phone)
    {
        auth.CreateUserWithEmailAndPasswordAsync(id, pw).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                print(task.Exception);
            }
            else if (task.IsFaulted)
            {
                print(task.Exception);
            }
            else if(task.IsCompleted)
            {
                user = task.Result.User;

                userInfo.email = user.Email;
                userInfo.name = name;
                userInfo.address = address;
                userInfo.phoneNumber = phone;
                userInfo.role = "user";

                isSignedUp = true;

                user.SendEmailVerificationAsync().ContinueWith(t =>
                {
                    if (task.IsCanceled)
                    {
                        print(task.Exception);
                    }
                    else if (task.IsFaulted)
                    {
                        print(task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        print("인증메일 전송완료");
                    }
                });

                print($"회원가입이 성공적으로 완료되었습니다! {user.UserId}");
            }
        });
    }

    IEnumerator CheckSignUp()
    {
        while(true)
        {
            yield return new WaitUntil(() => isSignedUp == true);

            // FirebaseDBManager를 사용하여 내 UID 기준 회원정보 등록
            FirebaseDBManager.instance.ResisterUserInfo(user, userInfo);

            isSignedUp = false;
        }
    }
}
