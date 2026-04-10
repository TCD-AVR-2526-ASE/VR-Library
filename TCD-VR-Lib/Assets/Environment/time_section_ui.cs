using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class time_section_ui : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider slider_time;
    [SerializeField] private TMP_Text TimeFollowText;
    [SerializeField] private WeatherChange weatherSystem;
    [SerializeField] private float manualOverrideDuration = 0.15f;

    private float manualOverrideUntil = 0f;

    private void Start()
    {
        if (slider_time != null)
        {
            slider_time.onValueChanged.AddListener(OnSliderValueChanged);
        }

        SyncFromWorldTime();
    }

    private void Update()
    {
        if (slider_time == null || WorldTime.Instance == null)
            return;

        if (Time.unscaledTime < manualOverrideUntil)
            return;

        SyncFromWorldTime();
    }

    public void OnPointerUp()
    {
        manualOverrideUntil = 0f;
    }

    private void OnSliderValueChanged(float value)
    {
        manualOverrideUntil = Time.unscaledTime + manualOverrideDuration;
        UpdateTimeText(value);
    }

    private void SyncFromWorldTime()
    {
        if (slider_time == null || WorldTime.Instance == null)
            return;

        float minutes = WorldTime.Instance.GetTime() * 60f;
        slider_time.SetValueWithoutNotify(minutes);
        UpdateTimeText(minutes, false);
    }

    private void UpdateTimeText(float value, bool updateWorldTime = true)
    {
        int totalMinutes = Mathf.RoundToInt(value);
        totalMinutes = Mathf.Clamp(totalMinutes, 0, 1440);

        if (updateWorldTime && WorldTime.Instance != null)
        {
            WorldTime.Instance.SetTime(totalMinutes / 60f);

            if (weatherSystem != null)
                weatherSystem.OnTimeManuallySet();
        }

        if (TimeFollowText == null)
            return;

        if (totalMinutes == 1440)
        {
            TimeFollowText.text = "24:00";
            return;
        }

        int hour = totalMinutes / 60;
        int minute = totalMinutes % 60;

        TimeFollowText.text = $"{hour:00}:{minute:00}";
    }
}
