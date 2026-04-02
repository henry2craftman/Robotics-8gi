using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google.MiniJSON;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// Firebase DB URL을 사용하여 DB의 JSON을 읽어온다. DB에 데이터를 쓴다.
/// 속성: DB Reference, url, json
/// MxComponent로 부터 하드웨어 PLC의 디바이스 정보를 DB에 쓴다.
/// </summary>
public class FirebaseDBManager : MonoBehaviour
{
    public static FirebaseDBManager instance; // 싱글턴 객체

    // 직렬화(serialization) 객체 -> File
    // 역직렬화(deserialization) File -> 객체
    [Serializable]
    public class Book
    {
        public string author;
        public int id;
        public bool is_borrowed;
        public List<string> tags = new List<string>();
        public string title;
    }

    [Serializable]
    public class LibraryData
    {
        public string library_name;
        public int total_count;
        public List<Book> books = new List<Book>();
    }

    public class PLCData
    {
        public string plcXData;
        public string plcYData;
    }

    public LibraryData library;
    public PLCData plcData = new PLCData();
    public bool isDataReceived;

    DatabaseReference dbRef;
    public string dbUrl;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject); // 씬 전환이 되어도 지워지지 않는 기능
        }
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                if(task.IsCompleted)
                {
                    FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbUrl);
                    dbRef = FirebaseDatabase.DefaultInstance.GetReference("library1");
                    Debug.Log("Firebase 초기화 완료");

                    RequestDBData();
                }
            }
            else
            {
                Debug.LogError("Firebase 의존성 오류: " + dependencyStatus);
            }
        });
    }

    void RequestDBData()
    {
        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCanceled)
            {
                print(task.Exception);
            }
            else if(task.IsFaulted)
            {
                print(task.Exception);
            }
            else if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                string json = snapshot.GetRawJsonValue();

                library = JsonConvert.DeserializeObject<LibraryData>(json);

                print($"도서관: {library.library_name}");
                print($"책 개수: {library.books.Count}");

                foreach(var book in library.books)
                {
                    print($"제목: {book.title}, 저자: {book.author}");
                }
            }
        });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeFirebase();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            // C# 람다식 이용방식
            var books = library.books.Where(book => book.is_borrowed).ToList();

            // SQL 쿼리와 방식( SELECT * FROM books WHERE is_borrowd = false )
            var availableBooks = from book in library.books
                                 where book.is_borrowed == false
                                 select book;

            List<Book> result = availableBooks.ToList();

            print("---- 대출 가능 도서 목록 ----");
            foreach(var book in result)
            {
                print($"제목: {book.title}, 저자: {book.author}");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var pythonBooks = from book in library.books
                              where book.tags.Contains("Python") || book.tags.Contains("Algorithm")
                              select book;

            print("---- 파이썬과 알고리즘 관련 도서 목록 ----");
            foreach(var book in pythonBooks)
            {
                print($"{book.id}: {book.title}");
            }
        }

        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            var sortedBooks = from book in library.books
                              orderby book.author ascending // 오름차순 1 -> 10, 가나다순
                              select book;

            print("---- 작가명 가나다순 정렬 ----");
            foreach(var book in sortedBooks)
            {
                print($"{book.id}: {book.title}");
            }
        }

        // 데이터 업데이트
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            Book newBook = new Book()
            {
                author = "신태욱",
                id = 3,
                is_borrowed = false,
                tags = new List<string> { "culture", "sociery" },
                title = "신태욱의 에세이"
            };

            library.books.Add(newBook);

            // 직렬화: Object -> JSON
            string json = JsonConvert.SerializeObject(library, Formatting.Indented);

            dbRef.SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if(task.IsCompleted)
                {
                    print("도서 1권이 추가되었습니다.");
                }
            });
        }

        // 1. 결재완료기능
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            // 결재가 완료되면 기존 role을 user -> admin 승격(쓰기)
            UpdateUserInfo(FirebaseAuthManager.instance.user,
                FirebaseAuthManager.instance.userInfo);
        }

        // 2. admin일 때만 내 정보를 변경
        if(Input.GetKeyDown(KeyCode.Alpha6))
        {

        }
    }

    private void UpdateUserInfo(FirebaseUser user, FirebaseAuthManager.User userInfo)
    {
        userInfo.role = "admin";

        string json = JsonConvert.SerializeObject(userInfo);

        dbRef.Child("users").Child(user.UserId).SetRawJsonValueAsync(json);
    }

    public void ResisterUserInfo(FirebaseUser user, FirebaseAuthManager.User userInfo)
    {
        string json = JsonConvert.SerializeObject(userInfo);

        dbRef.Child("users").Child(user.UserId).SetRawJsonValueAsync(json);
    }

    public void RequestUserInfo(FirebaseUser user)
    {
        dbRef.Child("users").Child(user.UserId).GetValueAsync().ContinueWith(task => 
        {
            if (task.IsCompleted)
            {
                DataSnapshot data = task.Result;

                string json = data.GetRawJsonValue();

                FirebaseAuthManager.instance.userInfo = JsonConvert.DeserializeObject<FirebaseAuthManager.User>(json);

                print("유저 정보를 불러왔습니다. " + user.UserId);
            }
        });
    }

    private void GetDBValueByKey(string key)
    {
        dbRef = FirebaseDatabase.DefaultInstance.GetReference(key); // DB의 가장 상위 특정 Key를 참조

        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogWarning($"요청 실패: {task.Exception}");
            }
            else if (task.IsCanceled)
            {
                Debug.LogWarning($"요청 실패: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapShot = task.Result;

                string json = snapShot.GetRawJsonValue();
                print(json);

                foreach (var child in snapShot.Children)
                {
                    //json = child.GetRawJsonValue();
                    //print(json);
                    print($"Key: {child.Key}");
                    //print($"Value: {child.Value}");
                    IDictionary val = (IDictionary)child.Value;
                    print($"{val["id"]} / {val["name"]}");
                }
            }
        });
    }

    

    public void SendPLCData(string xData, string yData)
    {
        plcData.plcXData = xData;
        plcData.plcYData = yData;

        string json = JsonConvert.SerializeObject(plcData);

        dbRef.Child("PLCData").SetRawJsonValueAsync(json);
    }

    // Slave: DB -> PLC
    public void RequestPLCData()
    {
        dbRef.Child("PLCData").GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCompleted)
            {
                DataSnapshot data = task.Result;
                string json = data.GetRawJsonValue();

                plcData = JsonConvert.DeserializeObject<PLCData>(json);

                isDataReceived = true;
            }
        });
    }
}
