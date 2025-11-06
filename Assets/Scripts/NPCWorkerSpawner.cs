using GameMath.Demo;
using UnityEngine;

public class NPCWorkerSpawner : MonoBehaviour
{
    public int maxNPCs = 50;
    public GameObject[] npcPrefabs;
    public BoxCollider[] spawnZones;
    public NPCData[] npcDataOptions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < maxNPCs; i++)
        {
            BoxCollider currentSpawnZone = spawnZones[i % spawnZones.Length];
            //Get a random position within the bounds of the spawn zone cube
            Vector3 randomPosition = GetRandomPositionInSpawnZone(currentSpawnZone);
            float _randomHeading = Random.Range(0f, 360f);
            NPCData selectedNPCData = npcDataOptions[Random.Range(0, npcDataOptions.Length)];

            // Select a random NPC prefab
            if (npcPrefabs.Length == 0)
            {
                Debug.LogError("No NPC prefabs assigned to NPCWorkerSpawner.");
                return;
            }

            GameObject selectedPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];

            //Instantiate the NPC at the random position within the spawn zone
            GameObject npcInstance = Instantiate(
                selectedPrefab,
                currentSpawnZone.transform.position + randomPosition,
                Quaternion.Euler(0, _randomHeading, 0)
            );

            if (npcInstance.TryGetComponent<NPC>(out var npcComponent))
            {
                npcComponent.npcData = selectedNPCData;
            }
        }

        MakeSpawnZonesInvisible();
    }

    private void MakeSpawnZonesInvisible()
    {
        foreach (BoxCollider zone in spawnZones)
        {
            Renderer _renderer = zone.GetComponent<Renderer>();
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }
    }

    private Vector3 GetRandomPositionInSpawnZone(BoxCollider spawnZone)
    {
        float _randomX = Random.Range(-spawnZone.bounds.extents.x, spawnZone.bounds.extents.x);
        float _randomZ = Random.Range(-spawnZone.bounds.extents.z, spawnZone.bounds.extents.z);
        return new Vector3(_randomX, 0, _randomZ);
    }
}
