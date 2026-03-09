using UnityEngine;

public class GroundTile : MonoBehaviour
{
    [Header("Settings")]
    public int tilesAheadToSpawn = 1;
    public int totalPlayers = 2; // total number of players to track

    private GroundSpawner spawner;
    private int playersEntered = 0;  // how many players have entered at least once
    private int playersExited = 0;   // how many players have exited after entering
    private bool hasSpawnedNext = false;

    void Start()
    {
        spawner = FindAnyObjectByType<GroundSpawner>();
        if (spawner == null)
        {
            Debug.LogError("GroundSpawner not found in scene!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersEntered++;

            // Spawn next tiles only once, when the first player enters
            if (!hasSpawnedNext && spawner != null)
            {
                for (int i = 0; i < tilesAheadToSpawn; i++)
                {
                    spawner.SpawnTile();
                }
                hasSpawnedNext = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersExited++;

            // Only destroy if ALL players have entered AND ALL have exited
            if (playersEntered >= totalPlayers && playersExited >= totalPlayers)
            {
                spawner.tiles.Remove(gameObject);
                Destroy(gameObject, 5);
            }
        }
    }
}
