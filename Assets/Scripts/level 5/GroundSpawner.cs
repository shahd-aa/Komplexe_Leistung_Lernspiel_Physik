using UnityEngine;
using System.Collections.Generic;

public class GroundSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject groundTilePrefab;
    public Transform startSpawnPoint;
    public float initialTiles;

    private Transform nextSpawnPoint;
    public List<GameObject> tiles = new List<GameObject>();

    void Start()
    {
        if (startSpawnPoint != null)
        {
            nextSpawnPoint = startSpawnPoint;
        }

        // Spawn initial tiles
        for (int i = 0; i < initialTiles; i++)
        {
            SpawnTile();
        }
    }

    public void SpawnTile()
    {
        if (nextSpawnPoint == null)
        {
            Debug.LogError("No spawn point available!");
            return;
        }

        GameObject tile = Instantiate(groundTilePrefab, nextSpawnPoint.position, Quaternion.identity);
        tiles.Add(tile);

        // Find next spawn point in the newly created tile
        Transform exitPoint = tile.transform.Find("NextSpawnPoint");

        if (exitPoint != null)
        {
            nextSpawnPoint = exitPoint;
        }
        else
        {
            Debug.LogError($"Tile '{tile.name}' doesn't have 'NextSpawnPoint' child!");
        }
    }
}
