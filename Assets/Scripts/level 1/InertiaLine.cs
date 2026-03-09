using System.Collections;
using UnityEngine;
using DG.Tweening;

public class InertiaLine : MonoBehaviour
{
    public float delayBetween = 0.2f;
    public float growDuration = 0.3f;
    public float displayDuration = 2f;
    public float shrinkDuration = 0.5f;

    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        StartCoroutine(ActivateChildren());
    }

    IEnumerator ActivateChildren()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
            AnimateChild(child);
            yield return new WaitForSeconds(delayBetween);
        }
    }

    void AnimateChild(Transform child)
    {
        Vector3 targetScale = child.localScale;
        child.localScale = new Vector3(0f, targetScale.y, targetScale.z);

        // grow to original scale
        child.DOScaleX(targetScale.x, growDuration).OnComplete(() =>
        {
            DOVirtual.DelayedCall(displayDuration, () =>
            {
                // shrink back to 0
                child.DOScaleX(0f, shrinkDuration).OnComplete(() =>
                    child.gameObject.SetActive(false));
            });
        });
    }
}