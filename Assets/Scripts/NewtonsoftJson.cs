using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;


/// <summary>
/// 복잡한 객체 구조를 JSON 파일로 만들고 싶다.
/// </summary>
public class NewtonsoftJson : MonoBehaviour
{
    public class Person
    {
        public string name;
        public int age;
        public bool isStudent;
    }

    public class School
    {
        public List<Person> persons = new List<Person>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        School school = new School();
        Person person1 = new Person() { name = "신태욱" , age = 20, isStudent = true };
        Person person2 = new Person() { name = "김흥수" , age = 22, isStudent = false };
        Person person3 = new Person() { name = "김하나" , age = 25, isStudent = true };
        school.persons.Add(person1);
        school.persons.Add(person2);
        school.persons.Add(person3);

        string json = JsonConvert.SerializeObject(school, Formatting.Indented); // 자동줄바꿈 기능

        print(json);

        using (FileStream fs = new FileStream("school.json", FileMode.OpenOrCreate))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(json);
            }
        }

        School newSchool = JsonConvert.DeserializeObject<School>(json);
        print(newSchool.persons[0].name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
