using UnityEngine;

namespace GameMath.Demo
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
    public class LevelData : ScriptableObject
    {
        public int level;
        public int xpNeededForNextLevel;
        public int totalXPNeededForThisLevel;
        public int xpPerTick;
    }
}
