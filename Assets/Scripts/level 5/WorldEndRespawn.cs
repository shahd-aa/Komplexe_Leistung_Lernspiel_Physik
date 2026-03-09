using UnityEngine;
using System.Collections.Generic;

public class WorldEndRespawn : MonoBehaviour
{
    public List<Transform> characters = new List<Transform>();
    public Transform RespawnPosition;

    void Start()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        // get position of characters and put them back to the respawn gameobject
        foreach (Transform character in characters)
        {
            character.position = RespawnPosition.position;
        }
    }
}
