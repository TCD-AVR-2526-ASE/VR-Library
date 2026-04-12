using UnityEngine;

/// <summary>
/// Stores the scene-wide time-of-day value as a simple singleton used by environment systems.
/// </summary>
public class WorldTime : MonoBehaviour
{
    public static WorldTime Instance;

    [Range(0f, 24f)]
    public float timeOfDay = 9f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Sets the current time of day in hours.
    /// </summary>
    /// <param name="t">The time value in the range 0-24.</param>
    public void SetTime(float t)
    {
        timeOfDay = Mathf.Clamp(t, 0f, 24f);
    }

    /// <summary>
    /// Returns the current time of day in hours.
    /// </summary>
    /// <returns>The current time value in the range 0-24.</returns>
    public float GetTime()
    {
        return timeOfDay;
    }
}
