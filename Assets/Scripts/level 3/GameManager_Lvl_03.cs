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

public class GameManager_Lvl_03 : BlueprintLevel
{
    [Header("Level Specific")]
    public GameObject ball;
    public GameObject slowMoPanel;
    public float jumpToThrowTime = 6.9833f;

    // Static fields for scene reload
    private static bool pendingSlowMoReplay = false;
    private static double pendingTimelineTime = 0.0;
    private static float pendingTimeScale = 1f;

    protected override void Start()
    {
        if (currentQuestion != null)
        {
            questionProgressList.Add(new QuestionProgress(currentQuestion));
        }

        // handle slowmo replay if coming from reload
        if (pendingSlowMoReplay)
        {
            HandleSlowMoReplay();
            return;
        }

        // hide slowmo panel initially
        if (slowMoPanel != null)
            slowMoPanel.SetActive(false);

        ChangeState(LevelState.Title);
    }

    void HandleSlowMoReplay()
    {
        if (slowMoPanel != null)
            slowMoPanel.SetActive(false);

        pendingSlowMoReplay = false;

        // Manually do ONLY what we need for slowmo
        foreach (CinemachineCamera cam in cinemachineCameras)
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(true);
            }
        }

        // Hide ALL UI panels
        HideAllPanels();

        // Apply timescale BEFORE starting timeline
        Time.timeScale = pendingTimeScale;

        if (timeline != null)
        {
            timeline.time = pendingTimelineTime;
            timeline.Evaluate();

            // Hook up the event to transition to quiz after slowmo ends
            timeline.stopped -= OnSlowMoEnded;
            timeline.stopped += OnSlowMoEnded;

            timeline.Play();
        }

        // Prep quiz components for later 
        PopulateAnswerOptions();
        DisableBackgroundRaycasts();
    }

    void OnDestroy()
    {
        if (timeline != null)
            timeline.stopped -= OnSlowMoEnded;
    }

    void OnSlowMoEnded(PlayableDirector pd)
    {
        Debug.Log("SLOWMO ENDED! Now showing quiz...");

        // Reset timescale
        Time.timeScale = 1f;

        if (pd != null)
        {
            pd.Pause();
        }

        StartCoroutine(OnQuizState());
    }

    protected override void OnCutsceneEnded(PlayableDirector pd)
    {
        Debug.Log("CUTSCENE ENDED! (Level 3 version)");

        if (pd != null)
        {
            pd.Pause();
        }

        foreach (CinemachineCamera cam in cinemachineCameras)
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(false);
            }
        }

        StartCoroutine(SlowMoReplaySequence());
    }

    IEnumerator SlowMoReplaySequence()
    {

        if (slowMoPanel != null)
            slowMoPanel.SetActive(true);
        RectTransform rt = slowMoPanel.GetComponent<RectTransform>();
        PopOut(rt);

        if (subtitlePanel != null)
            subtitlePanel.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (slowMoPanel != null)
            slowMoPanel.SetActive(false);

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);

        // Request reload with slowmo
        RequestReloadAndSlowMo(jumpToThrowTime, 0.7f);
    }

    void RequestReloadAndSlowMo(double timeToJump, float timeScale = 0.7f)
    {
        if (slowMoPanel != null)
            slowMoPanel.SetActive(false);

        // Set flags for next load
        pendingSlowMoReplay = true;
        pendingTimelineTime = timeToJump;
        pendingTimeScale = timeScale;

        // Cleanup
        DOTween.KillAll();
        AudioListener.pause = false;

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
