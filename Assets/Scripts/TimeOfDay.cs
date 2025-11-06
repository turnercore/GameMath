using UnityEngine;

public class TimeOfDay : MonoBehaviour
{
    [Tooltip("Real-world seconds per full in-game day (24h).")]
    public float secondsPerDay = 86400f; // default = 24 real hours

    public float currentTime = 0f; // in hours (0-24)
    public float currentDay = 0f;

    public Light directionalLight;

    void Update()
    {
        // advance time based on "seconds per day"
        float dayFractionPerSecond = 1f / secondsPerDay;
        currentTime += Time.deltaTime * 24f * dayFractionPerSecond;

        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            currentDay++;
            Debug.Log($"New day: {currentDay}");
        }

        // rotate sun
        if (directionalLight != null)
        {
            float sunAngle = (currentTime / 24f) * 360f - 90f;
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        }
    }

    public float GetCurrentHour() => currentTime;

    public float GetCurrentDay() => currentDay;

    public void SetTimeOfDay(float hour) => currentTime = hour % 24f;

    public void SetSecondsPerDay(float seconds) => secondsPerDay = seconds;

    public float GetSecondsPerDay() => secondsPerDay;

    public bool IsDaytime() => currentTime >= 6f && currentTime < 18f;

    public bool IsNighttime() => !IsDaytime();

    public float GetNormalizedTime() => currentTime / 24f;

    public void ToggleDayNight()
    {
        if (IsDaytime())
            SetTimeOfDay(20f);
        else
            SetTimeOfDay(8f);
    }

    public float GetSunElevationAngle() => directionalLight.transform.rotation.eulerAngles.x;
}
