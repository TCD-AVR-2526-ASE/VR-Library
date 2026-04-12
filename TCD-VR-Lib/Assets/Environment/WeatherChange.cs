using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Coordinates shared weather, season ambience, and wind-driven particle behavior across the network.
/// The owner chooses the active season and weather state while other clients mirror the replicated values.
/// </summary>
public class WeatherChange : NetworkBehaviour
{
    [Header("Particle System")]
    [SerializeField] private ParticleSystem ps; // Main particle system used for rain/snow
    [SerializeField] private Vector3 particleSpawnPosition = new(0f, 60f, 0f);

    // Cached particle modules
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.VelocityOverLifetimeModule velocity;
    private ParticleSystem.CollisionModule collision;
    private ParticleSystemRenderer psRenderer;
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    private Season currentSeason;

    [Header("UI Controls")]
    [SerializeField] private Slider timeSlider;     
    [SerializeField] private Slider windSlider;     // Controls volume of seasonal ambience
    [SerializeField] float weatherChangeIntervalHours = 3f;
    [SerializeField] private Button springButton;
    [SerializeField] private Button summerButton;
    [SerializeField] private Button autumnButton;
    [SerializeField] private Button winterButton;
    private float lastWeatherCheck;
    private int currentWeatherMode = 0; // 0 none, 1 rain, 2 snow

    [Header("Weather Audio")]
    [SerializeField] private Transform weatherAudioParent; // Parent containing spatial weather audio sources
    [SerializeField] private AudioClip rainAudio;
    [SerializeField] private AudioClip snowAudio;
    private AudioSource[] weatherAudios; // All audio sources used for weather ambience
    Coroutine weatherFade;

    [Header("Season Audio")]
    [SerializeField] private Transform seasonAudioParent; // Parent containing spatial season audio sources
    [SerializeField] private AudioClip springAudio;
    [SerializeField] private AudioClip summerAudio;
    [SerializeField] private AudioClip autumnAudio;
    [SerializeField] private AudioClip winterAudio;
    private AudioSource[] seasonAudios; // All audio sources used for season ambience
    Coroutine seasonFade;


    NetworkVariable<int> networkWeather = new NetworkVariable<int>(); // 0 none, 1 rain, 2 snow
    NetworkVariable<int> networkSeason = new NetworkVariable<int>();
    NetworkVariable<float> networkWind = new NetworkVariable<float>();
    NetworkVariable<float> networkTime = new NetworkVariable<float>();

    private void Awake()
    {
        // Move particle system to spawn
        ps.transform.position = particleSpawnPosition;

        // Cache particle modules cuz unity is a bitchass mf
        main = ps.main;
        emission = ps.emission;
        shape = ps.shape;
        velocity = ps.velocityOverLifetime;
        collision = ps.collision;
        psRenderer = ps.GetComponent<ParticleSystemRenderer>();

        // Ensure particles are not running on start
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Get all audio sources from the parent object
        weatherAudios = weatherAudioParent.GetComponentsInChildren<AudioSource>();
        seasonAudios = seasonAudioParent.GetComponentsInChildren<AudioSource>();
    }

    private void Start()
    {
        // Attach slider events to their control functions
        timeSlider.value = 0;
        windSlider.onValueChanged.AddListener(TimeControl);
        windSlider.value = 0.5f;
        windSlider.onValueChanged.AddListener(WindControl);
        springButton.onClick.AddListener(() => SetSeason(0));
        summerButton.onClick.AddListener(() => SetSeason(1));
        autumnButton.onClick.AddListener(() => SetSeason(2));
        winterButton.onClick.AddListener(() => SetSeason(3));
        SetSeason(0); // default = spring
        RollWeather();
        lastWeatherCheck = WorldTime.Instance.GetTime();
    }
    private void Update()
    {
        if (!IsOwner)
        {
            if (networkSeason.Value != (int)currentSeason)
                SetSeason(networkSeason.Value);

            if (networkWeather.Value != currentWeatherMode)
                WeatherControl(networkWeather.Value);

            WindControl(networkWind.Value);
            return;
        }
        float currentTime = WorldTime.Instance.GetTime();
        float delta = Mathf.Abs(currentTime - lastWeatherCheck);
        if (delta > 12f) delta = 24f - delta;
        if (delta >= weatherChangeIntervalHours)
        {
            RollWeather();
            lastWeatherCheck = currentTime;
        }
    }

    /// <summary>
    /// Resets the automatic weather timer after the time of day is adjusted manually.
    /// </summary>
    public void OnTimeManuallySet()
    {
        lastWeatherCheck = WorldTime.Instance.GetTime();
    }

    void RollWeather()
    {
        float r = UnityEngine.Random.value;

        switch (currentSeason)
        {
            case Season.Spring:
                if (r < 0.9f) WeatherControl(0); // none
                else WeatherControl(1); // rain
                break;

            case Season.Summer:
                WeatherControl(0);
                break;

            case Season.Autumn:
                if (r < 0.4f) WeatherControl(0);
                else if (r < 0.9f) WeatherControl(1);
                else WeatherControl(2);
                break;

            case Season.Winter:
                if (r < 0.1f) WeatherControl(0);
                else if (r < 0.2f) WeatherControl(1);
                else WeatherControl(2);
                break;
        }
    }

    /// <summary>
    /// Changes the active season and synchronizes it to other clients when called by the owner.
    /// </summary>
    /// <param name="s">The integer value of the target <see cref="Season"/>.</param>
    public void SetSeason(int s)
    {
        currentSeason = (Season)s;
        if (IsOwner)
        {
            networkSeason.Value = s;
        }
        SeasonControl(s);

    }

    void WeatherControl(float value)
    {
        int mode = Mathf.RoundToInt(value);
        // Reset particles and weather audio
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        AudioClip targetClip = null;
        currentWeatherMode = mode;
        if (IsOwner)
        {
            networkWeather.Value = mode;
        }

        switch (mode)
        {
            case 1: // Rain
                RainMode();
                targetClip = rainAudio;
                ps.Play();
                break;

            case 2: // Snow
                SnowMode();
                targetClip = snowAudio;
                ps.Play();
                break;

            default: // No weather
                break;
        }

        if (weatherFade != null)
            StopCoroutine(weatherFade);
        weatherFade = StartCoroutine(PlayWeatherAudio(targetClip));
        WindControl(windSlider.value);
    }

    void SeasonControl(float value)
    {
        int mode = Mathf.RoundToInt(value);
        // Determine which seasonal ambience clip to use
        AudioClip clip = mode switch
        {
            0 => springAudio,
            1 => summerAudio,
            2 => autumnAudio,
            3 => winterAudio,
            _ => springAudio
        };
        if (seasonFade != null)
            StopCoroutine(seasonFade);
        seasonFade = StartCoroutine(PlaySeasonAudio(clip));
    }
    
    void TimeControl(float value)
    {
        if (IsOwner || IsClient)
        {
            networkTime.Value = value;
        }
    }

    void WindControl(float value)
    {
        if (IsOwner)
        {
            networkWind.Value = value;
        }
        float seasonVol = Mathf.Max(0.4f, value);
        foreach (var audio in seasonAudios)
            audio.volume = seasonVol;
        float weatherVol = windSlider.value * 0.4f;
        foreach (var audio in weatherAudios)
            audio.volume = weatherVol;

        int weatherMode = currentWeatherMode;
        switch (weatherMode)
        {
            case 1: // Rain
                velocity.x = new ParticleSystem.MinMaxCurve(-value * 25f, value * 25f);
                velocity.z = new ParticleSystem.MinMaxCurve(-value * 10f, value * 10f);
                velocity.y = new ParticleSystem.MinMaxCurve(
                    -40f - value * 10f,
                    -20f - value * 10f
                );
                emission.rateOverTime = Mathf.Lerp(250f, 320f, value);
                break;

            case 2: // Snow
                velocity.x = new ParticleSystem.MinMaxCurve(-0.6f - value * 2f, 0.6f + value * 2f);
                velocity.z = new ParticleSystem.MinMaxCurve(-0.6f - value * 2f, 0.6f + value * 2f);
                velocity.y = new ParticleSystem.MinMaxCurve(
                    -18f - value * 4f,
                    -8f - value * 4f
                );

                var noise = ps.noise;
                noise.enabled = true;
                noise.strength = Mathf.Lerp(0.6f, 1.5f, value);
                noise.frequency = Mathf.Lerp(0.25f, 0.5f, value);
                break;
        }
    }

    IEnumerator PlayWeatherAudio(AudioClip newClip)
    {
        float duration = 1f;
        float startVolume = weatherAudios[0].volume;

        // Fade out
        for (float t = 0; t < duration / 2f; t += Time.deltaTime)
        {
            float v = Mathf.Lerp(startVolume, 0f, t / (duration / 2f));
            foreach (var audio in weatherAudios)
                audio.volume = v;

            yield return null;
        }

        foreach (var audio in weatherAudios)
        {
            audio.Stop();

            if (newClip != null)
            {
                audio.clip = newClip;
                audio.loop = true;
                audio.spatialBlend = 1f;
                audio.Play();
            }
        }

        if (newClip == null)
            yield break;

        // Fade in
        for (float t = 0; t < duration / 2f; t += Time.deltaTime)
        {
            float v = Mathf.Lerp(0f, windSlider.value * 0.4f, t / (duration / 2f));
            foreach (var audio in weatherAudios)
                audio.volume = v;

            yield return null;
        }

        float target = windSlider.value * 0.4f;
        foreach (var audio in weatherAudios)
            audio.volume = target;
    }

    IEnumerator PlaySeasonAudio(AudioClip newClip)
    {
        float duration = 1.5f;
        float startVolume = seasonAudios[0].volume;

        // Fade out
        for (float t = 0; t < duration / 2f; t += Time.deltaTime)
        {
            float v = Mathf.Lerp(startVolume, 0f, t / (duration / 2f));
            foreach (var audio in seasonAudios)
                audio.volume = v;

            yield return null;
        }

        foreach (var audio in seasonAudios)
        {
            audio.Stop();

            if (newClip != null)
            {
                audio.clip = newClip;
                audio.loop = true;
                audio.spatialBlend = 1f;
                audio.Play();
            }
        }

        if (newClip == null)
            yield break;

        // Fade in
        for (float t = 0; t < duration / 2f; t += Time.deltaTime)
        {
            float target = Mathf.Max(0.4f, windSlider.value);
            float v = Mathf.Lerp(0f, target, t / (duration / 2f));
            foreach (var audio in seasonAudios)
                audio.volume = v;

            yield return null;
        }

        foreach (var audio in seasonAudios)
            audio.volume = Mathf.Max(0.4f, windSlider.value);
    }

    /// <summary>
    /// Configures the particle system for rain behavior.
    /// </summary>
    public void RainMode()
    {
        // Basic particle lifetime and capacity
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 2f;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 500;

        // Thin stretched particle
        main.startSize3D = true;
        main.startSizeX = new ParticleSystem.MinMaxCurve(0.1f, 0.1f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(3f, 1f);
        main.startSizeZ = new ParticleSystem.MinMaxCurve(0.1f, 0.1f);

        // High spawn rate
        emission.enabled = true;
        emission.rateOverTime = 250f;

        // Large box spawn
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(100f, 10f, 100f);

        // Strong downward velocity
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(-40f, -20f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        // COLOR BY SPEED
        //colorBySpeed.enabled = true;
        //Gradient g = new Gradient();
        //g.SetKeys(
        //    new GradientColorKey[]
        //    {
        //        new GradientColorKey(Color.white, 0f),
        //        new GradientColorKey(Color.white, 1f)
        //    },
        //    new GradientAlphaKey[]
        //    {
        //        new GradientAlphaKey(0f, 0.00f),
        //        new GradientAlphaKey(1f, 0.15f),
        //        new GradientAlphaKey(1f, 0.85f),
        //        new GradientAlphaKey(0f, 1.00f)
        //    }
        //);
        //colorBySpeed.color = new ParticleSystem.MinMaxGradient(g);
        //colorBySpeed.range = new Vector2(0f, 1f);

        // Disable turbulence
        var noise = ps.noise;
        noise.enabled = false;

        // Allow rain to collide
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.lifetimeLoss = 0.5f;

        // Stretch particles
        psRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        psRenderer.lengthScale = 2f;
        psRenderer.velocityScale = 0f;
        psRenderer.cameraVelocityScale = 0f;
        psRenderer.maxParticleSize = 0.5f;
    }

    /// <summary>
    /// Configures the particle system for snow behavior.
    /// </summary>
    public void SnowMode()
    {
        // Basic particle lifetime and capacity
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 500;

        // Larger particles for snowflaskes
        main.startSize3D = true;
        main.startSizeX = 1.8f;
        main.startSizeY = 1.8f;
        main.startSizeZ = 1.8f;

        // Lower spawn rate
        emission.enabled = true;
        emission.rateOverTime = 150f;

        // Taller spawn box
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.position = new Vector3(0, -20, 0);
        shape.scale = new Vector3(100f, 50f, 100f);

        // Gentle falling
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
        velocity.y = new ParticleSystem.MinMaxCurve(-18.0f, -8.0f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);

        // Add turbulence
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.6f;
        noise.frequency = 0.25f;
        noise.scrollSpeed = 0.2f;
        noise.damping = true;

        // COLOR 
        //colorBySpeed.enabled = false;

        //var colOverLifetime = ps.colorOverLifetime;
        //colOverLifetime.enabled = true;

        //Gradient g = new Gradient();
        //g.SetKeys(
        //    new GradientColorKey[]
        //    {
        //    new GradientColorKey(Color.white, 0f),
        //    new GradientColorKey(Color.white, 1f)
        //    },
        //    new GradientAlphaKey[]
        //    {
        //    new GradientAlphaKey(0f, 0f),
        //    new GradientAlphaKey(1f, 0.2f),
        //    new GradientAlphaKey(1f, 0.8f),
        //    new GradientAlphaKey(0f, 1f)
        //    }
        //);
        //colOverLifetime.color = new ParticleSystem.MinMaxGradient(g);

        // Snow collides softly
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.lifetimeLoss = 1f;
        collision.bounce = 0.1f;

        // Billboard particles
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.maxParticleSize = 1f;
    }
}
