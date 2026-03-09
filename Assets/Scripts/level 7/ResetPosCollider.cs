using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ResetPosCollider : MonoBehaviour
{

    GameManager_Lvl_07 gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager_Lvl_07>();
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Trigger"))
        {
            Debug.Log("hit collider");
            gameManager.ResetPosition(gameManager.crate);
        }
    }
}
