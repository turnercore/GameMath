using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshPro))]
public class NPCLevelText : MonoBehaviour
{
    // Text compoinent reference
    private TMPro.TextMeshPro levelText;

    // Parent NPC reference
    private NPCBrain npcBrain;

    void Awake()
    {
        levelText = GetComponent<TMPro.TextMeshPro>();
        npcBrain = GetComponentInParent<NPCBrain>();
        if (npcBrain == null)
        {
            Debug.LogError("NPCLevelText: No NPCBrain component found in parent hierarchy.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // I'm doing this in update but really it should subsribe to a level up event on the npc
        if (npcBrain != null)
        {
            levelText.text = $"Level: {npcBrain.MyLevel}";
        }
    }
}
