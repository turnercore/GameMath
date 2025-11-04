using System.Collections.Generic;
using UnityEngine;

namespace GameMath.Demo
{
    [CreateAssetMenu(fileName = "NPCData", menuName = "TableForge/GameMath/NPC Data")]
    public class NPCData : ScriptableObject
    {
        public string npcName;
        public Color bodyColor;
    }
}
