using UnityEngine;

/// <summary>
/// Lightweight task abstraction for NPCBrains.
/// Not a MonoBehaviour — manage lifetime from the brain.
/// </summary>
public abstract class BrainTask
{
    protected NPCBrain brain;
    public bool IsCompleted { get; protected set; }

    public virtual void StartTask(NPCBrain brain)
    {
        this.brain = brain;
        IsCompleted = false;
    }

    public abstract void UpdateTask(float deltaTime);

    public virtual void Cancel()
    {
        IsCompleted = true;
    }
}
