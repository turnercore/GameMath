using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NPCJob))]
public class NPCBrainJobWorker : NPCBrain
{
    [Tooltip("Spread around job arrival to avoid dog-piling.")]
    public float jobArrivalSpread = 2.5f;

    [Tooltip("Spread around home arrival if you want it too.")]
    public float homeArrivalSpread = 0.0f; // usually 0

    public enum PhaseKind
    {
        Sleep,
        PreWork,
        Work,
        Home,
    }

    [Serializable]
    public class PhaseConfig
    {
        public PhaseKind kind = PhaseKind.Sleep;

        [Range(0, 24)]
        public float startHour = 22f;

        [Range(0, 24)]
        public float endHour = 6f;

        [SerializeReference]
        public BrainTask taskPrototype; // optional override for Sleep/PreWork/Home
    }

    [Header("Schedule (in order)")]
    public List<PhaseConfig> schedule = new();

    [Header("General")]
    public float workSpeed = 1f;

    [Tooltip("How close is 'arrived' at job.")]
    public float jobArriveRadius = 1.5f;

    [Tooltip("How close is 'arrived' at home.")]
    public float homeArriveRadius = 1.5f;

    [Header("Optional default tasks")]
    [SerializeReference]
    public BrainTask defaultPreWork;

    [SerializeReference]
    public BrainTask defaultHome;

    [SerializeReference]
    public BrainTask defaultSleep;

    [Header("Job")]
    private NPCJob Job;

    private int _phaseIndex = -1;
    private PhaseConfig _currentPhase;
    private TimeOfDay _time;
    private Vector3 _homePosition;
    private Action _onDoneOnce;

    private Transform JobLocation => Job != null ? Job.GetJobLocation() : null;

    protected override void Awake()
    {
        base.Awake();
        Job = GetComponent<NPCJob>();
        if (Job == null)
            Debug.LogError($"{name}: NPCJob component not found!");
    }

    private new void Start()
    {
        base.Start();
        _homePosition = transform.position;

        _time = FindFirstObjectByType<TimeOfDay>();
        if (_time == null)
            Debug.LogError("TimeOfDay instance not found.");

        // Default schedule: no commute phases — commute is part of Work/Home chains
        if (schedule == null || schedule.Count == 0)
        {
            schedule = new List<PhaseConfig>
            {
                new()
                {
                    kind = PhaseKind.Sleep,
                    startHour = 22f,
                    endHour = 6f,
                },
                new()
                {
                    kind = PhaseKind.PreWork,
                    startHour = 6f,
                    endHour = 8f,
                },
                new()
                {
                    kind = PhaseKind.Work,
                    startHour = 8f,
                    endHour = 17f,
                },
                new()
                {
                    kind = PhaseKind.Home,
                    startHour = 17f,
                    endHour = 22f,
                },
            };
        }

        float h = _time != null ? _time.GetCurrentHour() : 12f;
        int startIdx = FindPhaseIndexForHour(h);
        SetPhase(startIdx >= 0 ? startIdx : 0);
    }

    protected override void Update()
    {
        base.Update();

        float h = _time != null ? _time.GetCurrentHour() : 12f;
        int idxByHour = FindPhaseIndexForHour(h);

        // Hard switch by time windows; chains are canceled cleanly in SetPhase
        if (idxByHour >= 0 && idxByHour != _phaseIndex)
            SetPhase(idxByHour);

        if (CurrentTask != null && !CurrentTask.IsCompleted)
            CurrentTask.UpdateTask(Time.deltaTime);
    }

    // ---------------- State control ----------------

    private void SetPhase(int newIndex)
    {
        newIndex = WrapIndex(newIndex);
        if (newIndex == _phaseIndex && _currentPhase != null)
            return;

        // cancel any running chain/task
        if (CurrentTask != null)
        {
            CurrentTask.OnCompleted -= TaskCompletedOnce;
            CurrentTask.Cancel();
        }

        _phaseIndex = newIndex;
        _currentPhase = schedule[_phaseIndex];

        // Build phase task (with commute integrated for Work/Home)
        StartPhaseChain(_currentPhase);

#if UNITY_EDITOR
        Debug.Log(
            $"{name}: Phase -> {_currentPhase.kind} [{_currentPhase.startHour:0.##}-{_currentPhase.endHour:0.##}]"
        );
#endif
    }

    private void StartPhaseChain(PhaseConfig p)
    {
        // explicit override takes full control (rare)
        if (p.taskPrototype != null)
        {
            StartTaskOnce((BrainTask)p.taskPrototype.Clone());
            return;
        }

        switch (p.kind)
        {
            case PhaseKind.PreWork:
                StartTaskOnce(
                    defaultPreWork != null
                        ? (BrainTask)defaultPreWork.Clone()
                        : new BrainTaskRandomMove(5f, 20f)
                );
                break;

            case PhaseKind.Work:
                StartWorkChain();
                break;

            case PhaseKind.Home:
                StartHomeChain();
                break;

            case PhaseKind.Sleep:
                StartTaskOnce(
                    defaultSleep != null ? (BrainTask)defaultSleep.Clone() : new BrainTaskSleep()
                );
                break;

            default:
                StartTaskOnce(new BrainTaskIdle());
                break;
        }
    }

    // Work = (move to job arrival point) → WorkOnJob
    private void StartWorkChain()
    {
        if (Job == null || JobLocation == null)
        {
            StartTaskOnce(new BrainTaskIdle());
            return;
        }

        Vector3 dest = GetJobArrivalPoint();
        var move = new BrainTaskMoveToLocation(dest, jobArriveRadius, 1.0f, jobArrivalSpread);
        StartTaskOnce(
            move,
            onDone: () =>
            {
                // Ask the job what "working" means; default is WorkOnJob
                var workTask =
                    Job.CreateWorkTask(workSpeed) ?? new BrainTaskWorkOnJob(Job, workSpeed);
                StartTaskOnce(workTask);
            }
        );
    }

    // Home = (move to home arrival point) → home task/idle
    private void StartHomeChain()
    {
        Vector3 dest = GetHomeArrivalPoint();
        var move = new BrainTaskMoveToLocation(dest, homeArriveRadius, 1.0f, homeArrivalSpread);
        StartTaskOnce(
            move,
            onDone: () =>
            {
                var after =
                    defaultHome != null ? (BrainTask)defaultHome.Clone() : new BrainTaskIdle();
                StartTaskOnce(after);
            }
        );
    }

    private void StartTaskOnce(BrainTask task, Action onDone = null)
    {
        _onDoneOnce = onDone;
        CurrentTask = task;
        CurrentTask.OnCompleted += TaskCompletedOnce;
        CurrentTask.StartTask(this);
    }

    private void TaskCompletedOnce()
    {
        CurrentTask.OnCompleted -= TaskCompletedOnce;
        var cb = _onDoneOnce;
        _onDoneOnce = null;
        cb?.Invoke();
    }

    // ---------------- Arrival helpers ----------------

    private Vector3 GetJobArrivalPoint()
    {
        if (JobLocation == null)
            return transform.position;

        var ws = JobLocation.GetComponentInParent<Worksite>();
        Vector3 candidate;

        if (ws != null && ws.arrivalAnchor != null)
            candidate = ws.arrivalAnchor.position;
        else
        {
            var col =
                JobLocation.GetComponent<Collider>()
                ?? JobLocation.GetComponentInParent<Collider>();
            candidate = col ? col.ClosestPoint(transform.position) : JobLocation.position;
        }

        return NavMesh.SamplePosition(candidate, out var hit, 3f, NavMesh.AllAreas)
            ? hit.position
            : candidate;
    }

    private Vector3 GetHomeArrivalPoint()
    {
        return NavMesh.SamplePosition(_homePosition, out var hit, 3f, NavMesh.AllAreas)
            ? hit.position
            : _homePosition;
    }

    // ---------------- Helpers ----------------

    private static bool IsWithinWindow(float hour, float start, float end) =>
        (start <= end) ? (hour >= start && hour < end) : (hour >= start || hour < end); // wrap across midnight

    private int FindPhaseIndexForHour(float hour)
    {
        if (schedule == null || schedule.Count == 0)
            return -1;
        for (int i = 0; i < schedule.Count; i++)
            if (IsWithinWindow(hour, schedule[i].startHour, schedule[i].endHour))
                return i;
        return -1;
    }

    private int WrapIndex(int i)
    {
        if (schedule == null || schedule.Count == 0)
            return 0;
        int n = schedule.Count;
        int m = i % n;
        return m < 0 ? m + n : m;
    }

    protected override int GetLevel() => Job != null ? Job.JobLevel : 1;

    protected override int GetXP() => Job != null ? Job.CurrentJobXP : 0;
}
