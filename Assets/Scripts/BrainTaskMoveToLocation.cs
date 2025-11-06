using UnityEngine;
using UnityEngine.AI;

public class BrainTaskMoveToLocation : BrainTask
{
    private readonly Vector3 target;
    private readonly float arriveRadius;
    private readonly float repathInterval;

    // NEW: optional spread + stuck recovery
    private readonly float spreadRadius; // meters to jitter around target to avoid dog-piling
    private float stuckTimer;
    private Vector3 currentGoal;

    private NavMeshAgent agent;
    private float tSinceSet;

    // original 3-arg ctor (kept for compatibility)
    public BrainTaskMoveToLocation(
        Vector3 target,
        float arriveRadius = 0.5f,
        float repathInterval = 1.0f
    )
    {
        this.target = target;
        this.arriveRadius = Mathf.Max(0.01f, arriveRadius);
        this.repathInterval = Mathf.Max(0.05f, repathInterval);
        this.spreadRadius = 0f; // no jitter
    }

    // NEW 4-arg ctor (with spread)
    public BrainTaskMoveToLocation(
        Vector3 target,
        float arriveRadius,
        float repathInterval,
        float spreadRadius
    )
    {
        this.target = target;
        this.arriveRadius = Mathf.Max(0.01f, arriveRadius);
        this.repathInterval = Mathf.Max(0.05f, repathInterval);
        this.spreadRadius = Mathf.Max(0f, spreadRadius);
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

        if (agent.stoppingDistance < arriveRadius * 0.5f)
            agent.stoppingDistance = arriveRadius * 0.5f;

        // pick initial goal (optionally jittered + snapped to navmesh)
        currentGoal = SnapToNav(spreadRadius > 0f ? Jitter(target, spreadRadius) : target);

        // if we're already there, finish instantly
        float startDist = Vector3.Distance(brain.transform.position, currentGoal);
        if (startDist <= Mathf.Max(arriveRadius, agent.stoppingDistance))
        {
            Complete();
            return;
        }

        agent.SetDestination(currentGoal);
        tSinceSet = 0f;
        stuckTimer = 0f;
    }

    public override void UpdateTask(float dt)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            Complete();
            return;
        }

        // re-issue destination periodically
        tSinceSet += dt;
        if (tSinceSet >= repathInterval && !agent.pathPending)
        {
            agent.SetDestination(currentGoal);
            tSinceSet = 0f;
        }

        float threshold = Mathf.Max(arriveRadius, agent.stoppingDistance);

        if (!agent.pathPending)
        {
            // arrived
            if (
                agent.remainingDistance <= threshold
                && (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
            )
            {
                Complete();
                return;
            }

            // simple stuck detection → pick a new jittered goal
            if (agent.velocity.sqrMagnitude < 0.0004f) // ≈2 cm/s
            {
                stuckTimer += dt;
                if (stuckTimer >= 1.75f && spreadRadius > 0f)
                {
                    currentGoal = SnapToNav(Jitter(target, spreadRadius));
                    agent.SetDestination(currentGoal);
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }
    }

    public override void Cancel()
    {
        if (agent != null && agent.enabled)
            agent.ResetPath();
        base.Cancel();
    }

    // --- helpers ---

    private static Vector3 Jitter(Vector3 center, float radius)
    {
        // ring-ish distribution (not clustered at center)
        var r = Random.insideUnitCircle.normalized * Random.Range(radius * 0.35f, radius);
        return new Vector3(center.x + r.x, center.y, center.z + r.y);
    }

    private static Vector3 SnapToNav(Vector3 p) =>
        NavMesh.SamplePosition(p, out var hit, 3f, NavMesh.AllAreas) ? hit.position : p;
}
