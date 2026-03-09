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

public class BlueprintLevel : MonoBehaviour
{
    // ===== STATE =====
    protected LevelState currentState; // children of class can see this (protected)

    // ===== REFERENCES =====
    // Main panels
    [Header("Main Panels")]
    public GameObject titlePanel;
    public GameObject subtitlePanel;
    public GameObject quizPanel;
    public GameObject feedbackPanel;

    [Header("Rank")]
    public GameObject rankPointsPanel;

    // Quiz elements
    [Header("Quiz Elements")]
    public GameObject questionPanel;
    public GameObject answerOptionsPanel;
    public GameObject characterPanel;
    public TMP_Text feedbackText;
    public TMP_Text explanationText;

    public Button nextButton;
    public Button submitButton;
    public Button retryButton;

    // Question data
    [Header("Question Data")]
    [SerializeField] protected QuestionData currentQuestion;
    public int levelNumber;
    [HideInInspector] public List<QuestionProgress> questionProgressList;
    [HideInInspector] public int currentQuestionIndex = 0;
    public TMP_Text questionText;
    public AnswerSystem answerSystem;

    // References
    [Header("Scripts")]
    public SubtitleScript subtitleScript;
    public QuizIntroAnimation introAnim;
    public AnimationsUI animationsUIScript;

    [Header("References")]
    public GameObject lettersPanel;
    public PlayableDirector timeline;
    public GameObject blurImage;
    public GameObject confettiRight;
    public GameObject confettiLeft;

    // Cinemachine
    [Header("Cinemachine")]
    public List<CinemachineCamera> cinemachineCameras = new List<CinemachineCamera>();
    public CinemachineBrain mainCameraBrain;

    // Character
    [Header("Character")]
    public GameObject characterUpset;
    public GameObject characterHappy;

    // Settings
    [Header("Settings")]
    public float titleDisplayTime = 2f;
    public float delayBetweenAnswerOptions = 0.1f;
    public float scaleX, scaleY;
    public float tweenDuration = 0.35f;
    public float scaleUpFactor = 1.2f;
    private Vector3 originalSize;

    // Private
    public readonly List<GameObject> answerOptions = new List<GameObject>();
    private readonly List<RectTransform> answerOptionsRects = new List<RectTransform>();

    private Coroutine currentStateCoroutine;

    // ===== INITIALIZATION ====
    protected virtual void Awake()
    {
        if (answerSystem != null)
        {
            answerSystem.SetBlueprintLevel(this);
        }

        // Validate critical references that MUST exist
        if (answerSystem == null && submitButton == null)
        {
            Debug.LogError("AnswerSystem and Submit Button are required!", this);
            enabled = false;
            return;
        }

        // create progress trackers for each question
        questionProgressList = new List<QuestionProgress>();

        PopulateAnswerOptions();
    }

    protected virtual void Start() // children can override this (virtual)
    {
        ChangeState(LevelState.Title);

        if (currentQuestion != null)
        {
            questionProgressList.Add(new QuestionProgress(currentQuestion));
        }
    }

    // ===== STATE MANAGEMENT =====
    public void ChangeState(LevelState newState)
    {
        // stop any running state coroutine first
        if (currentStateCoroutine != null)
        {
            Debug.Log("coroutine stopped");
            StopCoroutine(currentStateCoroutine);
        }

        currentState = newState;

        // hide panels + disable raycasts before any state
        HideAllPanels();
        DisableBackgroundRaycasts();

        switch (newState)
        {
            case LevelState.Title:
                currentStateCoroutine = StartCoroutine(OnTitleState());
                break;

            case LevelState.Cutscene:
                OnCutsceneState();
                break;

            case LevelState.Quiz:
                currentStateCoroutine = StartCoroutine(OnQuizState());
                break;

            case LevelState.Feedback:
                OnFeedbackState();
                break;

            case LevelState.Complete:
                OnCompleteState();
                break;
        }
    }

    public QuestionProgress GetCurrentProgress()
    {
        return questionProgressList[currentQuestionIndex];
    }

    // ===== STATE HANDLERS (virtual = customizable for children) =====

    protected virtual IEnumerator OnTitleState()
    {
        Debug.Log("LEVEL STATE: TITLE STATE");

        if (timeline != null)
        {
            timeline.Pause();
        }

        SetActiveSafe(titlePanel, true);
        yield return new WaitForSeconds(Mathf.Max(0f, titleDisplayTime));
        SetActiveSafe(titlePanel, false);

        ChangeState(LevelState.Cutscene);
    }

    protected virtual void OnCutsceneState()
    {
        Debug.Log("LEVEL STATE: CUTSCENE STATE");

        SetActiveSafe(subtitlePanel, true);

        foreach (CinemachineCamera cam in cinemachineCameras)
        {
            cam.gameObject.SetActive(true);
        }

        if (timeline != null)
        {
            timeline.stopped -= OnCutsceneEnded; // unsubscribe first
            timeline.stopped += OnCutsceneEnded;
            timeline.Play();
        }
        else
        {
            ChangeState(LevelState.Quiz);
        }
    }

    protected virtual IEnumerator OnQuizState()
    {
        Debug.Log("LEVEL STATE: QUIZ STATE");

        QuestionProgress current = GetCurrentProgress();

        yield return new WaitForSeconds(1f);

        // hide everything initially
        HideAllPanels();

        // letters animation
        if (introAnim != null)
            yield return StartCoroutine(introAnim.PlayIntroAnimationCoroutine());

        LoadQuestionData();

        // quiz ui
        Debug.Log("showing quiz panels");
        SetActiveSafe(quizPanel, true);
        SetActiveSafe(questionPanel, true);
        SetActiveSafe(answerOptionsPanel, true);
        SetActiveSafe(blurImage, true);

        // ensure blur doesn't block buttons
        if (blurImage != null)
        {
            Image blurImg = blurImage.GetComponent<Image>();
            if (blurImg != null) blurImg.raycastTarget = false;
            blurImage.transform.SetAsFirstSibling();
        }

        // hide all answer choice options first
        foreach (GameObject option in answerOptions)
            SetActiveSafe(option, false);

        // show options one by one from panel children to match hierarchy
        if (answerOptionsPanel != null)
        {
            Transform panelTransform = answerOptionsPanel.transform;
            for (int i = 0; i < panelTransform.childCount; i++)
            {
                Transform answerTransform = panelTransform.GetChild(i);
                if (answerTransform == null) continue;

                GameObject answerOption = answerTransform.gameObject;
                SetActiveSafe(answerOption, true);

                Button btn = answerOption.GetComponent<Button>();
                if (btn != null) btn.interactable = true;

                RectTransform rect = answerOption.GetComponent<RectTransform>();
                if (rect != null) PopOut(rect);

                yield return new WaitForSeconds(Mathf.Max(0f, delayBetweenAnswerOptions));
            }

            answerOptionsPanel.transform.SetAsLastSibling();
        }

        // show submit button and wire safely
        if (submitButton != null && answerSystem != null)
        {
            SetActiveSafe(submitButton.gameObject, true);
            submitButton.onClick.RemoveAllListeners(); // for safety 
            submitButton.onClick.AddListener(() => answerSystem.CheckAnswer());
        }
        else if (submitButton != null)
        {
            Debug.LogWarning("submitButton present but answerSystem is missing - no checks will run", this);
        }
    }

    protected virtual void OnFeedbackState()
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
            // register as completed for the game progress
            currentProgress.MarkCompleted();

            // Get explanation for the correct answer
            int correctIndex = currentQuestion.correctAnswerIndex;
            string explanation = currentQuestion.GetExplanation(correctIndex);

            // Use specific explanation if available, otherwise use general feedback
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

            if (nextButton != null) SetActiveSafe(nextButton.gameObject, true);
            nextButton.onClick.AddListener(() => ChangeState(LevelState.Complete));
        }
        else
        {
            // register as failed attempt 
            currentProgress.RegisterFailedAttempt();

            // get explanation for the wrong answer they selected
            int selectedIndex = answerSystem.selectedIndex;
            string explanation = currentQuestion.GetExplanation(selectedIndex);

            if (nextButton != null) SetActiveSafe(nextButton.gameObject, false);

            // Use specific explanation if available, otherwise use general feedback
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
                retryButton.onClick.AddListener(OnRetry);
            }
        }

        RegisterLevelPoints(currentQuestionIndex);
    }

    protected virtual void OnCompleteState()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    // ===== HELPERS =====

    protected virtual void RegisterLevelPoints(int questionIndex)
    {
        int levelTotal = 0;

        // ALWAYS calculate points
        foreach (QuestionProgress qp in questionProgressList)
        {
            levelTotal += qp.CalculateEarnedPoints();
        }

        // ALWAYS save points
        GameProgress.SaveLevelPoints(levelNumber, levelTotal);

        // Get the first (and only) question - ignore the parameter
        QuestionProgress theQuestion = questionProgressList[0];
        Debug.Log($"level number: {levelNumber}, total points: {levelTotal}, failed attempts: {theQuestion.failedAttempts}");

        // Show animation if the question is NOW completed (regardless of past failures)
        if (theQuestion.isCompleted)
        {
            // Add null check!
            if (rankPointsPanel != null)
            {
                TextMeshProUGUI rankPointsText = rankPointsPanel.GetComponentInChildren<TextMeshProUGUI>();
                if (rankPointsText != null)
                {
                    rankPointsText.text = levelTotal.ToString() + " Punkte";
                    animationsUIScript.PopInFadeOut(rankPointsPanel);
                }
                else
                {
                    Debug.LogWarning("rankPointsPanel has no TextMeshProUGUI child!");
                }
            }
            else
            {
                Debug.LogWarning("rankPointsPanel is not assigned in inspector!");
            }
        }
    }

    protected virtual void HideAllPanels()
    {
        // MAIN PANELS

        // start
        SetActiveSafe(titlePanel, false);
        SetActiveSafe(subtitlePanel, false);

        // rank
        SetActiveSafe(rankPointsPanel, false);

        // quiz
        SetActiveSafe(questionPanel, false);
        SetActiveSafe(answerOptionsPanel, false);
        SetActiveSafe(quizPanel, false);
        SetActiveSafe(blurImage, false);

        // feedback
        SetActiveSafe(feedbackPanel, false);
        SetActiveSafe(characterPanel, false);
        SetActiveSafe(confettiRight, false);
        SetActiveSafe(confettiLeft, false);
        SetActiveSafe(characterHappy, false);
        SetActiveSafe(characterUpset, false);

        // buttons
        SetActiveSafe(submitButton.gameObject, false);
        SetActiveSafe(retryButton.gameObject, false);
        SetActiveSafe(nextButton.gameObject, false);
    }

    public void DisableBackgroundRaycasts()
    {
        if (quizPanel != null)
        {
            Image panelImg = quizPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.raycastTarget = false;
        }

        if (feedbackPanel != null)
        {
            Image panelImg = feedbackPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.raycastTarget = false;
        }

        if (questionPanel != null)
        {
            Image panelImg = questionPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.raycastTarget = false;
        }
    }

    public void SetActiveSafe(GameObject obj, bool value)
    {
        if (obj == null) return;
        if (obj.activeSelf == value) return;
        obj.SetActive(value);
    }

    protected virtual void OnCutsceneEnded(PlayableDirector pd)
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

        ChangeState(LevelState.Quiz);
    }

    // animation letters
    public void PopOut(RectTransform rect)
    {
        if (rect == null) return;
        Vector3 original = rect.localScale;
        Vector3 target = original * Mathf.Max(0.01f, scaleUpFactor);
        rect.localScale = original;
        rect.DOScale(target, Mathf.Max(0.01f, tweenDuration))
            .OnComplete(() => rect.DOScale(original, 0.5f));
    }

    // populate answerOptions from panel children
    public void PopulateAnswerOptions()
    {
        answerOptions.Clear();
        answerOptionsRects.Clear();

        if (answerOptionsPanel == null)
        {
            Debug.LogWarning("answerOptionsPanel is null - cannot populate options", this);
            return;
        }

        foreach (Transform child in answerOptionsPanel.transform)
        {
            if (child == null || child.gameObject == null) continue;
            answerOptions.Add(child.gameObject);
            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect != null)
            {
                answerOptionsRects.Add(rect);
                if (originalSize == Vector3.zero)
                {
                    originalSize = rect.localScale;
                    scaleX = originalSize.x;
                    scaleY = originalSize.y;
                }
            }
        }

        if (answerOptions.Count == 0)
            Debug.LogWarning("no answer option children found under answerOptionsPanel", this);
    }

    // load level questions, answer options, explanation, etc.
    protected virtual void LoadQuestionData()
    {
        if (currentQuestion == null)
        {
            Debug.LogError("currentQuestion is null", this);
            return;
        }

        if (currentQuestion.answers == null || currentQuestion.answers.Count == 0)
        {
            Debug.LogError("currentQuestion has no answers", this);
            return;
        }

        if (questionText != null)
            questionText.text = currentQuestion.questionText ?? string.Empty;

        int itemsToUse = Mathf.Min(currentQuestion.answers.Count, answerOptions.Count);
        if (itemsToUse == 0)
        {
            Debug.LogError("no UI answer options available to populate", this);
            return;
        }

        for (int i = 0; i < itemsToUse; i++)
        {
            GameObject buttonObj = answerOptions[i];
            if (buttonObj == null) continue;
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>(true);
            if (buttonText != null)
                buttonText.text = currentQuestion.GetAnswerText(i); // ← CHANGED!
            else
                Debug.LogWarning($"no TMP_Text on answer option {i}", this);
        }

        if (answerSystem != null)
        {
            answerSystem.correctIndex = Mathf.Clamp(currentQuestion.correctAnswerIndex, 0, Mathf.Max(0, currentQuestion.answers.Count - 1));
        }
        else
        {
            Debug.LogWarning("answerSystem missing - answer checks won't work", this);
        }
    }

    protected virtual void OnRetry()
    {
        Debug.Log("OnRetry called!");

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

    public void Shuffle()
    {
        if (currentQuestion == null || currentQuestion.answers == null || currentQuestion.answers.Count <= 1) return;

        // ← CHANGED: Store the entire correct AnswerOption, not just the text
        AnswerOption correctAnswer = currentQuestion.answers[Mathf.Clamp(currentQuestion.correctAnswerIndex, 0, currentQuestion.answers.Count - 1)];

        int n = currentQuestion.answers.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (currentQuestion.answers[i], currentQuestion.answers[j]) = (currentQuestion.answers[j], currentQuestion.answers[i]);
        }

        // ← CHANGED: Find the index of the correct AnswerOption
        currentQuestion.correctAnswerIndex = Mathf.Clamp(currentQuestion.answers.IndexOf(correctAnswer), 0, currentQuestion.answers.Count - 1);
    }

}
