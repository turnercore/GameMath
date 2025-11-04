// ...existing code...
using UnityEngine;

public class NPCBrainRandomMover : NPCBrain
{
    [Header("Random Mover Settings")]
    public float minDistance = 5f;
    public float maxDistance = 20f;

    [Tooltip("If true the random mover will pick and move to an initial destination on Start.")]
    public bool startOnStart = true;

    // current assigned task
    private BrainTask currentTask;

    protected override void Start()
    {
        base.Start();
        if (startOnStart)
            AssignNewRandomMoveTask();
    }

    protected override void Update()
    {
        base.Update();

        // tick current task
        if (currentTask != null && !currentTask.IsCompleted)
        {
            currentTask.UpdateTask(Time.deltaTime);
        }

        // if task finished (or null) assign next one
        if (currentTask == null || currentTask.IsCompleted)
        {
            AssignNewRandomMoveTask();
        }
    }

    void AssignNewRandomMoveTask()
    {
        // create task and start it
        currentTask = new BrainTaskRandomMove(minDistance, maxDistance);
        currentTask.StartTask(this);
        Debug.Log($"{name}: Assigned RandomMoveTask (min {minDistance} max {maxDistance})");
    }

    // keep the previous override if you still want a log on arrival (optional)
    protected override void OnArrival()
    {
        base.OnArrival();
        Debug.Log($"{name} arrived at destination {destination}");
    }
}
// ...existing code...
