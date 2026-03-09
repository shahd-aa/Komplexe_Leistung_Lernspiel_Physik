using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class BallSquish : MonoBehaviour
{
    public Animator ballAnimator;     // Animator on BallVisual child

    // force arrows up and down
    public Image arrowUp;
    public Image arrowDown;
    public float displayDuration = 2f;
    public float floatDistance = 50f; // UI units
    public float floatSpeed = 1f;

    private bool hasHitGround = false;


    void Start()
    {
        arrowUp.gameObject.SetActive(false);
        arrowDown.gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHitGround) return;

        if (collision.gameObject.CompareTag("ground"))
        {
            ballAnimator.SetTrigger("DoSquish");

            hasHitGround = true;
            ShowArrows();
        }
    }

    void ShowArrows()
    {
        arrowUp.gameObject.SetActive(true);
        arrowDown.gameObject.SetActive(true);

        arrowUp.DOFade(1f, 0f);
        arrowDown.DOFade(1f, 0f);

        float duration = displayDuration / floatSpeed;

        arrowUp.rectTransform.DOAnchorPosY(arrowUp.rectTransform.anchoredPosition.y + floatDistance, duration);
        arrowUp.DOFade(0f, duration);

        arrowDown.rectTransform.DOAnchorPosY(arrowDown.rectTransform.anchoredPosition.y - floatDistance, duration);
        arrowDown.DOFade(0f, duration).OnComplete(() =>
        {
            arrowUp.gameObject.SetActive(false);
            arrowDown.gameObject.SetActive(false);
        });
    }
}