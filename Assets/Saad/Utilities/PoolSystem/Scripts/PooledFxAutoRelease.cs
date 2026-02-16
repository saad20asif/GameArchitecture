using System.Collections;
using Blues.Core.PoolSystem;
using UnityEngine;

[DisallowMultipleComponent]
public class PooledFxAutoRelease : MonoBehaviour
{
    private PoolManagerSO _pool;
    private string _poolId;
    private float _delay;
    private Coroutine _job;

    // Call this right after you Get() the FX from the pool
    public void Arm(PoolManagerSO pool, string poolId, float delaySeconds)
    {
        _pool   = pool;
        _poolId = poolId;
        _delay  = delaySeconds;

        if (_job != null) StopCoroutine(_job);
        _job = StartCoroutine(ReleaseAfterDelay());
    }

    private IEnumerator ReleaseAfterDelay()
    {
        yield return new WaitForSeconds(_delay);

        // Stop/clear just in case the prefab loops
        var ps = GetComponent<ParticleSystem>();
        if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (_pool != null)
            _pool.Release(_poolId, gameObject); // Pooler should SetActive(false)
    }

    // If something else disables the FX early, also clear emission
    private void OnDisable()
    {
        var ps = GetComponent<ParticleSystem>();
        if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}