using GameMath.Demo;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public int maxNPCs = 50;
    public GameObject npcPrefab;
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

            //Instantiate the NPC at the random position within the spawn zone
            GameObject npcInstance = Instantiate(
                npcPrefab,
                currentSpawnZone.transform.position + randomPosition,
                Quaternion.Euler(0, _randomHeading, 0)
            );

            NPC npcComponent = npcInstance.GetComponent<NPC>();
            if (npcComponent != null)
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
