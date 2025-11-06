using UnityEngine;

public class NPCJobBlackMarketDealer : NPCJob
{
    [SerializeField]
    float margin = 1.2f;

    [SerializeField]
    bool clockwise = true;

    [SerializeField]
    float lookAhead = 3.0f;

    [SerializeField]
    float repathInterval = 0.2f;

    [SerializeField]
    float cornerRadius = 1.0f;

    protected override System.Type WorksiteType => typeof(BlackMarketWorksite);

    public override BrainTask CreateWorkTask(float workSpeed)
    {
        var root = _jobLocation != null ? _jobLocation : transform;
        return new BrainTaskPatrolPerimeter(
            root,
            margin: margin,
            clockwise: clockwise,
            lookAhead: lookAhead,
            repathInterval: repathInterval,
            cornerRadius: cornerRadius
        );
    }

    public override void ExecuteJob() => GainXpTick(); // XP tick stays the same

    public override void JobFinished() { }
}
