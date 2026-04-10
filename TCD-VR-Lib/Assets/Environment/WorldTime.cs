using UnityEngine;

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

    public void SetTime(float t)
    {
        timeOfDay = Mathf.Clamp(t, 0f, 24f);
    }

    public float GetTime()
    {
        return timeOfDay;
    }
}
