using UnityEngine;

/// <summary>
/// Simple job worker brain derived from the generic NPCBrain.
/// This brain type is intended for NPCs that perform jobs rather than move around.
/// </summary>
public class NPCBrainJobWorker : NPCBrain
{
    private Vector3 homePosition;

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
        // Set home position to spawn position
        homePosition = transform.position;
        // Find the TimeOfDay instance in the scene
        time = FindFirstObjectByType<TimeOfDay>();
        if (time == null)
        {
            Debug.LogError("TimeOfDay instance not found in the scene.");
        }
        // Set default tasks to move randomly if they are not set
        if (afterHoursTask == null)
            afterHoursTask = new BrainTaskMoveToLocation(homePosition);
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
                if (CurrentTask != null)
                {
                    CurrentTask.Cancel();
                }
                if (workJob != null)
                {
                    // Assign task to go to the location, and arrival handler to excute the job
                    BrainTaskMoveToLocation moveToJobLocationTask = new BrainTaskMoveToLocation(
                        jobLocation.position
                    );
                    moveToJobLocationTask.StartTask(this);
                    CurrentTask = moveToJobLocationTask;
                    CurrentTask.OnCompleted += () =>
                    {
                        workJob.ExecuteJob();
                    };
                }
            }
            else if (currentHour > workHours.y)
            {
                // After work hours
                if (CurrentTask != afterHoursTask)
                {
                    if (CurrentTask != null)
                    {
                        CurrentTask.Cancel();
                    }
                    afterHoursTask.StartTask(this);
                    CurrentTask = afterHoursTask;
                }
            }
            else if (currentHour < workHours.x)
            {
                // Before work hours
                if (CurrentTask != beforeHoursTask)
                {
                    if (CurrentTask != null)
                    {
                        CurrentTask.Cancel();
                    }
                    beforeHoursTask.StartTask(this);
                    CurrentTask = beforeHoursTask;
                }
            }
        }

        // tick current task
        if (CurrentTask != null && !CurrentTask.IsCompleted)
        {
            CurrentTask.UpdateTask(Time.deltaTime);
        }
    }

    //Default task of move around randomly if no other task is assigned
    void AssignNewRandomMoveTask()
    {
        // create task and start it
        CurrentTask = new BrainTaskRandomMove(minDistance, maxDistance);
        CurrentTask.StartTask(this);
        Debug.Log($"{name}: Assigned RandomMoveTask (min {minDistance} max {maxDistance})");
    }
}
