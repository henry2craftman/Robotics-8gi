using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 직렬화(Object -> File)
/// 역직렬화(File -> Object)를 Unity의 JsonUtility 클래스를 사용하여 테스트
/// </summary>
public class SerializationManager : MonoBehaviour
{
    string json = "{\r\n  \"name\": \"John\",\r\n  \"age\": 30,\r\n  \"isStudent\": false\r\n";

    public class Person
    {
        public string name;
        public int age;
        public bool isStudent;
    }

    // 복잡한 구조의 객체를 직렬화 -> JSON.NET 라이브러리
    public class School
    {
        public List<Person> personList;
    }

    void Start()
    {
        Person person = new Person();
        person.name = "신태욱";
        person.age = 20;
        person.isStudent = true;

        string newJson = JsonUtility.ToJson(person); // 직렬화
        print(newJson);

        FileStream fs = new FileStream("config.json", FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fs);
        sw.Write(newJson);

        sw.Close();
        fs.Close();

        FileStream fs2 = new FileStream("config.json", FileMode.Open);
        StreamReader sr = new StreamReader(fs2);
        string readJson = sr.ReadLine();

        // 역직렬화: JSON 형식과 같은 필드명을 가진 클래스 필요
        Person person2 = JsonUtility.FromJson<Person>(readJson);
        print($"나는 제2의 {person2.name}입니다.");

        sr.Close();
        fs2.Close();

        // using -> 자동으로 메모리 해제(해지 안한 경우 메모리 누수 memory leak)
        using(FileStream fs3 = new FileStream("config.json", FileMode.Open))
        {
            using (StreamReader sr3 = new StreamReader(fs3))
            {
                readJson = sr3.ReadLine();

                // 역직렬화: JSON 형식과 같은 필드명을 가진 클래스 필요
                Person person3 = JsonUtility.FromJson<Person>(readJson);
                print($"나는 제3의 {person3.name}입니다.");
            }
        }
    }
}
