using System;
using UnityEngine;

/// <summary>
/// Task that makes the NPC sleep until awakened.
/// </summary>
public class BrainTaskSleep : BrainTask
{
    public override void StartTask(NPCBrain brain)
    {
        base.StartTask(brain);
    }

    public override void UpdateTask(float deltaTime)
    {
        // no per-frame work required for this simple task;
        // task completion is driven by external wake-up calls.
    }

    public override void Cancel()
    {
        base.Cancel();
    }
}
