using UnityEngine;

public class TimeOfDay : MonoBehaviour
{
    public float timeScale = 1.0f; // speed multiplier for time progression
    public float currentTime = 0.0f; // current time in hours (0-24)
    public Light directionalLight; // reference to the sun light

    public float currentDay = 0.0f; // current day count

    // Update is called once per frame
    void Update()
    {
        // Advance time
        currentTime += Time.deltaTime * timeScale / 3600.0f; // convert seconds to hours
        if (currentTime >= 24.0f)
        {
            currentTime -= 24.0f; // wrap around after 24 hours
            currentDay += 1.0f; // increment day count
            Debug.Log("New day: " + currentDay);
        }

        // Update sun position based on time of day
        if (directionalLight != null)
        {
            float sunAngle = (currentTime / 24.0f) * 360.0f - 90.0f; // -90 to start at sunrise
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170.0f, 0.0f);
        }
    }

    public float GetCurrentHour()
    {
        return currentTime;
    }

    public float GetCurrentDay()
    {
        return currentDay;
    }

    public void SetTimeOfDay(float hour)
    {
        currentTime = hour % 24.0f;
    }

    public void SetTimeScale(float scale)
    {
        timeScale = scale;
    }

    public float GetTimeScale()
    {
        return timeScale;
    }

    public bool IsDaytime()
    {
        return currentTime >= 6.0f && currentTime < 18.0f; // simple day/night check
    }

    public bool IsNighttime()
    {
        return !IsDaytime();
    }

    public float GetNormalizedTime()
    {
        return currentTime / 24.0f; // returns time as a value between 0 and 1
    }

    public void ToggleDayNight()
    {
        if (IsDaytime())
        {
            SetTimeOfDay(20.0f); // switch to night
        }
        else
        {
            SetTimeOfDay(8.0f); // switch to day
        }
    }

    public float GetSunElevationAngle()
    {
        return directionalLight.transform.rotation.eulerAngles.x;
    }
}
