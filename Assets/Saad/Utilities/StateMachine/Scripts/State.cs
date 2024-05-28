using System.Collections;
using UnityEngine;

public class State : ScriptableObject
{
    public bool Paused;
    protected IState _listener;
    public virtual IEnumerator Init(IState listener)
    {
        _listener = listener;
        yield return null;
    }
    public virtual IEnumerator Execute()
    {
        yield return null;
    }
    public virtual IEnumerator Pause()
    {
        Paused = true;
        yield return null;
    }
    public virtual IEnumerator Resume()
    {
        Paused = false;
        yield return null;
    }
    public virtual IEnumerator Tick()
    {
        yield return null;
    }
    public virtual IEnumerator Exit()
    {
        yield return null;
    }
}
