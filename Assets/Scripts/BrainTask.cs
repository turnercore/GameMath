using System;

/// <summary>
/// Lightweight task abstraction for NPCBrains.
/// Not a MonoBehaviour — manage lifetime from the brain.
/// </summary>
public abstract class BrainTask : ICloneable
{
    protected NPCBrain brain;
    public bool IsCompleted { get; protected set; }
    public event System.Action OnCompleted;
    public event System.Action OnCancelled;

    public virtual void Dispose()
    {
        OnCompleted = null;
        OnCancelled = null;
    }

    public virtual void StartTask(NPCBrain brain)
    {
        this.brain = brain;
        IsCompleted = false;
    }

    public abstract void UpdateTask(float deltaTime);

    public virtual void Cancel()
    {
        IsCompleted = true;
        OnCancelled?.Invoke();
        Dispose();
    }

    public virtual void Complete()
    {
        IsCompleted = true;
        OnCompleted?.Invoke();
        Dispose();
    }

    public virtual object Clone() => MemberwiseClone();
}
