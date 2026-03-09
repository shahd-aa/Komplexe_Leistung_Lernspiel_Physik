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

public class AnimationsUI : MonoBehaviour
{
    public void PopInFadeOut(GameObject element)
    {
        RectTransform rt = element.GetComponent<RectTransform>();
        CanvasGroup cg = element.GetComponent<CanvasGroup>();

        // Kill any existing tweens on this element to prevent stacking
        rt.DOKill();
        cg.DOKill();


        element.SetActive(true);

        // Reset
        rt.localScale = Vector3.zero;
        cg.alpha = 0f;

        // POP animation
        rt.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack)
          .OnComplete(() =>
          {
              // Settle back to normal size
              rt.DOScale(1f, 0.1f);
          });

        // Fade in
        cg.DOFade(1f, 0.2f);

        // Fade out after 3 seconds
        cg.DOFade(0f, 0.5f).SetDelay(3f)
          .OnComplete(() => element.SetActive(false));
    }

    public void PopInPopOut(RectTransform target, float popTime = 0.2f, float stayTime = 2f)
    {
        target.gameObject.SetActive(true);

        // Start tiny
        target.localScale = Vector3.zero;

        // POP IN
        target.DOScale(1.2f, popTime).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Settle to normal size
                target.DOScale(1f, 0.1f);
            });

        // POP OUT after delay
        DOVirtual.DelayedCall(stayTime, () =>
        {
            target.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    target.gameObject.SetActive(false);
                });
        });
    }
}
