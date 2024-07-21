using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using ProjectCore.Utilities.Json;

[Serializable]
public struct Address
{
    public string street;
    public string city;
    public string state;
    public string postalCode;
}

[Serializable]
public struct Course
{
    public string courseId;
    public string courseName;
    public string instructor;
}

[Serializable]
public struct Grades
{
    public int math;
    public int science;
    public int english;
    public int history;
}

[Serializable]
public struct Student
{
    public int id;
    public string name;
    public int age;
    public string email;
    public Address address;
    public Grades grades;
    public Course[] courses;
}
[SerializeField]
struct StudentWrapper
{
    public Student[] Students;
}


public class StudentsJsonHandler : MonoBehaviour
{
    [SerializeField] private string filepath;
    private IJsonReader _jsonReader = new JsonFileReader();
    private IJsonWriter _jsonWriter = new JsonFileWriter();
    [SerializeField] Student[] Students;

    [Button]
    private void ReadJson()
    {
        if(_jsonReader == null)
        {
            print("Json Reader is null!");
            return;
        }

        try
        {
            print("asdas");
            StudentWrapper _studentWrapper = _jsonReader.ReadJson<StudentWrapper>(filepath);
            print("Here I am : "+ _studentWrapper.Students);
            Students = _studentWrapper.Students;
        }
        catch(Exception ex)
        {
            Debug.LogError($"Reading Json Failed with exception : {ex}");
        }
    }
    [Button]
    private void WriteJson()
    {
        if (_jsonWriter == null)
        {
            print("Json Writer is null!");
            return;
        }
        try
        {
            StudentWrapper _studentWrapper;
            _studentWrapper.Students = Students;
            _jsonWriter.WriteJson<StudentWrapper>(filepath, _studentWrapper);
        }
        catch(Exception ex)
        {
            Debug.LogError($"Writing Json Failed with exception : {ex}");
        }
    }

}
