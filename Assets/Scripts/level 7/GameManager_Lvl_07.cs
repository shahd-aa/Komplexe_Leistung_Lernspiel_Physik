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

public class GameManager_Lvl_07 : BlueprintLevel
{
    // ============================================================
    // INSPECTOR FIELDS
    // ============================================================

    [Header("=== QUIZ QUESTIONS ===")]
    [Space(5)]
    public QuestionData question1;
    public QuestionData question2;
    public List<string> taskTexts = new List<string>();

    [Header("=== ARROWS ===")]
    [Space(5)]
    public List<GameObject> arrows = new List<GameObject>();
    public List<Sprite> arrows2Images = new List<Sprite>();

    [Header("=== CHARACTERS & OBJECTS ===")]
    [Space(5)]
    public List<GameObject> bgCharacters = new List<GameObject>();
    public List<GameObject> characters = new List<GameObject>();
    public GameObject crate;

    [Header("=== UI PANELS ===")]
    [Space(5)]
    public GameObject GameplayUI;
    public GameObject taskPanel;
    public GameObject correctPanel;
    public GameObject wrongNotePanel;

    [Header("=== DROP ZONES ===")]
    [Space(5)]
    public List<DropZone> dropZones = new List<DropZone>();

    // ============================================================
    // PRIVATE FIELDS
    // ============================================================

    private AnimationsUI animationUIScript;
    private Vector3 initialPos;
    private List<string> arrowNames = new List<string>();

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    protected override void Start()
    {
        base.questionProgressList.Add(new QuestionProgress(question1));
        base.questionProgressList.Add(new QuestionProgress(question2));

        // Set first question of quiz
        currentQuestion = question1;
        currentQuestionIndex = 0;

        Debug.Log($"starting level: [{levelNumber}]");

        animationUIScript = FindAnyObjectByType<AnimationsUI>();

        initialPos = crate.transform.position;

        Debug.Log($"initial position of crate is {initialPos}");

        base.Start();
        PauseAnimations(bgCharacters);
    }

    // ============================================================
    // STATE MANAGEMENT - Cutscene & Experiment
    // ============================================================

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
    }

    void OnExperimentState()
    {
        RectTransform taskPanelRect = taskPanel.GetComponent<RectTransform>();
        base.animationsUIScript.PopInPopOut(taskPanelRect, 0.2f, 7f);

        ChangeTask();

        GameplayUI.SetActive(true);
    }

    void ChangeTask()
    {
        // change task text
        TextMeshProUGUI taskPanelTMP = taskPanel.GetComponentInChildren<TextMeshProUGUI>();

        if (currentQuestionIndex == 0)
        {
            taskPanelTMP.text = taskTexts[0];
        }
        else
        {
            taskPanelTMP.text = taskTexts[1];
        }
    }

    protected override void HideAllPanels()
    {
        base.HideAllPanels();
        taskPanel.SetActive(false);
        GameplayUI.SetActive(false);
        wrongNotePanel.SetActive(false);
        correctPanel.SetActive(false);
    }

    // ============================================================
    // FEEDBACK STATE - Handle quiz answers
    // ============================================================

    protected override void OnFeedbackState()
    {
        // get question progress for index of question
        QuestionProgress currentProgress = GetCurrentProgress();

        SetActiveSafe(submitButton?.gameObject, false);
        SetActiveSafe(answerOptionsPanel, false);
        SetActiveSafe(questionPanel, false);
        SetActiveSafe(quizPanel, true);
        SetActiveSafe(feedbackPanel, true);

        if (answerSystem.isCorrect)
        {
            currentProgress.MarkCompleted();
            HandleCorrectAnswer();
        }
        else
        {
            currentProgress.RegisterFailedAttempt();
            HandleWrongAnswer();
        }
    }

    void HandleCorrectAnswer()
    {
        int correctIndex = currentQuestion.correctAnswerIndex;
        string explanation = currentQuestion.GetExplanation(correctIndex);

        if (feedbackText != null)
            feedbackText.text = string.IsNullOrEmpty(explanation)
                ? currentQuestion.correctFeedback
                : currentQuestion.correctFeedback;

        if (explanationText != null)
            explanationText.text = explanation;

        SetActiveSafe(characterPanel, true);
        SetActiveSafe(characterHappy, true);
        SetActiveSafe(confettiRight, true);
        SetActiveSafe(confettiLeft, true);

        // First question correct -> go to second gameplay
        if (currentQuestionIndex == 0)
        {
            SetupNextButton("Weiter", GoToSecondGameplay);
        }
        // Second question correct -> level complete
        else
        {
            SetupNextButton("Nächste", () => ChangeState(LevelState.Complete));
        }
        
        RegisterLevelPoints(currentQuestionIndex);
    }

    void HandleWrongAnswer()
    {
        int selectedIndex = answerSystem.selectedIndex;
        string explanation = currentQuestion.GetExplanation(selectedIndex);

        if (nextButton != null) SetActiveSafe(nextButton.gameObject, false);

        if (feedbackText != null)
            feedbackText.text = string.IsNullOrEmpty(explanation)
                ? currentQuestion.wrongFeedback
                : currentQuestion.wrongFeedback;

        if (explanationText != null)
            explanationText.text = explanation;

        SetActiveSafe(characterPanel, true);
        SetActiveSafe(characterUpset, true);

        if (retryButton != null)
        {
            SetActiveSafe(retryButton.gameObject, true);
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryMultiQuestion);
        }
    }

    protected override void RegisterLevelPoints(int questionIndex)
    {
        int levelTotal = 0;
        TextMeshProUGUI rankPointsText = rankPointsPanel.GetComponentInChildren<TextMeshProUGUI>();

        // ALWAYS calculate points
        foreach (QuestionProgress qp in questionProgressList)
        {
            levelTotal += qp.CalculateEarnedPoints();
        }

        // ALWAYS save points
        GameProgress.SaveLevelPoints(levelNumber, levelTotal);

        QuestionProgress justAnswered = questionProgressList[questionIndex];
        Debug.Log($"level number: {levelNumber}, total points: {levelTotal}, question {questionIndex} failed attempts: {justAnswered.failedAttempts}");

        // Show animation if the question is NOW completed (regardless of past failures)
        if (justAnswered.isCompleted)
        {
            rankPointsText.text = levelTotal.ToString() + " Punkte";
            animationsUIScript.PopInFadeOut(rankPointsPanel);
        }
    }

    void SetupNextButton(string buttonText, System.Action onClick)
    {
        if (nextButton != null)
        {
            SetActiveSafe(nextButton.gameObject, true);

            TMP_Text text = nextButton.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = buttonText;

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => onClick());
        }
    }

    // ============================================================
    // QUESTION FLOW - Navigate between questions/gameplay
    // ============================================================

    // Transition to second quiz question (currently unused)
    void GoToSecondQuestion()
    {
        Debug.Log("Going to second question!");

        currentQuestionIndex = 1;
        currentQuestion = question2;

        if (answerSystem != null)
        {
            answerSystem.ResetButtons();
        }

        HideAllPanels();
        StartCoroutine(OnQuizState());
    }

    protected override IEnumerator OnQuizState()
    {
        AnimationHandler(characters, false);
        return base.OnQuizState();
    }

    // Transition back to gameplay for second arrow puzzle
    void GoToSecondGameplay()
    {
        Debug.Log("Going to second gameplay phase!");

        currentQuestionIndex = 1;
        currentQuestion = question2; // Prepare second question for after arrows

        HideAllPanels();
        ResetGameplayForSecondRound();

        AnimationHandler(characters, true);

        RectTransform taskPanelRect = taskPanel.GetComponent<RectTransform>();
        base.animationsUIScript.PopInPopOut(taskPanelRect, 0.2f, 7f);

        ChangeTask();

        GameplayUI.SetActive(true);

        Debug.Log("Waiting for player to place arrows for round 2...");
    }

    // Retry current quiz question after wrong answer
    void OnRetryMultiQuestion()
    {
        Debug.Log("OnRetry called for multi-question level!");

        SetActiveSafe(feedbackPanel, false);
        SetActiveSafe(characterPanel, false);
        SetActiveSafe(characterHappy, false);
        SetActiveSafe(characterUpset, false);

        if (retryButton != null)
        {
            SetActiveSafe(retryButton.gameObject, false);
            retryButton.onClick.RemoveAllListeners();
        }

        SetActiveSafe(questionPanel, true);
        SetActiveSafe(answerOptionsPanel, true);

        Shuffle();

        if (answerSystem != null) answerSystem.ResetButtons();

        LoadQuestionData();

        if (submitButton != null && answerSystem != null)
        {
            SetActiveSafe(submitButton.gameObject, true);
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(() => answerSystem.CheckAnswer());
        }
    }

    // ============================================================
    // GAMEPLAY LOGIC - Arrow checking & validation
    // ============================================================

    // Reset everything for second gameplay round
    void ResetGameplayForSecondRound()
    {
        arrowNames.Clear();

        // Stop any running coroutines and clear arrow references
        foreach (DropZone zone in dropZones)
        {
            zone.StopCheckCoroutine();
            zone.ClearArrow(zone.currentArrowObject);
        }

        foreach (GameObject arrow in arrows)
        {
            if (arrow != null)
            {
                DraggableItem item = arrow.GetComponent<DraggableItem>();
                item.ReturnToOriginal();
            }
        }

        // change sprites for the last two arrows
        ChangeSprite(arrows2Images[0], 2); // third arrow
        ChangeMagnitude(arrows[2], 2, 25);
        ChangeSprite(arrows2Images[1], 3); // fourth arrow
        ChangeMagnitude(arrows[3], 3, 25);

        ResetPosition(crate);

        Debug.Log("Gameplay reset for second round - drop zones cleared");
    }

    // Called by DropZone when arrow is placed - validates both arrows
    public IEnumerator CheckBothArrows()
    {
        arrowNames.Clear();
        string arrowName;

        // Collect arrows from both drop zones
        foreach (DropZone zone in dropZones)
        {
            arrowName = zone.CheckArrow();
            if (arrowName != null)
            {
                arrowNames.Add(arrowName);
            }
        }

        yield return new WaitForSeconds(2f);

        // Check if both zones are filled
        if (arrowNames.Count < 2)
        {
            Debug.Log("Not all drop zones are filled");
            yield break;
        }

        bool isCorrect = false;

        // Validate arrows based on current question index
        if (currentQuestionIndex == 0)
        {
            isCorrect = (arrowNames[0] == "Blue_Arrow_Right" && arrowNames[1] == "Red_Arrow_Right");
        }
        else if (currentQuestionIndex == 1)
        {
            isCorrect = (arrowNames[0] == "Blue_Arrow_Right" && arrowNames[1] == "Red_Arrow_Left");
        }

        if (isCorrect)
        {
            Debug.Log($"✓ CORRECT! Both arrows fit correctly for question {currentQuestionIndex + 1}");
            yield return new WaitForSeconds(2f);
            ShowConfetti();
            animationUIScript.PopInFadeOut(correctPanel);

            // Wait, then transition to quiz
            yield return new WaitForSeconds(3f);
            HideAllPanels();
            ChangeState(LevelState.Quiz);
        }
        else
        {
            Debug.Log("Both arrows are wrong!");
            animationUIScript.PopInFadeOut(wrongNotePanel);
        }
    }

    // ============================================================
    // UTILITY METHODS - Helpers & animations
    // ============================================================

    void ChangeSprite(Sprite newSprite, int index)
    {
        Image image = arrows[index].GetComponent<Image>();
        image.sprite = newSprite;
    }

    void ChangeMagnitude(GameObject arrowObj, int index, int magnitude)
    {
        ForceArrow arrow = arrowObj.GetComponent<ForceArrow>();
        arrow.magnitude = magnitude;
    }

    public void ResetPosition(GameObject obj)
    {
        obj.transform.position = initialPos;
    }

    void ShowConfetti()
    {
        base.confettiRight.SetActive(true);
        base.confettiLeft.SetActive(true);
    }

    // Signal receiver method for timeline
    public void StartAnimation(Animator anim)
    {
        anim.speed = 1f;
    }


    public void AnimationHandler(List<GameObject> list, bool IsAnimating)
    {
        Debug.Log("checking animation of characters ...");
        foreach (GameObject item in list)
        {
            Animator anim = item.GetComponent<Animator>();
            if (IsAnimating == true)
            {
                anim.speed = 1f;
                Debug.Log("animation speed of characters = 1");
            }
            else
            {
                anim.speed = 0f;
                anim.Play("idle anim", 0, 0f);
                anim.Update(0f);
                Debug.Log("animation speed of characters = 0");
            }
        }
    }

    void PauseAnimations(List<GameObject> list)
    {
        foreach (GameObject item in list)
        {
            Animator anim = item.GetComponent<Animator>();
            anim.speed = 0f;
        }
    }
}