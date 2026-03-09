using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    [Header("Settings")]
    public int buildingsAheadToSpawn = 1;
    public int totalPlayers = 2; // total number of players to track

    private BuildingSpawner spawner;
    private int playersEntered = 0;  // how many players have entered at least once
    private int playersExited = 0;   // how many players have exited after entering
    private bool hasSpawnedNext = false;

    void Start()
    {
        spawner = FindAnyObjectByType<BuildingSpawner>();
        if (spawner == null)
        {
            Debug.LogError("BuildingSpawner not found in the scene!");
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
                for (int i = 0; i < buildingsAheadToSpawn; i++)
                {
                    spawner.SpawnBuilding();
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
                spawner.buildings.Remove(gameObject);
                Destroy(gameObject, 5);
            }
        }
    }

}
