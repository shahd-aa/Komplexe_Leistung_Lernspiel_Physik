using UnityEngine;
using System.Collections.Generic;

public class BuildingSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject buildingPrefab;
    public Transform startSpawnPoint;
    public float initialBuildings;

    private Transform nextSpawnPoint;
    public List<GameObject> buildings = new List<GameObject>();

    void Start()
    {
        nextSpawnPoint = startSpawnPoint;

        // Spawn initial tiles
        for (int i = 0; i < initialBuildings; i++)
        {
            SpawnBuilding();
        }
    }

    public void SpawnBuilding()
    {
        if (nextSpawnPoint == null)
        {
            Debug.LogError("No spawn point available!");
            return;
        }

        GameObject building = Instantiate(buildingPrefab, nextSpawnPoint.position, Quaternion.identity);
        buildings.Add(building);

        // Find next spawn point in the newly created tile
        Transform exitPoint = building.transform.Find("NextSpawnPoint");

        if (exitPoint != null)
        {
            nextSpawnPoint = exitPoint;
        }
        else
        {
            Debug.LogError($"Tile '{building.name}' doesn't have 'NextSpawnPoint' child!");
        }
    }
}
