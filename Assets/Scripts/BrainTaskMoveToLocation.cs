using System;
using UnityEngine;

/// <summary>
///
/// Task that moves the NPC to a specified location and completes when the NPC arrives.
/// </summary>
public class BrainTaskMoveToLocation : BrainTask
{
    private readonly Vector3 targetLocation;
    private Action arrivalHandler;

    //Constructor
    public BrainTaskMoveToLocation(Vector3 targetLocation)
    {
        this.targetLocation = targetLocation;
    }

    public override void StartTask(NPCBrain brain)
    {
        base.StartTask(brain);
        arrivalHandler = OnArrived;
        brain.OnArrived += arrivalHandler;
        bool ok = brain.SetAgentDestinationReliably(targetLocation);
        if (!ok)
            brain.ForceDestination(targetLocation);
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

    void OnArrived()
    {
        if (brain != null && arrivalHandler != null)
            brain.OnArrived -= arrivalHandler;
        Complete();
    }
}
