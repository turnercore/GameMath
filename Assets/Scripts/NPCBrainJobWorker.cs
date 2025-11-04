using UnityEngine;

/// <summary>
/// Simple job worker brain derived from the generic NPCBrain.
/// This brain type is intended for NPCs that perform jobs rather than move around.
/// </summary>
public class NPCBrainJobWorker : NPCBrain
{
    private Vector3 homePosition;

    private BrainTask currentTask;

    [Header("Random Mover Settings")]
    public float minDistance = 5f;
    public float maxDistance = 20f;

    [Header("Work Settings")]
    public float workSpeed = 1f;
    public Transform jobLocation;
    public Vector2 workHours = new(8f, 17f);
    public NPCJob workJob;
    public BrainTask afterHoursTask;
    public BrainTask beforeHoursTask;

    //get day/night cycle from GameManager
    private TimeOfDay time;

    private new void Start()
    {
        base.Start();
        // Find the TimeOfDay instance in the scene
        time = FindFirstObjectByType<TimeOfDay>();
        if (time == null)
        {
            Debug.LogError("TimeOfDay instance not found in the scene.");
        }
        // Set default tasks to move randomly if they are not set
        if (afterHoursTask == null)
            afterHoursTask = new BrainTaskRandomMove(minDistance, maxDistance);
        if (beforeHoursTask == null)
            beforeHoursTask = new BrainTaskRandomMove(minDistance, maxDistance);
        if (workJob == null)
            Debug.LogError($"{name}: workJob is not assigned!");
    }

    // Tick current tasks, if the time of day is within work hours cancel current task and go to work, if time of day is after work hours excute after hours task if before work hours make sure our current task is before horus task, etc
    protected override void Update()
    {
        base.Update();

        if (time != null)
        {
            float currentHour = time.GetCurrentHour();

            if (currentHour >= workHours.x && currentHour <= workHours.y)
            {
                // Within work hours
                if (currentTask != null)
                {
                    currentTask.Cancel();
                }
                if (workJob != null)
                {
                    workJob.ExecuteJob();
                }
            }
            else if (currentHour > workHours.y)
            {
                // After work hours
                if (currentTask != afterHoursTask)
                {
                    if (currentTask != null)
                    {
                        currentTask.Cancel();
                    }
                    afterHoursTask.StartTask(this);
                    currentTask = afterHoursTask;
                }
            }
            else if (currentHour < workHours.x)
            {
                // Before work hours
                if (currentTask != beforeHoursTask)
                {
                    if (currentTask != null)
                    {
                        currentTask.Cancel();
                    }
                    beforeHoursTask.StartTask(this);
                    currentTask = beforeHoursTask;
                }
            }
        }

        // tick current task
        if (currentTask != null && !currentTask.IsCompleted)
        {
            currentTask.UpdateTask(Time.deltaTime);
        }
    }

    //Default task of move around randomly if no other task is assigned
    void AssignNewRandomMoveTask()
    {
        // create task and start it
        currentTask = new BrainTaskRandomMove(minDistance, maxDistance);
        currentTask.StartTask(this);
        Debug.Log($"{name}: Assigned RandomMoveTask (min {minDistance} max {maxDistance})");
    }
}
