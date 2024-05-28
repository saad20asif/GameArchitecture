using System.Collections;
using UnityEngine;

public class State : ScriptableObject
{
    public bool Paused;
    private IEnumerator Init()
    {
        yield return null;
    }
    private IEnumerator Execute()
    {
        yield return null;
    }
    private IEnumerator Pause()
    {
        Paused = true;
        yield return null;
    }
    private IEnumerator Resume()
    {
        Paused = false;
        yield return null;
    }
    private IEnumerator Tick()
    {
        yield return null;
    }
    private IEnumerator Exit()
    {
        yield return null;
    }
}
