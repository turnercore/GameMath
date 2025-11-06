using TMPro;
using UnityEngine;

public class DebugNPCStateText : MonoBehaviour
{
    private NPCBrain npcBrain;
    private TextMeshPro textComp;

    void Awake()
    {
        npcBrain = GetComponentInParent<NPCBrain>();
        textComp = GetComponent<TextMeshPro>();

        if (npcBrain == null)
            Debug.LogError("DebugNPCStateText: No NPCBrain found in parent hierarchy.");
        if (textComp == null)
            Debug.LogError("DebugNPCStateText: No TextMeshPro component on object.");
    }

    void Update()
    {
        if (npcBrain == null || textComp == null)
            return;

        BrainTask task = npcBrain.CurrentTask;
        if (task == null)
        {
            textComp.text = "Idle";
            return;
        }

        string mainName = StripName(task.GetType().Name);
        string fullText = mainName;

        // If this task has a visible subtask (e.g. BrainTaskWorkOnJob → HerdCow)
        if (task is BrainTaskWorkOnJob workJob)
        {
            var child = workJob.CurrentBehavior; // expose this property in the class
            if (child != null && !child.IsCompleted)
            {
                string childName = StripName(child.GetType().Name);
                fullText = $"{mainName} → {childName}";
            }
        }

        textComp.text = fullText;
    }

    private static string StripName(string name)
    {
        if (name.StartsWith("BrainTask"))
            name = name.Substring("BrainTask".Length);
        return name;
    }
}
