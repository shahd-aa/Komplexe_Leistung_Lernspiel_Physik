using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class GameManager_Lvl_04 : BlueprintLevel
{
    [Header("Level Specific")]
    public GameObject character;

    public GameObject spoon1;
    public GameObject spoon2;
    public GameObject spoon3;


    protected override void Start()
    {
        Debug.Log($"starting level: [{levelNumber}]");
        base.Start();
    }

    protected override void OnCutsceneEnded(PlayableDirector pd)
    {
        base.OnCutsceneEnded(pd);
        character.SetActive(false);
    }

    public void DeactivateObject(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(false);
    }

    public void ActivateObject(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(true);
    }
}
