using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameMath.Demo
{
    [CreateAssetMenu(fileName = "NPCWorkerData", menuName = "TableForge/GameMath/NPC Worker Data")]
    public class NPCWorkerData : ScriptableObject
    {
        public string npcName;
        public Color bodyColor;
        public NPCJob job;
        public BrainTask beforeWorkTask;
        public BrainTask afterWorkTask;
        public Vector2 workHours = new Vector2(8f, 17f);
        public float workSpeed = 1f;

        // Legacy marker you added; duplication copies this, so it's not reliable for detecting duplicates.
        public string uniqueId;

        // Use the asset's GUID to detect duplication (duplicates get a NEW GUID).
        public string assetGuid;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (uniqueId != assetGuid)
            {
                uniqueId = assetGuid;
                npcName = GenerateRandomName();
                EditorUtility.SetDirty(this);
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            // Get current GUID from asset database
            string path = AssetDatabase.GetAssetPath(this);
            string currentGuid = string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.AssetPathToGUID(path);

            // If the asset has no GUID stored yet (first ever creation)
            if (string.IsNullOrEmpty(assetGuid))
            {
                assetGuid = currentGuid;
                uniqueId = assetGuid; // sync IDs on first use

                if (string.IsNullOrEmpty(npcName))
                {
                    npcName = GenerateRandomName();
                }

                EditorUtility.SetDirty(this);
                return;
            }

            // If GUID changed → this asset is a duplicate
            if (assetGuid != currentGuid)
            {
                // Sync assetGuid → current GUID
                assetGuid = currentGuid;

                // Assign new uniqueId to mark new instance
                uniqueId = currentGuid;

                // Regenerate name
                npcName = GenerateRandomName();

                EditorUtility.SetDirty(this);
                return;
            }

            // If user manually clears name
            if (string.IsNullOrEmpty(npcName))
            {
                npcName = GenerateRandomName();
                EditorUtility.SetDirty(this);
            }
        }
#endif

        private static readonly List<string> firstNames = new List<string>
        {
            "Wobble",
            "Pickle",
            "Snorf",
            "Bumble",
            "Tater",
            "Muffin",
            "Boingo",
            "Spronk",
            "Beebo",
            "Florp",
            "Zibby",
            "Gunk",
            "Fizz",
            "Noodle",
            "Pumpkin",
            "Zonko",
            "Womble",
            "Binky",
            "Sploof",
            "Chumble",
            "Doodle",
            "Giblet",
            "Plinko",
            "Skipper",
            "Blink",
            "Goober",
            "Wiggles",
            "Spud",
            "YoYo",
            "Scrungle",
            "Momo",
            "Pip",
            "Boppo",
            "Froodle",
            "Flib",
            "Snickers",
            "Wumbo",
            "Mango",
            "Blarb",
        };

        private static readonly List<string> lastNames = new List<string>
        {
            "McWobble",
            "Pickleton",
            "Bagelstein",
            "Snickerdoodle",
            "Flapjackson",
            "Sprinklebottom",
            "Wiggleworth",
            "Bumblesnort",
            "Gobblesworth",
            "Scooterton",
            "Chumblebee",
            "Splonkers",
            "Fizzlebottom",
            "Gloober",
            "Dinglesniff",
            "Froggleburn",
            "Wafflestein",
            "Sparkletush",
            "Chonko",
            "Flufferpuff",
            "Mugsywomp",
            "Biscuitford",
            "Snufflecrumb",
            "Barnabywax",
            "Bunkleton",
            "Snootlewhip",
            "Gibblechunk",
            "Toasterson",
            "Bubblebelly",
            "Crumbsniffer",
            "Wobblehaus",
            "Drizzlepatch",
            "Tinklenut",
            "Thunderbuns",
            "Snorkleton",
            "Gumphammer",
            "Waddlington",
            "Mooington",
        };

        public static string GenerateRandomName()
        {
            if (
                firstNames == null
                || firstNames.Count == 0
                || lastNames == null
                || lastNames.Count == 0
            )
                return "TEMP_NAME";

            string first = firstNames[Random.Range(0, firstNames.Count)];
            string last = lastNames[Random.Range(0, lastNames.Count)];
            return $"{first} {last}";
        }
    }
}
