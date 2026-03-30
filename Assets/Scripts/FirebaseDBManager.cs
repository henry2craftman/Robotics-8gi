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
/// </summary>
public class FirebaseDBManager : MonoBehaviour
{
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
    public LibraryData library;

    DatabaseReference dbRef;
    public string dbUrl;

    public string libraryName;
    public int bookCnt;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Test();

            //var availableBooks = library.books.Where(b => !b.is_borrowed).ToList();

            // 1. 대출 가능한 도서 추출 (SQL: SELECT * FROM books WHERE is_borrowed = false)
            var availableBooks = from b in library.books
                                 where b.is_borrowed == false
                                 select b;

            // 리스트로 변환
            List<Book> resultList = availableBooks.ToList();

            Debug.Log("=== [대출 가능 도서 목록] ===");
            foreach (var book in availableBooks)
            {
                Debug.Log($"ID: {book.id} | 제목: {book.title} | 저자: {book.author}");
            }


            // 2. 특정 태그가 포함된 도서의 '제목'만 추출 (SQL: SELECT title FROM books WHERE tags LIKE '%Python%')
            var pythonBookTitles = from b in library.books
                                   where b.tags.Contains("Python")
                                   select b.title;


            Debug.Log("\n=== [Python 관련 도서 제목] ===");
            foreach (var title in pythonBookTitles)
            {
                Debug.Log($"찾은 제목: {title}");
            }


            // 3. ID 순으로 정렬하여 추출 (SQL: SELECT * FROM books ORDER BY id DESC)
            var sortedBooks = from b in library.books
                              orderby b.id descending
                              select b;

            Debug.Log("\n=== [도서 ID 역순 정렬] ===");
            foreach (var book in sortedBooks)
            {
                Debug.Log($"ID: {book.id} - {book.title}");
            }
        }
    }

    public void Test()
    {
        dbRef.GetValueAsync().ContinueWith(t =>
        {
            if (t.IsCanceled)
            {
                print(t.Exception);
            }
            else if (t.IsFaulted)
            {
                print(t.Exception);
            }
            else if (t.IsCompleted)
            {
                DataSnapshot totalData = t.Result;

                library = JsonConvert.DeserializeObject<LibraryData>(totalData.GetRawJsonValue());

                Debug.Log($"도서관: {library.library_name}");
                Debug.Log($"책 개수: {library.books.Count}");

                foreach (var book in library.books)
                {
                    Debug.Log($"제목: {book.title}, 저자: {book.author}");
                }

                //foreach (var data in totalData.Children)
                //{
                //    // 1. 키값 확인 (이건 잘 나올 겁니다)
                //    string key = data.Key;
                //    Debug.Log($"현재 처리 중인 키: {key}");

                //    if (key == "library_name")
                //    {
                //        // Value.ToString()으로 안전하게 가져오기
                //        libraryName = data.Value.ToString();
                //        Debug.Log($"도서관 이름 확인: {libraryName}");
                //    }
                //    else if (key == "total_count")
                //    {
                //        // Firebase 숫자는 기본적으로 long 타입입니다.
                //        bookCnt = int.Parse(data.Value.ToString());
                //        Debug.Log($"총 개수 확인: {bookCnt}");
                //    }
                //    else if (key == "books")
                //    {
                //        // 'books'일 때만 리스트로 처리
                //        foreach (var book in data.Children)
                //        {
                //            Debug.Log($"책 제목: {book.Child("title").Value}");
                //        }
                //    }
                //}
            }
        });
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

                    Test();

                }
            }
            else
            {
                Debug.LogError("Firebase 의존성 오류: " + dependencyStatus);
            }
        });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeFirebase();
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
}
