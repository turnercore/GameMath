using UnityEngine;

public class TimeOfDay : MonoBehaviour
{
    [Tooltip("Real-world seconds per full in-game day (24h).")]
    public float secondsPerDay = 86400f;

    [Tooltip("Maximum sunlight intensity at noon.")]
    public float maxSunIntensity = 1.0f;

    [Tooltip("Angle (in degrees) where the sun fades out near the horizon.")]
    public float fadeRangeDegrees = 10f;

    public float currentTime = 0f; // hours (0–24)
    public float currentDay = 0f;

    public Light directionalLight;

    void Update()
    {
        // Advance time
        float dayFractionPerSecond = 1f / secondsPerDay;
        currentTime += Time.deltaTime * 24f * dayFractionPerSecond;

        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            currentDay++;
        }

        // Compute rotation: 0 = midnight, 12 = noon
        float sunAngle = (currentTime / 24f) * 360f - 90f;
        float clampedAngle = Mathf.Max(sunAngle, -fadeRangeDegrees); // stops before going below horizon
        directionalLight.transform.rotation = Quaternion.Euler(clampedAngle, 170f, 0f);

        // Fade intensity near horizon
        float fadeT = Mathf.InverseLerp(-fadeRangeDegrees, 0f, sunAngle);
        directionalLight.intensity = Mathf.Lerp(0f, maxSunIntensity, fadeT);
    }

    // --- API ---
    public float GetCurrentHour() => currentTime;

    public float GetCurrentDay() => currentDay;

    public void SetTimeOfDay(float hour) => currentTime = hour % 24f;

    public bool IsDaytime() => currentTime >= 6f && currentTime < 18f;

    public bool IsNighttime() => !IsDaytime();

    public float GetNormalizedTime() => currentTime / 24f;
}
