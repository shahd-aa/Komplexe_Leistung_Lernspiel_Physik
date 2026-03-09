using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections;

public class GameManager_Lvl_02 : BlueprintLevel
{
    [Header("Level Specific")]
    public GameObject arrowsPanel;
    public GameObject placeholderCharacter;


    protected override void Start()
    {
        base.Start();
    }

    protected override void OnCutsceneState()
    {
        base.OnCutsceneState();
        SetActiveSafe(arrowsPanel, true);
    }

    protected override void OnCutsceneEnded(PlayableDirector pd)
    {
        base.OnCutsceneEnded(pd);
        SetActiveSafe(placeholderCharacter, true);
    }

    protected override IEnumerator OnQuizState()
    {
        yield return base.OnQuizState();
        SetActiveSafe(placeholderCharacter, true);
    }

    protected override void HideAllPanels()
    {
        base.HideAllPanels();
        SetActiveSafe(arrowsPanel, false);
        SetActiveSafe(placeholderCharacter, false);
    }
}
