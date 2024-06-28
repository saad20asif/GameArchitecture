using System.Collections;
using UnityEngine;

public class State : ScriptableObject
{
    protected IState Listener;
    public virtual IEnumerator Enter()
    {
        yield return null;
    }
    public virtual IEnumerator Exit()
    {
        yield return null;
    }
}
