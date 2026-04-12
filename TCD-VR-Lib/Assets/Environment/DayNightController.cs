using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Synchronizes world time and lighting across the network, including the sun, ambient light, fog, and library lights.
/// </summary>
public class DayNightController : NetworkBehaviour
{
    [System.Serializable]
    struct CachedLibraryLight
    {
        public Light Light;
        public float BaseIntensity;
    }

    [Header("Time")]
    [SerializeField] float dayLengthInMinutes = 5f;
    [SerializeField] private float nightThreshold = 0.25f; // ~6am
    [SerializeField] private float dayThreshold = 0.75f;   // ~6pm
    private float timePercent;

    [Header("Lighting")]
    [SerializeField] Light sun;
    [SerializeField] float intensity;
    [SerializeField] Gradient ambientColor;
    [SerializeField] Gradient directionalColor;
    [SerializeField] Gradient fogColor;
    [SerializeField] private Slider sunSlider;
    [SerializeField] private Light[] nightLights;
    [SerializeField] private Transform libraryLightsRoot;

    private CachedLibraryLight[] cachedLibraryLights = System.Array.Empty<CachedLibraryLight>();

    NetworkVariable<float> networkTime = new NetworkVariable<float>(-1f);
    private bool hasInitializedTime = false;

    void Start()
    {
        InitializeTimeState();
        CacheLibraryLights();

        if (sunSlider != null)
        {
            sunSlider.onValueChanged.AddListener(SetLightIntensity);
            SetLightIntensity(sunSlider.value);
        }
    }

    void Update()
    {
        UpdateTime();
        UpdateLighting();
    }

    void UpdateTime()
    {
        dayLengthInMinutes = Mathf.Max(dayLengthInMinutes, 0.01f);
        float dayLengthSeconds = dayLengthInMinutes * 60f;

        if (!hasInitializedTime)
            InitializeTimeState();

        if (IsOwner)
        {
            float t = WorldTime.Instance.GetTime();
            t += (Time.deltaTime / dayLengthSeconds) * 24f;
            t %= 24f;

            networkTime.Value = t;
            WorldTime.Instance.SetTime(t);
        }
        else if (networkTime.Value >= 0f)
        {
            WorldTime.Instance.SetTime(networkTime.Value);
        }

        timePercent = WorldTime.Instance.GetTime() / 24f;
    }

    void InitializeTimeState()
    {
        if (WorldTime.Instance == null)
            return;

        float currentTime = WorldTime.Instance.GetTime();

        if (IsOwner)
        {
            networkTime.Value = currentTime;
        }
        else if (networkTime.Value >= 0f)
        {
            WorldTime.Instance.SetTime(networkTime.Value);
        }

        hasInitializedTime = true;
    }

    void UpdateLighting()
    {
        // Ambient + fog
        RenderSettings.ambientLight = ambientColor.Evaluate(timePercent) * intensity;
        RenderSettings.fogColor = fogColor.Evaluate(timePercent);

        if (sun != null)
        {
            // Sun color
            sun.color = directionalColor.Evaluate(timePercent);

            // Sun rotation (sunrise to sunset)
            sun.transform.localRotation = Quaternion.Euler(Mathf.Lerp(-90f, 270f, timePercent), 170f, 0);

            sun.intensity = intensity * directionalColor.Evaluate(timePercent).grayscale;
        }

        bool isNight = timePercent <= nightThreshold || timePercent >= dayThreshold;

        foreach (var light in nightLights)
        {
            if (light != null)
                light.enabled = isNight;
        }

        UpdateLibraryLights();
    }

    /// <summary>
    /// Updates the shared global light intensity multiplier.
    /// </summary>
    /// <param name="value">The brightness multiplier applied to environment lighting.</param>
    public void SetLightIntensity(float value)
    {
        intensity = value;
    }

    void CacheLibraryLights()
    {
        if (libraryLightsRoot == null)
        {
            cachedLibraryLights = System.Array.Empty<CachedLibraryLight>();
            return;
        }

        Light[] lights = libraryLightsRoot.GetComponentsInChildren<Light>(true);
        cachedLibraryLights = new CachedLibraryLight[lights.Length];

        for (int i = 0; i < lights.Length; i++)
        {
            cachedLibraryLights[i] = new CachedLibraryLight
            {
                Light = lights[i],
                BaseIntensity = lights[i].intensity
            };
        }
    }

    void UpdateLibraryLights()
    {
        for (int i = 0; i < cachedLibraryLights.Length; i++)
        {
            if (cachedLibraryLights[i].Light == null)
                continue;

            cachedLibraryLights[i].Light.intensity = cachedLibraryLights[i].BaseIntensity * intensity;
        }
    }
}
