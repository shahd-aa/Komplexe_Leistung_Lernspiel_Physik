using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager_Lvl_06 : MonoBehaviour
{

    [Header("Team 2 UI (Left Side)")]
    public Button addCharBtn_Team2;
    public Button removeCharBtn_Team2;

    [Header("Game Control UI")]
    public Button submitBtn;
    public Button backBtn;
    public Button nextBtn;
    public GameObject team2WinnerPanel;

    [Header("References")]
    public Transform rope; // THE ONE ROPE both teams are pulling
    public Rigidbody rbRope;
    public Transform team1Container; // Container holding Team 1 characters 
    public Transform team2Container; // Container holding Team 2 characters 

    [Header("Spacing Settings")]
    public float characterSpacing = 1f; // Distance between characters

    [Header("Animator Speed Settings")]
    public float minAnimSpeed = 0.75f;
    public float maxAnimSpeed = 1f;

    [Header("Pull Settings")]
    public float baseForce = 5f; // force per character
    public float baseResistance = 5f;

    // Private vars for Team 1
    public List<GameObject> allCharacters_Team1 = new List<GameObject>();
    private GameObject defaultChar_Team1;
    public int activeCharacterCount_Team1 = 1;
    private List<Vector3> initialPositions_Team1 = new List<Vector3>();

    // Private vars for Team 2
    public List<GameObject> allCharacters_Team2 = new List<GameObject>();
    private GameObject defaultChar_Team2;
    public int activeCharacterCount_Team2 = 1;
    private List<Vector3> initialPositions_Team2 = new List<Vector3>();

    // Private vars for pulling
    private bool isPulling = false;
    private float resistanceForce_Team1 = 0f;
    private float currentPullForce_Team2 = 0f;
    private float netPullForce = 0f; // Net force (team1 - team2)
    private Vector3 netPullDirection = Vector3.zero;

    // initial position rope
    private Vector3 initialRopePos;

    void Start()
    {
        DisableElementsUI();
        // Initialize Team 1 (right side)
        InitializeTeam(team1Container, allCharacters_Team1, ref defaultChar_Team1, 2, ref activeCharacterCount_Team1);
        StartCoroutine(GetInitialPositions(allCharacters_Team1, initialPositions_Team1));

        // Initialize Team 2 (left side)
        InitializeTeam(team2Container, allCharacters_Team2, ref defaultChar_Team2, 1, ref activeCharacterCount_Team2);
        StartCoroutine(GetInitialPositions(allCharacters_Team2, initialPositions_Team2));

        // Add listeners to buttons
        addCharBtn_Team2.onClick.AddListener(() => AddChar(2));
        removeCharBtn_Team2.onClick.AddListener(() => RemoveChar(2));
        submitBtn.onClick.AddListener(OnSubmit);
        backBtn.onClick.AddListener(OnBack);
        nextBtn.onClick.AddListener(BeginMultipleChoice);

        UpdateButtonStates();

        // get initial pos of rope after delay
        StartCoroutine(GetRopePos(2f));
    }

    IEnumerator GetRopePos(float time)
    {
        yield return new WaitForSeconds(time);
        initialRopePos = rope.position;
    }

    public int ReturnActiveCharacters()
    {
        Debug.Log($"active character count is {activeCharacterCount_Team2}");
        return activeCharacterCount_Team2;
    }

    void InitializeTeam(Transform container, List<GameObject> characterList, ref GameObject defaultChar, int defaultCount, ref int activeCount)
    {
        for (int i = 0; i < container.childCount; i++)
        {
            Transform character = container.GetChild(i);
            characterList.Add(character.gameObject);
            character.gameObject.SetActive(false);

            // Set animator speed to 0 initially
            Animator animator = character.GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 0f;
            }
        }

        // set default character active
        int charsToActivate = Mathf.Min(defaultCount, characterList.Count);

        for (int i = 0; i < charsToActivate; i++)
        {
            characterList[i].SetActive(true);
        }

        // set active count
        activeCount = charsToActivate;

        // Set the first one as the defaultChar reference
        if (characterList.Count > 0)
        {
            defaultChar = characterList[0];
        }

    }

    IEnumerator GetInitialPositions(List<GameObject> characterList, List<Vector3> initialPositions)
    {
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < characterList.Count; i++)
        {
            initialPositions.Add(characterList[i].transform.position);
        }
    }

    void OnSubmit()
    {
        isPulling = true;

        // Reset result parameters before animating
        ResetResultParameters(allCharacters_Team2, activeCharacterCount_Team2);

        // animate teams
        AnimatePulling(allCharacters_Team2, activeCharacterCount_Team2, "Team 2");
        AnimateHolding(allCharacters_Team1, activeCharacterCount_Team1, "Team 1");

        // calculate pull force based off of amount of characters
        resistanceForce_Team1 = baseResistance * activeCharacterCount_Team1;
        currentPullForce_Team2 = baseForce * activeCharacterCount_Team2;

        // which side is pulling more
        float forceDifference = resistanceForce_Team1 - currentPullForce_Team2;

        netPullForce = Mathf.Abs(forceDifference);

        // determine direction based on which team is stronger
        if (currentPullForce_Team2 > resistanceForce_Team1)
        {
            rbRope.AddForce(Vector3.left * netPullForce);
            Debug.Log($"team 2 is stronger than the resistance by team 1. force: {Mathf.Abs(forceDifference)}");
        }
        else if (currentPullForce_Team2 == resistanceForce_Team1)
        {
            netPullForce = 0f;
            AnimateHolding(allCharacters_Team1, activeCharacterCount_Team1, "Team 1");

        }
        else
        {
            netPullForce = 0f;
            AnimateHolding(allCharacters_Team1, activeCharacterCount_Team1, "Team 1");
            Debug.Log($"team 1 has stronger resistance. force: {Mathf.Abs(forceDifference)}");
        }

        // Disable all buttons during pulling
        DisableAllButtons();
    }

    void OnBack()
    {
        // Stop pulling
        isPulling = false;

        // reset rope
        rope.position = initialRopePos;
        rbRope.linearVelocity = Vector3.zero;
        rbRope.isKinematic = false;

        // Reset Team 1 animators
        ResetAnimations(allCharacters_Team1, activeCharacterCount_Team1, initialPositions_Team1);

        // Reset Team 2 animators
        ResetAnimations(allCharacters_Team2, activeCharacterCount_Team2, initialPositions_Team2);

        // Re-enable buttons
        UpdateButtonStates();
        submitBtn.interactable = true;

        Debug.Log("Back pressed! Reset to initial state");
    }

    // ---------------------- ANIMATORS ------------------------------

    void ResetTeamAnimators(List<GameObject> team, int activeCount)
    {
        for (int i = 0; i < activeCount; i++)
        {
            Animator animator = team[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 0f;
            }
        }
    }

    void ResetAnimations(List<GameObject> team, int activeCount, List<Vector3> initialPositions)
    {
        for (int i = 0; i < activeCount; i++)
        {
            Animator animator = team[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 0f;
                animator.Rebind();
                animator.Update(0f);
            }

            // Reset each character to their own initial position
            if (i < initialPositions.Count)
            {
                team[i].transform.position = initialPositions[i];
            }
        }
    }

    void AnimatePulling(List<GameObject> team, int activeCount, string teamName)
    {
        for (int i = 0; i < activeCount; i++)
        {
            Animator animator = team[i].GetComponent<Animator>();
            if (animator != null)
            {
                float randomSpeed = Random.Range(minAnimSpeed, maxAnimSpeed);
                animator.speed = randomSpeed;
                Debug.Log($"{teamName} - {team[i].name} animator speed: {randomSpeed}");
            }
        }
    }

    // Update this method to accept blend value
    void AnimateHolding(List<GameObject> team, int activeCount, string teamName)
    {
        for (int i = 0; i < activeCount; i++)
        {
            Animator animator = team[i].GetComponent<Animator>();
            if (animator != null)
            {
                if (isPulling)
                {
                    if (animator.HasState(0, Animator.StringToHash("Base Layer.walking backwards anim")))
                    {
                        Debug.Log("playing walking backwards");
                        animator.CrossFade("Base Layer.walking backwards anim", 0.1f);
                        animator.speed = 0.25f;
                    }
                    else
                    {
                        Debug.Log("State not found");
                    }
                }
                else
                {
                    if (animator.HasState(0, Animator.StringToHash("Base Layer.holding rope idle anim")))
                    {
                        Debug.Log("playing idle holding");
                        animator.Play("Base Layer.holding rope idle anim");
                        animator.speed = 1f;
                    }
                    else
                    {
                        Debug.Log("State not found");
                    }
                }
            }
        }
    }

    void AnimateWinner(List<GameObject> team, int activeCount, bool won)
    {
        isPulling = false;

        for (int i = 0; i < activeCount; i++)
        {
            Animator animator = team[i].GetComponent<Animator>();
            if (animator != null)
            {
                int index = Random.Range(1, 4); // 1, 2, or 3
                animator.SetInteger("resultIndex", index);
                animator.SetBool("hasWon", won);
            }
        }
    }

    public void DetermineWinner(int teamWon)
    {
        isPulling = false;

        // stop team 1 from animating
        AnimateHolding(allCharacters_Team1, activeCharacterCount_Team1, "Team 1");

        // stop rope from moving
        rbRope.isKinematic = true;

        if (teamWon == 2)
        {
            AnimateWinner(allCharacters_Team2, activeCharacterCount_Team2, true);
        }
        else
        {
            Debug.Log("invalid winner");
            return;
        }
    }

    void ResetResultParameters(List<GameObject> team, int activeCount)
    {
        for (int i = 0; i < activeCount; i++)
        {
            Animator animator = team[i].GetComponent<Animator>();
            if (animator != null)
            {
                // Reset to default "pulling" state parameters
                animator.SetBool("hasWon", false);
                animator.SetInteger("resultIndex", -1); // use -1 to not trigger win/lose states
            }
        }
    }

    // ---------------------- BUTTONS ------------------------------
    void UpdateButtonStates()
    {
        // Team 2 buttons
        addCharBtn_Team2.interactable = (activeCharacterCount_Team2 < allCharacters_Team2.Count);
        removeCharBtn_Team2.interactable = (activeCharacterCount_Team2 > 1);
    }

    void DisableAllButtons()
    {
        addCharBtn_Team2.interactable = false;
        removeCharBtn_Team2.interactable = false;
        submitBtn.interactable = false;
    }

    void AddChar(int team)
    {
        if (team == 1)
        {
            if (activeCharacterCount_Team1 >= allCharacters_Team1.Count)
            {
                Debug.Log("Team 1: No more characters to add!");
                return;
            }

            GameObject nextChar = allCharacters_Team1[activeCharacterCount_Team1];
            nextChar.SetActive(true);

            activeCharacterCount_Team1++;
            Debug.Log($"Team 1: Added character! Total active: {activeCharacterCount_Team1}");
        }
        else if (team == 2)
        {
            if (activeCharacterCount_Team2 >= allCharacters_Team2.Count)
            {
                Debug.Log("Team 2: No more characters to add!");
                return;
            }

            GameObject nextChar = allCharacters_Team2[activeCharacterCount_Team2];
            nextChar.SetActive(true);

            activeCharacterCount_Team2++;
            Debug.Log($"Team 2: Added character! Total active: {activeCharacterCount_Team2}");
        }

        UpdateButtonStates();
    }

    void RemoveChar(int team)
    {
        if (team == 1)
        {
            if (activeCharacterCount_Team1 <= 2) // can't go below 2 for team 1
            {
                Debug.Log("error: default characters cant be removed");
                return;
            }

            activeCharacterCount_Team1--;
            allCharacters_Team1[activeCharacterCount_Team1].SetActive(false);
            Debug.Log($"team 1: character removed. total active: {activeCharacterCount_Team1}");
        }
        else if (team == 2)
        {
            if (activeCharacterCount_Team2 <= 1)
            {
                Debug.Log("error: default character cant be removed");
                return;
            }

            activeCharacterCount_Team2--;
            allCharacters_Team2[activeCharacterCount_Team2].SetActive(false);
            Debug.Log($"team 2: character removed. total active: {activeCharacterCount_Team2}");
        }

        UpdateButtonStates();
    }

    void DisableElementsUI()
    {
        team2WinnerPanel.SetActive(false);
    }

    // ---------------------- EXTRA ------------------------------

    public void StopPulling()
    {
        isPulling = false;
        AnimateHolding(allCharacters_Team1, activeCharacterCount_Team1, "Team 1");
        Debug.Log("pulling stopped");
    }

    public void BeginMultipleChoice()
    {
        // run multiple choice
    }
}