using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// Smooth perimeter patrol around a worksite's bounds.
public class BrainTaskPatrolPerimeter : BrainTask
{
    private readonly Transform root; // worksite (use collider/render bounds)
    private readonly float margin; // wall clearance
    private readonly bool clockwise; // orbit direction
    private readonly float lookAhead; // how far ahead to chase along the loop
    private readonly float repathInterval; // how often to refresh destination
    private readonly float cornerRadius; // rounding at corners

    private NavMeshAgent agent;
    private readonly List<Vector3> loop = new(); // closed polyline (XZ)
    private readonly List<float> cumulative = new(); // cumulative length
    private float totalLen;
    private float t; // progress along loop in meters
    private float tSinceSet;

    public BrainTaskPatrolPerimeter(
        Transform root,
        float margin = 1.0f,
        bool clockwise = true,
        float lookAhead = 2.5f,
        float repathInterval = 0.2f,
        float cornerRadius = 1.0f
    )
    {
        this.root = root;
        this.margin = Mathf.Max(0f, margin);
        this.clockwise = clockwise;
        this.lookAhead = Mathf.Max(0.5f, lookAhead);
        this.repathInterval = Mathf.Clamp(repathInterval, 0.05f, 1.0f);
        this.cornerRadius = Mathf.Max(0f, cornerRadius);
    }

    public override void StartTask(NPCBrain brain)
    {
        base.StartTask(brain);

        agent = brain.GetComponent<NavMeshAgent>();
        if (agent == null || !agent.enabled || !agent.isOnNavMesh || root == null)
        {
            Complete();
            return;
        }

        // Important for continuous motion
        agent.autoBraking = false;
        agent.updateRotation = true;
        agent.avoidancePriority = Random.Range(30, 70);

        BuildLoop();
        if (loop.Count < 2 || totalLen < 0.1f)
        {
            Complete();
            return;
        }

        // Spread workers out: start each with a different offset on the loop
        int seed = unchecked(brain.GetInstanceID() * 73856093);
        var rand = new System.Random(seed);
        t = (float)rand.NextDouble() * totalLen;

        // Kick the first destination
        tSinceSet = repathInterval;
    }

    public override void UpdateTask(float dt)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            Complete();
            return;
        }

        // Advance progress using current speed (no hard “arrive” checks)
        float speed = Mathf.Max(0.1f, agent.speed * 0.95f);
        t = Wrap(t + (clockwise ? speed : -speed) * dt);

        // Refresh destination to a carrot ahead along the track
        tSinceSet += dt;
        if (tSinceSet >= repathInterval && !agent.pathPending)
        {
            Vector3 carrot = PointAt(Wrap(t + lookAhead));
            if (NavMesh.SamplePosition(carrot, out var hit, 2.0f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(carrot);

            tSinceSet = 0f;
        }
    }

    public override void Cancel()
    {
        if (agent != null && agent.enabled)
            agent.ResetPath();
        base.Cancel();
    }

    // ---------- loop construction & sampling ----------

    private void BuildLoop()
    {
        loop.Clear();
        cumulative.Clear();
        totalLen = 0f;

        // Bounds from collider if possible, else renderer
        Bounds b;
        var col = root.GetComponentInParent<Collider>();
        if (col != null)
            b = col.bounds;
        else
        {
            var r = root.GetComponentInParent<Renderer>();
            b = r != null ? r.bounds : new Bounds(root.position, new Vector3(2, 1, 2));
        }

        float expand = margin + (agent != null ? agent.radius * 1.2f : 0.4f);
        var c = b.center;
        var e = b.extents + new Vector3(expand, 0f, expand);

        // Rectangle corners (XZ)
        Vector3 p0 = new(c.x - e.x, c.y, c.z - e.z);
        Vector3 p1 = new(c.x + e.x, c.y, c.z - e.z);
        Vector3 p2 = new(c.x + e.x, c.y, c.z + e.z);
        Vector3 p3 = new(c.x - e.x, c.y, c.z + e.z);

        // Order based on direction
        var corners = clockwise ? new[] { p0, p1, p2, p3 } : new[] { p0, p3, p2, p1 };

        // Build rounded rectangle with short arcs at corners
        for (int i = 0; i < 4; i++)
        {
            Vector3 a = corners[i];
            Vector3 b1 = corners[(i + 1) & 3];

            // straight segment shortened by corner radii
            Vector3 dir = (b1 - a);
            dir.y = 0f;
            float segLen = dir.magnitude;
            if (segLen < 0.001f)
                continue;
            dir /= segLen;

            float trim = Mathf.Min(cornerRadius, segLen * 0.45f);
            Vector3 A = a + dir * trim;
            Vector3 B = b1 - dir * trim;

            AddPoint(A);
            AddPoint(B);

            // corner arc around b1
            if (cornerRadius > 0.01f)
            {
                // previous and next directions
                Vector3 prevDir = dir;
                Vector3 nextDir = ((corners[(i + 2) & 3] - b1));
                nextDir.y = 0f;
                nextDir.Normalize();
                // rotate prevDir toward nextDir ±90° to get arc center offset
                Vector3 outward = Vector3.Cross(prevDir, Vector3.up).normalized;
                if (!clockwise)
                    outward = -outward;
                Vector3 center = b1 + outward * cornerRadius;

                int arcSteps = 6;
                // start at point near B toward b1, sweep quarter circle
                Vector3 from = B;
                for (int s = 1; s <= arcSteps; s++)
                {
                    float t = s / (float)arcSteps;
                    // interpolate angle around center
                    float ang0 = Mathf.Atan2(from.z - center.z, from.x - center.x);
                    float ang1 = ang0 + (clockwise ? -Mathf.PI * 0.5f : Mathf.PI * 0.5f);
                    float ang = Mathf.Lerp(ang0, ang1, t);
                    Vector3 pt = new(
                        center.x + Mathf.Cos(ang) * cornerRadius,
                        b1.y,
                        center.z + Mathf.Sin(ang) * cornerRadius
                    );
                    AddPoint(pt);
                }
            }
        }

        // Close loop
        if (loop.Count > 1 && (loop[0] - loop[^1]).sqrMagnitude > 0.0004f)
            loop.Add(loop[0]);

        // snap to navmesh & build cumulative length
        for (int i = 0; i < loop.Count; i++)
            if (NavMesh.SamplePosition(loop[i], out var hit, 2.0f, NavMesh.AllAreas))
                loop[i] = hit.position;

        cumulative.Add(0f);
        for (int i = 1; i < loop.Count; i++)
        {
            totalLen += Vector3.Distance(loop[i - 1], loop[i]);
            cumulative.Add(totalLen);
        }
        if (totalLen < 0.1f)
        {
            loop.Clear();
            cumulative.Clear();
        }
    }

    private void AddPoint(Vector3 p)
    {
        loop.Add(new Vector3(p.x, root.position.y, p.z));
    }

    private float Wrap(float d) => (d % totalLen + totalLen) % totalLen;

    private Vector3 PointAt(float dist)
    {
        // binary search segment
        int lo = 0,
            hi = cumulative.Count - 1;
        while (lo + 1 < hi)
        {
            int mid = (lo + hi) >> 1;
            if (cumulative[mid] <= dist)
                lo = mid;
            else
                hi = mid;
        }
        float segStart = cumulative[lo];
        float segLen = Mathf.Max(0.0001f, cumulative[hi] - segStart);
        float tSeg = (dist - segStart) / segLen;
        return Vector3.Lerp(loop[lo], loop[hi], tSeg);
    }
}
