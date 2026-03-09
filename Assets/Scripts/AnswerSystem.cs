using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnswerSystem : MonoBehaviour
{
    // ============================================================
    // INSPECTOR FIELDS
    // ============================================================
    
    [Header("References")]
    public Transform AnswerOptionsPanel;
    
    // ============================================================
    // PRIVATE FIELDS
    // ============================================================
    
    private BlueprintLevel blueprintLevel;
    private List<Button> answerButtons = new List<Button>();
    
    [HideInInspector] public int correctIndex;
    [HideInInspector] public bool isCorrect;
    public int selectedIndex = -1;

    // ============================================================
    // INITIALIZATION
    // ============================================================
    
    private void Awake()
    {
        // Find BlueprintLevel reference
        if (blueprintLevel == null)
        {
            blueprintLevel = FindAnyObjectByType<BlueprintLevel>();
            if (blueprintLevel == null)
            {
                Debug.LogError("BlueprintLevel reference missing and cannot be found!", this);
                enabled = false;
                return;
            }
        }

        SetupButtons();
    }

    public void SetBlueprintLevel(BlueprintLevel level)
    {
        blueprintLevel = level;
    }

    void SetupButtons()
    {
        answerButtons.Clear();

        if (AnswerOptionsPanel == null)
        {
            Debug.LogError("AnswerOptionsPanel is null!", this);
            return;
        }

        // Cache buttons
        foreach (Transform child in AnswerOptionsPanel)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null)
            {
                answerButtons.Add(btn);

                // Capture index correctly
                int index = answerButtons.Count - 1;
                btn.onClick.AddListener(() => OnAnswerClicked(index));
            }
        }

        if (answerButtons.Count == 0)
        {
            Debug.LogWarning("No answer buttons found under AnswerOptionsPanel!", this);
        }
    }

    // ============================================================
    // ANSWER SELECTION & CHECKING
    // ============================================================
    
    void OnAnswerClicked(int index)
    {
        // Validate index
        if (index < 0 || index >= answerButtons.Count)
        {
            Debug.LogError($"Invalid answer index: {index}", this);
            return;
        }

        selectedIndex = index;
        Debug.Log($"Selected answer index: {index}");
    }

    public void CheckAnswer()
    {
        // Validate selection
        if (selectedIndex < 0)
        {
            Debug.LogWarning("No answer selected!");
            return;
        }

        // Check correctness
        isCorrect = (selectedIndex == correctIndex);

        // Disable interaction
        DisableAllButtons();

        // Notify BlueprintLevel to show feedback
        if (blueprintLevel != null)
        {
            blueprintLevel.ChangeState(LevelState.Feedback);
        }
        else
        {
            Debug.LogError("Cannot transition to Feedback - BlueprintLevel is null!", this);
        }
    }

    void DisableAllButtons()
    {
        foreach (Button btn in answerButtons)
        {
            if (btn != null)
            {
                btn.interactable = false;
            }
        }
    }

    // ============================================================
    // RESET
    // ============================================================
    
    public void ResetButtons()
    {
        // Reset all buttons to default state
        foreach (Button btn in answerButtons)
        {
            if (btn != null)
            {
                btn.interactable = true;
            }
        }

        selectedIndex = -1;
        isCorrect = false;
    }

    // ============================================================
    // CLEANUP
    // ============================================================
    
    private void OnDestroy()
    {
        // Remove all listeners to prevent memory leaks
        foreach (Button btn in answerButtons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
            }
        }
    }

    // ============================================================
    // PUBLIC GETTERS
    // ============================================================
    
    public bool HasSelection()
    {
        return selectedIndex >= 0;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }
}