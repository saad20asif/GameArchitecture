using System.Collections;
using UnityEngine;

public class State : ScriptableObject
{
    protected IState Listener;
    public virtual IEnumerator Init(IState _listener)
    {
        Listener = _listener;
        yield return null;
    }
    public virtual IEnumerator Execute()
    {
        yield return null;
    }
    public virtual IEnumerator Exit()
    {
        yield return null;
    }
}
