using UnityEngine;

public class QuestionProgress : MonoBehaviour
{
    private QuestionData questionData;
    [HideInInspector] public int failedAttempts;
    [HideInInspector] public bool hasFailed;
    [HideInInspector] public bool isCompleted;

    // constructor (called when creating a new instance)
    public QuestionProgress(QuestionData data)
    {
        questionData = data;
        failedAttempts = 0;
        isCompleted = false;
    }

    public QuestionData GetQuestionData() => questionData;
    public int GetCorrectAnswerIndex() => questionData.correctAnswerIndex;

    public void RegisterFailedAttempt()
    {
        failedAttempts++;
        Debug.Log("called from: QuestionProgress. method: registered failed attempt");
    }

    // when player presses submit
    public void MarkCompleted()
    {
        isCompleted = true;
        float points = CalculateEarnedPoints();
        Debug.Log($"called from: QuestionProgress. method: marked as completed with {points}");
    }

    public int CalculateEarnedPoints()
    {
        // safety checks
        if (!isCompleted) return 0;
        if (failedAttempts >= 10) return 0;

        float multiplier = 1f - (failedAttempts * 0.1f);
        return Mathf.RoundToInt(questionData.basePoints * multiplier);
    }

    public int GetFailedAttempts() => failedAttempts;
    public bool IsCompleted() => isCompleted;
}