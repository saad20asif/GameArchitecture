using UnityEngine;
using UnityEngine.UI;
using Blues.Core.PoolSystem;

public class PoolMonitor : MonoBehaviour
{
    [SerializeField] private Text debugText;
    [SerializeField] private PoolManagerSO poolManager;
    [SerializeField] private float refreshRate = 1f;

    private float _timer;

    /*void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= refreshRate)
        {
            _timer = 0;
            RefreshDebug();
        }
    }

    private void RefreshDebug()
    {
        if (debugText == null || poolManager == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>POOL USAGE:</b>");

        foreach (var kvp in poolManager.DebugAllPools())
        {
            sb.AppendLine($"{kvp.Key} → Active: {kvp.Value.active}, Inactive: {kvp.Value.inactive}, Total: {kvp.Value.total}");
        }

        debugText.text = sb.ToString();
    }*/
}