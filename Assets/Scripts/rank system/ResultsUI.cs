using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultsUI : MonoBehaviour
{
    public DialogueManager dialogueScript;

    [Header("Settings")]
    public ParticleSystem sparkles;
    public TextMeshProUGUI rankText;
    public Image rankIcon;

    [Header("Rank Icons")]
    public Sprite bronzeIcon;
    public Sprite silberIcon;
    public Sprite goldIcon;
    public Sprite meisterIcon;
    public Sprite eliteIcon;

    void Start()
    {
        EnableRankDisplay(false);

        int totalPoints = GameProgress.GetTotalPoints();
        Debug.Log($"total points: {totalPoints}");

        string rank = GameProgress.GetRank();
        Debug.Log($"rank: {rank}");
        rankText.text = rank;
        rankIcon.sprite = GetIconForRank(rank);
    }

    private Sprite GetIconForRank(string rank)
    {
        if (rank == "Elite") return eliteIcon;
        if (rank == "Meister") return meisterIcon;
        if (rank == "Gold") return goldIcon;
        if (rank == "Silber") return silberIcon;
        return bronzeIcon;
    }

    public void EnableRankDisplay(bool state)
    {
        if (state == true)
        {
            sparkles.gameObject.SetActive(true);
            rankIcon.gameObject.SetActive(true);
        }
        else
        {
            sparkles.gameObject.SetActive(false);
            rankIcon.gameObject.SetActive(false);
        }
    }
}
