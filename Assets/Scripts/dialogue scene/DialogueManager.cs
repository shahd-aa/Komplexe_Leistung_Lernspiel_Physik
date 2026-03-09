using UnityEngine;
using TMPro; // for UI 
using UnityEngine.SceneManagement; // for switching scenes
using System.Collections; // for coroutines (timed actions)

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")] // attribute that helps organize inspector view
    // variables
    public TextMeshProUGUI dialogueText;
    public GameObject continueButton;

    [Header("Dialogue Content")]
    public string[] dialogueLines; // array for dialogue lines

    [Header("Settings")]
    public float textSpeed = 0.05f;
    public bool HasRankAssignment;
    public AudioSource typingSoundEffect;

    [Header("Scripts")]
    public ResultsUI rankResultScript;

    [Header("Character Animation")]
    public Animator characterAnim;

    public int currentLineIndex = 0;
    public bool IsGameEnd;
    private bool isTyping = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        continueButton.SetActive(false); // hides the button at the start
        StartCoroutine(TypeLine()); // starts typing first line 
    }

    IEnumerator TypeLine() // coroutine that lets u pause between actions
    {
        isTyping = true;
        typingSoundEffect.Play();
        dialogueText.text = "";

        foreach (char c in dialogueLines[currentLineIndex].ToCharArray()) // loop through each character in the line
        {
            dialogueText.text += c; // adds one character at a time
            yield return new WaitForSeconds(textSpeed); // pause between each character (typing effect) 
        }

        isTyping = false; // false after typing
        typingSoundEffect.Stop();
        continueButton.SetActive(true);
    }

    public void NextLine() // method called when player clicks continue button
    {
        if (isTyping)
        {
            StopAllCoroutines(); // stops typing animation
            dialogueText.text = dialogueLines[currentLineIndex]; // shows full line
            isTyping = false;
            continueButton.SetActive(true);
            return;
        }

        continueButton.SetActive(false); // hides button when next line types
        currentLineIndex++; // move to next line in the array

        ChangeAnimation();


        if (HasRankAssignment)
        {
            if (currentLineIndex == 3)
            {
                dialogueLines[3] = dialogueLines[3].Replace("{total}", GameProgress.GetTotalPoints().ToString());
                dialogueLines[3] = dialogueLines[3].Replace("{rank}", GameProgress.GetRank().ToString());
                rankResultScript.EnableRankDisplay(true);
            }
        }

        if (currentLineIndex < dialogueLines.Length) // if the current line number is less than the total number of lines…
        {
            StartCoroutine(TypeLine()); // type next one
        }
        else
        {
            if (IsGameEnd)
            {
                Application.Quit();
                Debug.Log("player quit the game");
            }
            else
            {
                EndDialogue();
            }
        }
    }

    public void ChangeAnimation()
    {
        int animationCounter = characterAnim.GetInteger("animationCounter");
        animationCounter++;
        characterAnim.SetInteger("animationCounter", animationCounter);
    }

    void EndDialogue()
    {
        Debug.Log("Cutscene finished, loading level 1...");
        SceneManager.LoadScene("Level_1");
    }
}
