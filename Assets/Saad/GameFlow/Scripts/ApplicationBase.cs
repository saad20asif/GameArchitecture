using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationBase : MonoBehaviour
{
    [SerializeField] private int AndroidFrameRate = 60;
    [SerializeField] private int IOSFrameRate = 60;
    [SerializeField] private FiniteStateMachine FiniteStateMachine;
    private IEnumerator Start()
    {
        Application.targetFrameRate = AndroidFrameRate;
        Debug.Log("ApplicationBase Start called");
        yield return FiniteStateMachine.Init();
        yield return null;
    }
}
