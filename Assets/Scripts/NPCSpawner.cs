using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public int maxNPCs = 50;
    public GameObject npcPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < maxNPCs; i++)
        {
            Vector3 _spawnPosition = new Vector3(
                Random.Range(-10f, 10f),
                0f,
                Random.Range(-10f, 10f)
            );
            Instantiate(npcPrefab, _spawnPosition, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update() { }
}
