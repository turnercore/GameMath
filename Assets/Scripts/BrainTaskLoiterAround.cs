using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Walks to random points on a ring around 'center', waits briefly, repeats.
/// Great for "patrol the building" / "loiter around market".
/// </summary>
public class BrainTaskLoiterAround : BrainTask
{
    private readonly Vector3 center;
    private readonly float radius;
    private readonly float dwell;
    private readonly float repathInterval;

    private NavMeshAgent agent;
    private Vector3 goal;
    private float dwellTimer;

    private enum State
    {
        Moving,
        Dwelling,
    }

    private State state;

    public BrainTaskLoiterAround(
        Vector3 center,
        float radius,
        float dwellSeconds,
        float repathInterval = 1.0f
    )
    {
        this.center = center;
        this.radius = Mathf.Max(0.5f, radius);
        this.dwell = Mathf.Max(0f, dwellSeconds);
        this.repathInterval = Mathf.Max(0.05f, repathInterval);
    }

    public override void StartTask(NPCBrain brain)
    {
        base.StartTask(brain);
        agent = brain.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            Complete();
            return;
        }

        PickNewGoal();
        state = State.Moving;
        dwellTimer = 0f;
    }

    public override void UpdateTask(float dt)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            Complete();
            return;
        }

        switch (state)
        {
            case State.Moving:
                if (
                    !agent.pathPending
                    && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.3f)
                    && (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                )
                {
                    state = State.Dwelling;
                    dwellTimer = 0f;
                }
                break;

            case State.Dwelling:
                dwellTimer += dt;
                if (dwellTimer >= dwell)
                {
                    PickNewGoal();
                    state = State.Moving;
                }
                break;
        }
    }

    public override void Cancel()
    {
        if (agent != null && agent.enabled)
            agent.ResetPath();
        base.Cancel();
    }

    private void PickNewGoal()
    {
        Vector2 r2 = Random.insideUnitCircle.normalized * Random.Range(radius * 0.5f, radius);
        Vector3 candidate = new Vector3(center.x + r2.x, center.y, center.z + r2.y);

        // snap to navmesh; if fail, try a few times then fallback to center
        for (int i = 0; i < 5; i++)
        {
            if (NavMesh.SamplePosition(candidate, out var hit, 2.5f, NavMesh.AllAreas))
            {
                goal = hit.position;
                agent.SetDestination(goal);
                return;
            }
            r2 = Random.insideUnitCircle.normalized * Random.Range(radius * 0.5f, radius);
            candidate = new Vector3(center.x + r2.x, center.y, center.z + r2.y);
        }

        goal = center;
        agent.SetDestination(goal);
    }
}
