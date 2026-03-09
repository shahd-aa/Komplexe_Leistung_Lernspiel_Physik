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

public class GameManager_Lvl_05 : MonoBehaviour
{
    [Header("Scripts")]
    public SpeedometerUI speedometer1;
    public SpeedometerUI speedometer2;

    public ForceSliderScript sliderScript1;
    public ForceSliderScript sliderScript2;

    AnimationsUI animationUIScript;
    BlueprintLevel blueprintLevel;
    ForceSliderScript sliderScript;

    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject questionPanel;
    public GameObject feedbackPanel;
    public GameObject wrongNotePanel;
    public GameObject rankPointsPanel;

    [Header("Misc UI Elements")]
    public GameObject confettiRight;
    public GameObject confettiLeft;

    [Header("Buttons")]
    public Button nextButton;
    public Button continueButton;

    [Header("References")]
    public Transform sliderAndSpeedometer;
    public PlayableDirector timeline;
    public List<GameObject> characters = new List<GameObject>();
    public List<Rigidbody> rbCrates = new List<Rigidbody>();
    public List<CinemachineCamera> characterCameras = new List<CinemachineCamera>();
    public GameObject characterStudio;

    [Header("Background Objects")]
    public List<CinemachineCamera> bgCameras = new List<CinemachineCamera>();
    public List<GameObject> bgCharacters = new List<GameObject>();

    [Header("Settings")]
    public float titleDisplayTime = 3f;
    public int levelNumber = 5;

    // private 
    private int failedAttempts = 0;
    private bool isCompleted = false;

    void Start()
    {
        StartCoroutine(TitleState());

        // takes all level specific UI elements at the start and hides them
        DisableUIElements(sliderAndSpeedometer);
        HideAllPanels();

        timeline.stopped += OnCutsceneEnded;

        sliderScript = FindAnyObjectByType<ForceSliderScript>();
        animationUIScript = FindAnyObjectByType<AnimationsUI>();

        // listeners 
        sliderScript.confirmBtn.onClick.AddListener(() => StartCoroutine(CheckBothAccelerations()));
        continueButton.onClick.AddListener(ShowFeedback);
        nextButton.onClick.AddListener(OnCompleteState);
    }

    IEnumerator TitleState()
    {
        Debug.Log("LEVEL STATE: TITLE STATE");

        if (timeline != null)
        {
            timeline.Pause();
        }

        titlePanel.SetActive(true);
        yield return new WaitForSeconds(Mathf.Max(0f, titleDisplayTime));
        titlePanel.SetActive(false);

        CutsceneState();
    }

    void CutsceneState()
    {
        Debug.Log("LEVEL STATE: CUTSCENE STATE");

        ChangeCharCameraState(false);

        if (timeline != null)
        {
            timeline.stopped -= OnCutsceneEnded;
            timeline.stopped += OnCutsceneEnded;
            timeline.Play();
        }
    }

    IEnumerator CheckBothAccelerations()
    {
        if (speedometer1 == null || speedometer2 == null)
        {
            Debug.LogError("Both speedometers must be assigned!", this);
            yield return null;
        }

        // accelerations of both crates 
        float acc1 = sliderScript1.confirmedForce / rbCrates[0].mass;
        float acc2 = sliderScript2.confirmedForce / rbCrates[1].mass;

        yield return new WaitForSeconds(2f);

        // Compare with small tolerance
        if (Mathf.Abs(acc1 - acc2) < 0.01f)
        {
            Debug.Log($"richtig! Beide Kisten haben gleiche Beschleunigung! ({acc1:F2} vs {acc2:F2} m/s²)");

            isCompleted = true;
            RegisterLevelPoints();

            ShowConfetti();
            yield return new WaitForSeconds(2f);
            continueButton.gameObject.SetActive(true);
        }
        else if (acc1 > acc2)
        {
            Debug.Log($"falsch! Kiste 1 ist schneller! ({acc1:F2} vs {acc2:F2} m/s²)");
            failedAttempts++;

            wrongNotePanel.SetActive(true);
            animationUIScript.PopInFadeOut(wrongNotePanel);
        }
        else
        {
            Debug.Log($"falsch! Kiste 2 ist schneller! ({acc2:F2} vs {acc1:F2} m/s²)");
            failedAttempts++;

            wrongNotePanel.SetActive(true);
            animationUIScript.PopInFadeOut(wrongNotePanel);
        }
    }

    void RegisterLevelPoints()
    {
        // calculate points based on failed attempts (same logic as QuestionProgress)
        int maxPoints = 100;
        int pointsPerFailure = 10;
        int earnedPoints = Mathf.Max(0, maxPoints - (failedAttempts * pointsPerFailure));

        // ALWAYS save points
        GameProgress.SaveLevelPoints(levelNumber, earnedPoints);
        Debug.Log($"level number: {levelNumber}, total points: {earnedPoints}, failed attempts: {failedAttempts}");

        // Show animation if completed
        if (isCompleted && rankPointsPanel != null)
        {
            TextMeshProUGUI rankPointsText = rankPointsPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (rankPointsText != null)
            {
                rankPointsText.text = earnedPoints.ToString() + " Punkte";
                animationUIScript.PopInFadeOut(rankPointsPanel);
            }
        }
    }

    void ShowConfetti()
    {
        confettiRight.SetActive(true);
        confettiLeft.SetActive(true);
    }

    void ShowFeedback()
    {
        // show feedback panel
        feedbackPanel.SetActive(true);
        continueButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(true);
        characterStudio.SetActive(true);
        PauseMovement(rbCrates);
        PauseAnimators(characters);
    }

    // HELPER FUNCTIONS

    void PauseMovement(List<Rigidbody> list)
    {
        Debug.Log("movement is paused");
        foreach (Rigidbody rb in list)
        {
            rb.isKinematic = true;
        }
    }

    void PauseAnimators(List<GameObject> list)
    {
        Debug.Log("animators paused");
        foreach (GameObject character in list)
        {
            Animator anim = character.GetComponent<Animator>();
            anim.speed = 0f;
        }
    }

    void DisableBgObjects(List<CinemachineCamera> cams, List<GameObject> characters)
    {
        foreach (CinemachineCamera cam in cams)
        {
            cam.gameObject.SetActive(false);
        }

        foreach (GameObject character in characters)
        {
            character.SetActive(false);
        }
    }

    void HideAllPanels()
    {
        // panels
        questionPanel.SetActive(false);
        wrongNotePanel.SetActive(false);
        feedbackPanel.SetActive(false);
        rankPointsPanel.SetActive(false);

        // character feedback 
        characterStudio.SetActive(false);

        // buttons
        nextButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        // misc
        confettiRight.SetActive(false);
        confettiLeft.SetActive(false);
    }

    void OnCompleteState()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    void OnCutsceneEnded(PlayableDirector d)
    {
        Debug.Log("Timeline finished!");
        // after timeline is finished, show question
        animationUIScript.PopInPopOut(questionPanel.GetComponent<RectTransform>(), 0.2f, 5f);

        // default to the split screen (forever active)
        ChangeCharCameraState(true);
        EnableUIElements(sliderAndSpeedometer);

        DisableBgObjects(bgCameras, bgCharacters);
    }

    void ChangeCharCameraState(bool state)
    {
        foreach (CinemachineCamera cam in characterCameras)
        {
            cam.gameObject.SetActive(state);
        }
    }

    // UI FUNCTIONS 

    void DisableUIElements(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void EnableUIElements(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
