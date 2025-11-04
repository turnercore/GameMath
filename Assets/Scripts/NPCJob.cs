using UnityEngine;

// make this an abstract base so it cannot be attached directly in the Inspector
public abstract class NPCJob : MonoBehaviour
{
    // optional common initialization for all jobs
    protected virtual void Start() { }

    // optional common update logic for all jobs
    protected virtual void Update() { }

    // required hook for derived classes to implement the job behavior, should be callable by Brains
    public abstract void ExecuteJob();
}
