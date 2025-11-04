using System;
using UnityEngine;

/// <summary>
/// Task that issues a single random-move destination to the brain and completes when the brain reports arrival.
/// </summary>
public class BrainTaskRandomMove : BrainTask
{
    private readonly float minDistance;
    private readonly float maxDistance;
    private Action arrivalHandler;

    //Constructor
    public BrainTaskRandomMove(float minDistance, float maxDistance)
    {
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
    }

    public override void StartTask(NPCBrain brain)
    {
        base.StartTask(brain);
        arrivalHandler = OnArrived;
        brain.OnArrived += arrivalHandler;
        PickAndMove();
    }

    public override void UpdateTask(float deltaTime)
    {
        // no per-frame work required for this simple task;
        // task completion is driven by the brain's OnArrived event.
    }

    public override void Cancel()
    {
        if (brain != null && arrivalHandler != null)
            brain.OnArrived -= arrivalHandler;
        base.Cancel();
    }

    void PickAndMove()
    {
        Vector3 dest = GetRandomDestination();
        bool ok = brain.SetAgentDestinationReliably(dest);
        if (!ok)
            brain.ForceDestination(dest);
    }

    Vector3 GetRandomDestination()
    {
        float angle = UnityEngine.Random.Range(0f, 360f);
        float distance = UnityEngine.Random.Range(minDistance, maxDistance);
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        return brain.transform.position + dir * distance;
    }

    void OnArrived()
    {
        if (brain != null && arrivalHandler != null)
            brain.OnArrived -= arrivalHandler;
        IsCompleted = true;
    }
}
