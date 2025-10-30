using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public int maxNPCs = 50;
    public GameObject npcPrefab;
    public BoxCollider spawnZone;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < maxNPCs; i++)
        {
            //Get a random position within the bounds of the spawn zone cube
            Vector3 randomPosition = GetRandomPositionInSpawnZone();

            //Instantiate the NPC at the random position within the spawn zone
            Instantiate(
                npcPrefab,
                spawnZone.transform.position + randomPosition,
                Quaternion.identity
            );
        }
    }

    private Vector3 GetRandomPositionInSpawnZone()
    {
        float _randomX = Random.Range(-spawnZone.bounds.extents.x, spawnZone.bounds.extents.x);
        float _randomZ = Random.Range(-spawnZone.bounds.extents.z, spawnZone.bounds.extents.z);
        return new Vector3(_randomX, 0, _randomZ);
    }
}
