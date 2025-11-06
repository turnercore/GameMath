using GameMath.Demo;
using UnityEngine;

[RequireComponent(typeof(NPCBrain))]
public class NPC : MonoBehaviour
{
    private NPCData _npcData; // assign NPCData ScriptableObject in the Inspector or on spawn
    public NPCData npcData
    {
        get { return _npcData; }
        set
        {
            _npcData = value;
            if (_npcData != null)
            {
                // apply NPCData properties, e.g., set body color
                if (bodyRenderer != null)
                    bodyRenderer.material.color = _npcData.bodyColor;
                npcName = _npcData.npcName;
            }
        }
    }

    public Renderer bodyRenderer; // assign the Renderer of the NPC's body in the Inspector

    private string npcName;

    [SerializeField]
    private NPCBrain _npcBrain; // assign NPCBrain in the Inspector
}
