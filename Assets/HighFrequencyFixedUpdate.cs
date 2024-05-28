using UnityEngine;

public class HighFrequencyFixedUpdate : MonoBehaviour
{
    private float lastUpdate;
    private int fixedUpdateCount;

    void Start()
    {
        Time.fixedDeltaTime = 1f / 50000f; // Set to 300 updates per second
        lastUpdate = Time.time;
    }

    void FixedUpdate()
    {
        fixedUpdateCount++;
        // Simulate physics calculations
    }

    void Update()
    {
        if (Time.time - lastUpdate >= 1f)
        {
            Debug.Log($"FixedUpdate calls per second: {fixedUpdateCount}");
            fixedUpdateCount = 0;
            lastUpdate = Time.time;
        }

        Debug.Log($"Frame Rate: {1f / Time.deltaTime} frames per second");
    }
}
