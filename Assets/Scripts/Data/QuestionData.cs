using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Question_1", menuName = "Quiz/Question_1")]
public class QuestionData : ScriptableObject
{
    [Header("Question Content")]
    [TextArea(3, 6)]
    public string questionText;
    
    [Header("Answers")]
    public List<AnswerOption> answers = new List<AnswerOption>();
    
    [Header("Correct Answer")]
    public int correctAnswerIndex;
    
    [Header("General Feedback")]
    [TextArea(2, 4)]
    public string correctFeedback = "Richtig! Gut gemacht!";
    
    [TextArea(2, 4)]
    public string wrongFeedback = "Leider falsch. Versuch es nochmal!";

    [Header("General Feedback")]
    public int basePoints;
    
    private void OnValidate()
    {
        if (answers.Count < 2)
            Debug.LogWarning($"Question '{name}' needs at least 2 answers!");
        
        if (correctAnswerIndex < 0 || correctAnswerIndex >= answers.Count)
        {
            Debug.LogError($"Question '{name}' has invalid correctAnswerIndex!");
            correctAnswerIndex = 0;
        }
    }
    
    public bool IsCorrect(int selectedIndex)
    {
        return selectedIndex == correctAnswerIndex;
    }
    
    // Get the explanation for a specific answer
    public string GetExplanation(int answerIndex)
    {
        if (answerIndex < 0 || answerIndex >= answers.Count)
            return "";
        
        return answers[answerIndex].explanation;
    }
    
    // Get the answer text for a specific index
    public string GetAnswerText(int answerIndex)
    {
        if (answerIndex < 0 || answerIndex >= answers.Count)
            return "";
        
        return answers[answerIndex].answerText;
    }
}

[System.Serializable]
public class AnswerOption
{
    [TextArea(1, 3)]
    public string answerText;
    
    [TextArea(2, 5)]
    public string explanation;
}