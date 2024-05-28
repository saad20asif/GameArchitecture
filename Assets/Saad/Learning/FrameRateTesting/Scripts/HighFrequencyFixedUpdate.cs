using UnityEngine;

public class HighFrequencyFixedUpdate : MonoBehaviour
{
    private float lastUpdate;
    private int fixedUpdateCount;
    [SerializeField] private int RequiredFramesInFixedUpdate;

    void Start()
    {
        Time.fixedDeltaTime = 1f / RequiredFramesInFixedUpdate; // Set to RequiredFramesInFixedUpdate updates per second
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
