using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class GameManager_Lvl_01 : BlueprintLevel
{
    public List<Rigidbody> boxes;

    [Header("Multiple Questions")]

    public QuestionData question1;
    public QuestionData question2;

    // Override Start to use question1 as the current question initially
    protected override void Start()
    {
        base.questionProgressList.Add(new QuestionProgress(question1));
        base.questionProgressList.Add(new QuestionProgress(question2));

        currentQuestion = question1; // ← Set first question
        currentQuestionIndex = 0;
        base.Start(); // ← Call parent's Start
    }

    public QuestionProgress GetCurrentProgress()
    {
        return questionProgressList[currentQuestionIndex];
    }

    protected override IEnumerator OnQuizState()
    {
        return base.OnQuizState();
    }

    // Override feedback to handle multiple questions
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
            // register as completed for the game progress
            currentProgress.MarkCompleted();

            // get explanation for the correct answer
            int correctIndex = currentQuestion.correctAnswerIndex;
            string explanation = currentQuestion.GetExplanation(correctIndex);

            // Use specific explanation if available, otherwise use general feedback
            if (feedbackText != null)
                feedbackText.color = correctColor;
                feedbackText.text = string.IsNullOrEmpty(explanation)
                    ? currentQuestion.correctFeedback
                    : currentQuestion.correctFeedback;

            if (explanationText != null)
                explanationText.text = explanation;

            SetActiveSafe(characterPanel, true);
            SetActiveSafe(characterHappy, true);
            SetActiveSafe(confettiRight, true);
            SetActiveSafe(confettiLeft, true);

            if (currentQuestionIndex == 0) // First question correct
            {
                if (nextButton != null)
                {
                    SetActiveSafe(nextButton.gameObject, true);

                    TMP_Text buttonText = nextButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null) buttonText.text = "Weiter";

                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(GoToSecondQuestion);
                }
            }
            else // Second question correct
            {
                if (nextButton != null)
                {
                    SetActiveSafe(nextButton.gameObject, true);

                    TMP_Text buttonText = nextButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null) buttonText.text = "Fertig";

                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(() => ChangeState(LevelState.Complete));
                }
            }
        }
        else // Wrong answer
        {
            // register as failed attempt 
            currentProgress.RegisterFailedAttempt();

            // get explanation for the wrong answer they selected
            int selectedIndex = answerSystem.selectedIndex;
            string explanation = currentQuestion.GetExplanation(selectedIndex);

            if (nextButton != null) SetActiveSafe(nextButton.gameObject, false);

            // Use specific explanation if available, otherwise use general feedback
            if (feedbackText != null)
                feedbackText.color = wrongColor;
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

        RegisterLevelPoints(currentQuestionIndex);
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

    // New method to transition to second question
    void GoToSecondQuestion()
    {
        Debug.Log("going to second question");

        currentQuestionIndex = 1;
        currentQuestion = question2; // switch to second question

        if (answerSystem != null)
        {
            answerSystem.ResetButtons();
        }

        HideAllPanels();
        StartCoroutine(OnQuizState()); // ← Reuse parent's quiz state!
    }

    // Override OnRetry for multi-question level
    void OnRetryMultiQuestion()
    {
        Debug.Log("OnRetry called for multi-question level");


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

        // Shuffle the current question's answers
        Shuffle();

        if (answerSystem != null) answerSystem.ResetButtons();

        // Reload current question data (either question1 or question2)
        LoadQuestionData();

        if (submitButton != null && answerSystem != null)
        {
            SetActiveSafe(submitButton.gameObject, true);
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(() => answerSystem.CheckAnswer());
        }
    }

    public void SetKinematic(bool state)
    {
        foreach (Rigidbody box in boxes)
            box.isKinematic = state;
    }
}