using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TimeOfDayText : MonoBehaviour
{
    TimeOfDay timeOfDay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeOfDay = FindFirstObjectByType<TimeOfDay>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timeOfDay != null)
        {
            int hour = Mathf.FloorToInt(timeOfDay.GetCurrentHour());
            int minute = Mathf.FloorToInt((timeOfDay.GetCurrentHour() - hour) * 60f);
            string timeString = $"{hour:00}:{minute:00}";
            GetComponent<TextMeshProUGUI>().text = timeString;
        }
    }
}
