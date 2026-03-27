using Firebase;
using Firebase.Database;
using UnityEngine;

/// <summary>
/// Firebase DB URL을 사용하여 DB의 JSON을 읽어온다. DB에 데이터를 쓴다.
/// 속성: DB Reference, url, json
/// </summary>
public class FirebaseDBManager : MonoBehaviour
{
    DatabaseReference dbRef;
    public string dbUrl;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbUrl); // DB URL 등록
        dbRef = FirebaseDatabase.DefaultInstance.RootReference; // DB의 가장 상위 참조

        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapShot = task.Result;

                string json = snapShot.GetRawJsonValue();
                print(json);

                foreach (var child in snapShot.Children)
                {
                    json = child.GetRawJsonValue();

                    print(json);
                }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
