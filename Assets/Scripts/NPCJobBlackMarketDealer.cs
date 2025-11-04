// ...existing code...
using UnityEngine;

public class NPCJobBlackMarketDealer : NPCJob
{
    protected override void Start()
    {
        base.Start();
        // dealer-specific init
    }

    protected override void Update()
    {
        base.Update();
        // dealer-specific per-frame logic, then call job
        ExecuteJob();
    }

    public override void ExecuteJob()
    {
        // actual job implementation
        Debug.Log($"{name} is dealing in the black market.");
    }
}
