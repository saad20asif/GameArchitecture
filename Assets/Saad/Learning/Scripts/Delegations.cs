using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Delegations : MonoBehaviour
{
    [Header("Test All these in Playmode")]
    #region UnityEvents
    public UnityEvent OnButtonClick;  // You can bind functions with this from inspector like Ui Buttons!
    [Button]
    public void ButtonClicked()
    {
        OnButtonClick.Invoke();
    }
    #endregion

    #region DelegateWithReturnType
    public delegate int Callback(); // It can store a refernce of function which returns int
    [Button("DelegateWithReturnType")]
    private void CallDelegateWithReturnType()
    {
        StartCoroutine(DownloadData(OnDownloadComplete));
    }

    private IEnumerator DownloadData(Callback callback)
    {
        // Simulate a web request with a delay
        yield return new WaitForSeconds(2.0f);
        // Invoke the callback once the download is complete
        int a = (int)(callback?.Invoke());
        print(a);
    }

    private int OnDownloadComplete()
    {
        Debug.Log("Download complete!");
        return 90;
    }
    #endregion  

    #region DelegateWithParameter
    public delegate void MyDelegate(int var); // It can store a refernce of function with parameter int
    private void TempFunc(int var)
    {
        print($"var : {var}");
    }

    [Button("DelegateWithParameter")]
    private void Main()
    {
        MyDelegate del = TempFunc;
        del?.Invoke(10);
    }
    #endregion

    #region DelegateIssueExample
    public delegate void MyDelegateNew(int var);
    public static event MyDelegateNew OnEvent;
    [Button]
    private void VerifyDelegateIssue()
    {
        // Class A subscribes to the delegate
        OnEvent += ClassAHandler;

        OnEvent = TempFuncNew;
        // Class B subscribes to the delegate
        OnEvent += ClassBHandler;

        // Somewhere in the code, the delegate is reassigned, resetting the invocation list
        //OnEvent = ClassBHandler;
        //OnEvent = ClassAHandler;

        // Only TempFunc will be called, ClassAHandler and ClassBHandler are removed
        OnEvent?.Invoke(42);
    }

    private void ClassAHandler(int var)
    {
        Debug.Log("Class A Handler: " + var);
    }

    private void ClassBHandler(int var)
    {
        Debug.Log("Class B Handler: " + var);
    }

    private void TempFuncNew(int var)
    {
        Debug.Log("TempFunc: " + var);
    }
    #endregion
}

class A : MonoBehaviour
{
    private void OnEnable()
    {
        Delegations.OnEvent += Evi;
        Delegations.OnEvent += Evi;
        //Delegations.OnEvent = Evi2;
        //Delegations.OnEvent?.Invoke(42);
    }
    private void OnDisable()
    {
        Delegations.OnEvent -= Evi;
    }
    private void Evi(int a)
    {

    }
    private void Evi2(int b)
    {

    }
}
