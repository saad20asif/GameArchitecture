using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationBase : MonoBehaviour
{
    [SerializeField] private int ApplicationFrameRate = 60;
    private void Awake()
    {
        Application.targetFrameRate = ApplicationFrameRate;
    }
}
