using UnityEngine;

/// <summary>
/// Ticks job.ExecuteJob() every workSpeed seconds,
/// and (optionally) runs a child behavior task in parallel (loiter/herd/etc).
/// </summary>
public class BrainTaskWorkOnJob : BrainTask
{
    private readonly NPCJob job;
    private readonly float workSpeed;
    private readonly BrainTask behavior; // optional child task

    private float timer;
    private bool running = true;

    //expose publicly CurrentBehavior for reading and debugging only
    public BrainTask CurrentBehavior => behavior;

    public BrainTaskWorkOnJob(NPCJob job, float workSpeed, BrainTask behavior = null)
    {
        this.job = job;
        this.workSpeed = Mathf.Max(0.01f, workSpeed);
        this.behavior = behavior;
    }

    public override void StartTask(NPCBrain brain)
    {
        base.StartTask(brain);
        timer = 0f;
        running = true;

        if (behavior != null)
            behavior.StartTask(brain);
    }

    public override void UpdateTask(float deltaTime)
    {
        if (!running)
            return;

        // Drive the optional behavior
        if (behavior != null && !behavior.IsCompleted)
            behavior.UpdateTask(deltaTime);

        // Tick the job
        timer += deltaTime;
        if (timer >= workSpeed)
        {
            job.ExecuteJob(); // should call GainXpTick() inside
            timer = 0f;
        }
    }

    public override void Cancel()
    {
        running = false;
        if (behavior != null)
            behavior.Cancel();
        base.Cancel();
        job.JobFinished();
    }
}
