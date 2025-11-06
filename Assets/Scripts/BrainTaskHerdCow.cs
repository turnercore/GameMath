using UnityEngine;
using UnityEngine.AI;

public class BrainTaskHerdCow : BrainTask
{
    private readonly Transform searchOrigin;
    private readonly float searchRadius;
    private readonly float followDistance;
    private readonly float reacquireEvery;
    private readonly float repathInterval;

    private NavMeshAgent agent;
    private Cow target;
    private float tSinceReacquire;
    private float tSinceSetDest;

    public BrainTaskHerdCow(
        Transform searchOrigin,
        float searchRadius,
        float followDistance,
        float reacquireEvery = 2.0f,
        float repathInterval = 0.25f
    )
    {
        this.searchOrigin = searchOrigin;
        this.searchRadius = Mathf.Max(1f, searchRadius);
        this.followDistance = Mathf.Max(0.25f, followDistance);
        this.reacquireEvery = Mathf.Max(0.1f, reacquireEvery);
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

        if (agent.stoppingDistance < followDistance * 0.5f)
            agent.stoppingDistance = followDistance * 0.5f;

        AcquireCow();
        SetGoalToCow();
        tSinceReacquire = 0f;
        tSinceSetDest = 0f;
    }

    public override void UpdateTask(float dt)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            Complete();
            return;
        }

        tSinceReacquire += dt;
        tSinceSetDest += dt;

        // Reacquire periodically or if we lost/strayed from the cow
        if (
            target == null
            || (target.transform.position - searchOrigin.position).sqrMagnitude
                > searchRadius * searchRadius
            || tSinceReacquire >= reacquireEvery
        )
        {
            AcquireCow();
            tSinceReacquire = 0f;
        }

        // Refresh destination toward current cow
        if (tSinceSetDest >= repathInterval && target != null && !agent.pathPending)
        {
            SetGoalToCow();
            tSinceSetDest = 0f;
        }

        // If we’re close enough, slow/stop—NavMeshAgent will keep minimal movement
        if (target != null)
        {
            float d = Vector3.Distance(agent.transform.position, target.transform.position);
            if (d <= Mathf.Max(followDistance, agent.stoppingDistance))
            {
                // soft brake; keep path but tiny velocity
                if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
                    agent.velocity *= 0.5f;
            }
        }
    }

    public override void Cancel()
    {
        if (agent != null && agent.enabled)
            agent.ResetPath();
        base.Cancel();
        // Clear cow
        target = null;
    }

    public override void Complete()
    {
        base.Complete();
        // Clear cow
        target = null;
    }

    // --- internals ---

    private void AcquireCow()
    {
        Cow[] cows = Object.FindObjectsByType<Cow>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        if (cows == null || cows.Length == 0)
        {
            target = null;
            return;
        }

        Vector3 origin = searchOrigin != null ? searchOrigin.position : brain.transform.position;
        float best = float.PositiveInfinity;
        Cow bestCow = null;

        for (int i = 0; i < cows.Length; i++)
        {
            float d2 = (cows[i].transform.position - origin).sqrMagnitude;
            if (d2 <= searchRadius * searchRadius && d2 < best)
            {
                best = d2;
                bestCow = cows[i];
            }
        }

        target = bestCow;
    }

    private void SetGoalToCow()
    {
        if (target == null)
            return;

        Vector3 p = target.transform.position;
        // Snap so we don’t feed an off-mesh point
        if (NavMesh.SamplePosition(p, out var hit, 2.0f, NavMesh.AllAreas))
            p = hit.position;

        agent.SetDestination(p);
    }
}
