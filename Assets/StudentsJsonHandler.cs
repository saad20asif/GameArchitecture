using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Bson;

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

[Serializable]
public struct StudentWrapper
{
    public Student[] students;
}

public class StudentsJsonHandler : MonoBehaviour
{
    [SerializeField] private string FilePath;
    [SerializeField] private Student[] Students;

    private IJsonReader _jsonReader = new JsonFileReader();

    // Dependency injection via constructor or method injection can be used.
    // Here we use setter injection for simplicity.
    //public void SettJsonReader(IJsonReader jsonReader)
    //{
    //    _jsonReader = jsonReader;
    //}
    //private void Awake()
    //{
    //    _jsonReader = new JsonFileReader();
    //}
    [Button]
    private void LoadJson()
    {
        if (_jsonReader == null)
        {
            Debug.LogError("IJsonReader not set!");
            return;
        }

        try
        {
            StudentWrapper studentWrapper = _jsonReader.Read<StudentWrapper>(FilePath);
            Students = studentWrapper.students;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load JSON: {ex.Message}");
        }
    }
}
