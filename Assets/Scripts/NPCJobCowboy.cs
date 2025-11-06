using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCJobCowboy : NPCJob
{
    [Header("Cowboy settings")]
    [SerializeField]
    private float searchRadius = 60f; // how far to look for cows

    [SerializeField]
    private float followDistance = 1.25f; // stop this close to the cow

    [SerializeField]
    private float reacquireEvery = 2.0f; // seconds between cow re-scans

    [SerializeField]
    private float repathInterval = 0.25f; // seconds between SetDestination refresh

    private NavMeshAgent agent;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();

        // XP tuning
        XPPerTickBonus = 2.0f;
        XPPerTickMultiplier = 1.15f;
    }

    protected override Type WorksiteType => typeof(CowboyWorksite);

    // Provide the active behavior used during Work
    public override BrainTask CreateActiveBehavior()
    {
        // prefer the worksite position as search origin (keeps cowboys at their field)
        var origin = _jobLocation != null ? _jobLocation : transform;
        return new BrainTaskHerdCow(
            origin,
            searchRadius,
            followDistance,
            reacquireEvery,
            repathInterval
        );
    }

    // Called by BrainTaskWorkOnJob at your workSpeed cadence
    public override void ExecuteJob()
    {
        GainXpTick();
    }

    public override void JobFinished() { }
}
