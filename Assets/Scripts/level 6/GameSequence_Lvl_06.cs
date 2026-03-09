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

public class GameSequence_Lvl_06 : BlueprintLevel
{
    [Header("Level Specific UI")]
    public GameObject GameplayUI;
    public Button continueButton;
    public GameObject taskPanel;
    public List<GameObject> arrows = new List<GameObject>();

    [Header("Visual Representation")]
    public GameObject visualVersionPanel;
    public Button visualVersionBtn;
    public Button continueFeedbackButton;

    [Header("References")]
    public Transform treasure;
    public Transform rope;

    [Header("Scripts")]
    public GameManager_Lvl_06 gameManager;

    protected override void Start()
    {
        Debug.Log($"starting level: [{levelNumber}]");
        base.Start();
    }

    protected override void OnCutsceneEnded(PlayableDirector pd)
    {
        Debug.Log("CUTSCENE ENDED!");

        if (pd != null)
        {
            pd.Pause();
            pd.stopped -= OnCutsceneEnded;
        }

        foreach (CinemachineCamera cam in cinemachineCameras)
        {
            cam.gameObject.SetActive(false);
        }

        OnExperimentState();

        // parent treasure to rope
        treasure.SetParent(rope);

        // show gameplay ui 
        GameplayUI.SetActive(true);
    }

    void OnExperimentState()
    {
        RectTransform taskPanelRect = taskPanel.GetComponent<RectTransform>(); ;
        base.animationsUIScript.PopInPopOut(taskPanelRect, 0.2f, 9f);

        // continue button
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() => StartCoroutine(base.OnQuizState()));
        visualVersionBtn.onClick.AddListener(ShowVisualVersion);
    }

    void ShowVisualVersion()
    {
        feedbackPanel.SetActive(false);
        quizPanel.SetActive(true);
        visualVersionPanel.SetActive(true);

        int activeCharacterCount = gameManager.ReturnActiveCharacters();

        for (int i = 0; i < arrows.Count; i++)
        {
            arrows[i].SetActive(i < activeCharacterCount);
        }

        continueFeedbackButton.onClick.AddListener(ReenableFeedbackPanel);
    }

    void ReenableFeedbackPanel()
    {
        HideAllPanels();
        quizPanel.SetActive(true);
        feedbackPanel.SetActive(true);
        characterPanel.SetActive(true);

        if (answerSystem.isCorrect)
        {
            base.SetActiveSafe(characterHappy, true);
            if (nextButton != null) SetActiveSafe(nextButton.gameObject, true);
            nextButton.onClick.AddListener(() => ChangeState(LevelState.Complete));
        }
        else
        {
            base.SetActiveSafe(characterUpset, true);
            if (retryButton != null)
            {
                SetActiveSafe(retryButton.gameObject, true);
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetry);
            }

        }
    }

    protected override void HideAllPanels()
    {
        base.HideAllPanels();
        GameplayUI.SetActive(false);
        taskPanel.SetActive(false);
        visualVersionPanel.SetActive(false);
    }

    protected override void RegisterLevelPoints(int questionIndex)
    {
        base.RegisterLevelPoints(questionIndex);
    }
}