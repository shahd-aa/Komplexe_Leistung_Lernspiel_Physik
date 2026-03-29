using DG.Tweening;
using UnityEngine;
using System.Collections;

public class DialogueUIAnimations : MonoBehaviour
{

    [Header("GameObjects")]
    public GameObject gameIconObject;
    public GameObject[] newtonLawsObjects;
    public GameObject[] sevenLevelsObjects;
    public GameObject[] rankObjects;

    [Header("Script")]
    public DialogueManager dialogueScript;
    public ParticleSystem sparkles;

    [Header("Panel")]
    public Transform imagesPanel; // The parent panel containing all objects
    public GameObject newtonLawsPanel;
    public GameObject rankPanels;
    public GameObject sevenLevelsPanel;

    [Header("Settings")]
    public float zOffsetPerRank = 1f;
    public float delayBetweenRanks = 0.5f;
    public float delayToDisplay;

    [Header("Trigger Settings")]
    public int gameIconLineIndex = 0;
    public int newtonsLawsIndex = 1;
    public int sevenLevelsIndex = 2;
    public int ranksLineIndex = 3;

    // private
    private bool[] triggersActivated;
    private Vector3 originalSparklePos;

    void Start()
    {
        triggersActivated = new bool[10];

        // Hide all objects initially
        HideAllObjects();

        foreach (var obj in rankObjects)
        {
            obj.transform.localScale = Vector3.zero;
        }

        originalSparklePos = sparkles.transform.position;
        sparkles.gameObject.SetActive(false);

        StartCoroutine(CheckForTriggers());
    }

    void HideAllObjects()
    {
        // Loop through all children in the panel
        foreach (Transform child in imagesPanel)
        {
            child.gameObject.SetActive(false);
        }
    }

    void ShowOnlyThisObject(GameObject objectToShow)
    {
        // First hide everything
        HideAllObjects();

        // Then show only the one we want
        objectToShow.SetActive(true);
    }

    IEnumerator CheckForTriggers()
    {
        while (true)
        {
            int currentLine = dialogueScript.currentLineIndex;

            // game icon trigger
            if (currentLine == gameIconLineIndex && !triggersActivated[0])
            {
                triggersActivated[0] = true;
                ShowOnlyThisObject(gameIconObject);
                PopObject(gameIconObject, 0.5f, delayToDisplay);
            }

            // newtons laws trigger
            if (currentLine == newtonsLawsIndex && !triggersActivated[1])
            {
                triggersActivated[1] = true;
                ShowOnlyThisObject(newtonLawsPanel);

                // Hide all children in the panel first
                foreach (Transform child in newtonLawsPanel.transform)
                {
                    child.gameObject.SetActive(false);
                }

                // Pop each newton law object one by one
                foreach (GameObject obj in newtonLawsObjects)
                {
                    obj.SetActive(true);
                    PopObject(obj, 0.5f, delayToDisplay);
                    yield return new WaitForSeconds(1f);
                }
            }

            // seven levels trigger
            if (currentLine == sevenLevelsIndex && !triggersActivated[2])
            {
                triggersActivated[2] = true;
                ShowOnlyThisObject(sevenLevelsPanel);

                foreach (Transform child in sevenLevelsPanel.transform)
                {
                    child.gameObject.SetActive(false);
                }

                foreach (GameObject obj in sevenLevelsObjects)
                {
                    obj.SetActive(true);
                    PopObject(obj, 0.5f, delayToDisplay);
                    yield return new WaitForSeconds(0.5f);
                }
            }

            // ranks trigger
            if (currentLine == ranksLineIndex && !triggersActivated[3])
            {
                triggersActivated[3] = true;
                HideAllObjects();
                rankPanels.SetActive(true);

                // Show all rank objects
                foreach (var obj in rankObjects)
                {
                    obj.SetActive(true);
                }
                StartCoroutine(PopRanksInSequence());
            }

            yield return null;
        }
    }

    IEnumerator PopRanksInSequence()
    {
        sparkles.gameObject.SetActive(true);

        for (int i = 0; i < rankObjects.Length; i++)
        {
            rankObjects[i].transform
                .DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack);

            sparkles.transform.position = new Vector3(
                originalSparklePos.x,
                originalSparklePos.y,
                originalSparklePos.z + (i * zOffsetPerRank)
            );

            // ← stop and clear old particles first
            sparkles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // ← reset scale instantly to full, no tween on the PS itself
            sparkles.transform.localScale = Vector3.one;

            sparkles.Play();

            yield return new WaitForSeconds(delayBetweenRanks);
        }

        // ← stop after last rank
        yield return new WaitForSeconds(1f);
        sparkles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void PopObject(GameObject obj, float duration, float delay, Ease easeType = Ease.OutBack)
    {
        obj.transform.localScale = Vector3.zero;
        obj.transform
            .DOScale(1f, duration)
            .SetEase(easeType)
            .SetDelay(delay);
    }
}