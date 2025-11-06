// ...existing code...
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class NPCBrain : MonoBehaviour
{
    [Header("Agent")]
    public NavMeshAgent agent;
    public bool autoStart = false; // base does not choose destinations automatically

    [Header("Rotation")]
    [Tooltip(
        "If true let NavMeshAgent rotate the GameObject root. If false the visual model (modelTransform) will be rotated."
    )]
    public bool useAgentRotation = true;

    [Tooltip("Visual model to rotate when agent is NOT rotating the root.")]
    public Transform modelTransform;

    [Tooltip("If your model faces -Z, set true to invert the look direction.")]
    public bool invertModelForward = true;

    [Tooltip("Degrees/sec used when rotating the visual model.")]
    public float modelRotateSpeed = 360f;

    [Header("Arrival")]
    [Tooltip("Optional extra threshold to consider 'arrived' in world units.")]
    public float arrivalThreshold = 0.0f;

    // Essential state
    protected Vector3 destination;
    public Vector3 Destination => destination;

    protected bool _hasActiveDestination = false;
    public bool HasActiveDestination => _hasActiveDestination;

    public string CurrentState
    {
        get
        {
            if (CurrentTask != null)
            {
                return CurrentTask.GetType().Name;
            }
            else
            {
                return "Idle";
            }
        }
    }

    protected bool _arrivalReported = false;

    // Event raised when an arrival is detected. Derived classes / systems subscribe or override OnArrival().
    public event Action OnArrived;

    private BrainTask _currentTask;

    public BrainTask CurrentTask
    {
        get => _currentTask;
        set
        {
            // Clean up previous task
            if (_currentTask != null)
            {
                _currentTask.Dispose();
            }
            _currentTask = value;
        }
    }

    public int MyLevel => GetLevel();
    public int MyXP => GetXP();

    protected virtual int GetLevel()
    {
        return -1;
    }

    protected virtual int GetXP()
    {
        return -1;
    }

    protected virtual void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        // configure agent rotation wrt modelTransform
        agent.updateRotation = useAgentRotation && modelTransform == null;
        if (agent.angularSpeed <= 0f)
            agent.angularSpeed = 120f;

        // base does not pick destinations; derived classes or external systems control that.
        if (autoStart)
        {
            // optional: derived classes can override Start and call SetAgentDestinationReliably themselves
        }
    }

    /// <summary>Virtual arrival hook — does nothing by default. Override in derived classes to react.</summary>
    protected virtual void OnArrival()
    {
        // intentionally empty
    }

    /// <summary>Reliable wrapper around NavMeshAgent.SetDestination. Sets internal state but does NOT compute a next destination.</summary>
    public virtual bool SetAgentDestinationReliably(Vector3 dest, int maxAttempts = 10)
    {
        destination = dest;
        _arrivalReported = false;

        bool destinationSet = false;
        for (int i = 0; i < maxAttempts; i++)
        {
            if (agent.SetDestination(dest))
            {
                destinationSet = true;
                break;
            }
        }

        // Always mark the destination as active when requested so the generic arrival checks will consider it.
        // If SetDestination succeeded we'll also ensure the agent is running.
        agent.isStopped = false;
        _hasActiveDestination = true;

        if (!destinationSet)
        {
            // destination didn't produce a path immediately — fallback checks in Update will use direct distance.
            Debug.LogWarning(
                $"{name}: SetDestination did not return true; fallback arrival distance will be used."
            );
        }

        return destinationSet;
    }

    /// <summary>Force destination and mark as active even if SetDestination didn't return true.</summary>
    public virtual void ForceDestination(Vector3 dest)
    {
        destination = dest;
        _hasActiveDestination = true;
        _arrivalReported = false;
        agent.isStopped = false;
        agent.SetDestination(dest);
    }

    public virtual void StopMovement()
    {
        agent.isStopped = true;
        _hasActiveDestination = false;
    }

    public virtual void ResumeMovement()
    {
        agent.isStopped = false;
        if (agent.hasPath)
            _hasActiveDestination = true;
    }

    protected virtual void Update()
    {
        // rotate visual model if assigned and agent is not rotating the root
        if (modelTransform != null && agent != null && !agent.updateRotation)
        {
            Vector3 vel = agent.velocity;
            vel.y = 0f;
            if (vel.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = invertModelForward ? -vel.normalized : vel.normalized;
                Quaternion target = Quaternion.LookRotation(dir);
                modelTransform.rotation = Quaternion.RotateTowards(
                    modelTransform.rotation,
                    target,
                    modelRotateSpeed * Time.deltaTime
                );
            }
        }

        if (agent == null)
            return;

        // robust arrival detection:
        // prefer remainingDistance when available, handle PathComplete/Partial/Invalid, and fallback to direct distance
        float threshold = Mathf.Max(agent.stoppingDistance, arrivalThreshold);
        bool arrived = false;

        // If the agent is still building the path, wait a frame — but still allow fallback checks when appropriate
        if (!agent.pathPending)
        {
            float remaining = agent.remainingDistance;
            var status = agent.pathStatus;

            if (status == NavMeshPathStatus.PathComplete)
            {
                // normal case: use remainingDistance
                if (!float.IsInfinity(remaining) && remaining <= threshold)
                    arrived = true;
            }
            else if (status == NavMeshPathStatus.PathPartial)
            {
                // partial paths can still get us close enough
                if (!float.IsInfinity(remaining) && remaining <= threshold)
                    arrived = true;
            }
            else // PathInvalid
            {
                // no nav path — use direct distance fallback when a destination was requested
                if (_hasActiveDestination)
                {
                    float dist = Vector3.Distance(transform.position, destination);
                    if (dist <= Mathf.Max(threshold, 0.1f))
                        arrived = true;
                }
            }

            // Extra conservative check: if agent has essentially stopped and remainingDistance is small treat as arrived
            if (!arrived && _hasActiveDestination && agent.velocity.sqrMagnitude < 0.01f)
            {
                if (!float.IsInfinity(remaining) && remaining <= Mathf.Max(threshold + 0.1f, 0.1f))
                    arrived = true;
            }
        }

        if (arrived && !_arrivalReported)
        {
            _arrivalReported = true;
            _hasActiveDestination = false;
            OnArrived?.Invoke();
            OnArrival();
        }
    }
}
// ...existing code...
