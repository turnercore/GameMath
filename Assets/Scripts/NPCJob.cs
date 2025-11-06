using System;
using GameMath.Demo;
using UnityEngine;

// make this an abstract base so it cannot be attached directly in the Inspector
public abstract class NPCJob : MonoBehaviour
{
    public int CurrentJobXP = 0;
    public int JobLevel = 1;

    public float XPPerTickMultiplier = 1.0f;
    public float XPPerTickBonus = 0.0f;

    [SerializeField]
    private LevelData[] levelData;

    protected Transform _jobLocation;

    //Reference to the specvific worksite child class that we are looking for, we'll find the closest one
    protected abstract Type WorksiteType { get; } // <- property instead of field

    protected virtual void Awake()
    {
        // Initialization logic common to all jobs can go here
    }

    // optional common initialization for all jobs
    protected virtual void Start() { }

    // optional common update logic for all jobs
    protected virtual void Update() { }

    // required hook for derived classes to implement the job behavior, should be callable by Brains
    public abstract void ExecuteJob();

    public Transform GetJobLocation()
    {
        Worksite foundWorksite = FindClosestWorksite();
        if (foundWorksite != null)
        {
            _jobLocation = foundWorksite.transform;
            return _jobLocation;
        }
        else
        {
            Debug.LogError($"{name}: No valid worksite of type {WorksiteType} found!");
            return null;
        }
    }

    public Worksite FindClosestWorksite()
    {
        // guard: must be a Worksite-derived type
        if (WorksiteType == null || !typeof(Worksite).IsAssignableFrom(WorksiteType))
            return null;

        // Non-generic lookup by runtime Type
        UnityEngine.Object[] found = FindObjectsByType(
            WorksiteType,
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        if (found == null || found.Length == 0)
            return null;

        Vector3 me = transform.position;
        Worksite closest = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < found.Length; i++)
        {
            // each is UnityEngine.Object; cast up to Worksite
            var ws = found[i] as Worksite;
            if (ws == null)
                continue;

            float d2 = (ws.transform.position - me).sqrMagnitude;
            if (d2 < bestSqr)
            {
                bestSqr = d2;
                closest = ws;
            }
        }

        return closest;
    }

    protected virtual void GainXpTick()
    {
        // Add XP
        if (levelData == null || levelData.Length < JobLevel)
            return;

        int xpThisTick = Mathf.RoundToInt(
            levelData[JobLevel - 1].xpPerTick * XPPerTickMultiplier + XPPerTickBonus
        );

        CurrentJobXP += xpThisTick;
        // Check for level up
        if (CurrentJobXP >= levelData[JobLevel - 1].xpNeededForNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        JobLevel++;
        Debug.Log($"{name} leveled up to level {JobLevel} in job!");
        // Reset XP for the new level
        CurrentJobXP = 0;
    }

    public abstract void JobFinished();

    /// Return a behavior to run during Work (optional).
    public virtual BrainTask CreateActiveBehavior() => null;

    /// Default work task: tick job + run optional behavior.
    public virtual BrainTask CreateWorkTask(float workSpeed) =>
        new BrainTaskWorkOnJob(this, workSpeed, CreateActiveBehavior());
}
