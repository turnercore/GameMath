// ...existing code...
using UnityEngine;

public class NPCJobBlackMarketDealer : NPCJob
{
    protected override void Start()
    {
        base.Start();
        // guard-specific init
    }

    protected override void Update()
    {
        base.Update();
        // guard-specific per-frame logic, then call job
        ExecuteJob();
    }

    public override void ExecuteJob()
    {
        // actual job implementation
        Debug.Log($"{name} is guarding.");
    }
}
