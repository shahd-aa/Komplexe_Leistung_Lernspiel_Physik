using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

public class QuizIntroAnimation : MonoBehaviour
{
    [Header("Letter References")]
    public RectTransform[] letters; // Drag your letter UI elements here!
    
    [Header("Animation Settings")]
    [Tooltip("How long each letter takes to pop in")]
    public float popDuration = 0.4f;
    
    [Tooltip("Delay between each letter popping")]
    public float delayBetweenLetters = 0.15f;
    
    [Tooltip("Final scale of letters (1 = normal size)")]
    public float targetScale = 1f;
    
    [Header("Rotation Settings")]
    [Tooltip("Enable random rotation for letters")]
    public bool enableRotation = true;
    
    [Tooltip("Min rotation angle in degrees")]
    public float minRotation = -15f;
    
    [Tooltip("Max rotation angle in degrees")]
    public float maxRotation = 15f;
    
    [Header("Easing")]
    [Tooltip("The bounce/spring effect type")]
    public Ease easeType = Ease.OutBack;
    
    [Header("Fade Out")]
    [Tooltip("How long to wait before fading out")]
    public float waitBeforeFadeOut = 1f;
    
    [Tooltip("How long the fade out takes")]
    public float fadeOutDuration = 0.5f;
    
    [Header("Events")]
    [Tooltip("What happens after animation finishes")]
    public UnityEngine.Events.UnityEvent onAnimationComplete;
    
    [Header("Auto Play")]
    [Tooltip("Should the animation play automatically on start?")]
    public bool autoPlayOnStart = true;
    
    void Start()
    {
        // Hide all letters at start (including children inside panels)
        foreach (RectTransform letter in letters)
        {
            letter.localScale = Vector3.zero;
            
            // Also hide any child elements (like text inside the panel)
            foreach (Transform child in letter)
            {
                child.localScale = Vector3.zero;
            }
        }
        
        // Auto-play if enabled
        if (autoPlayOnStart)
        {
            PlayIntroAnimation();
        }
    }
    
    public void PlayIntroAnimation()
    {
        StartCoroutine(AnimateLetters());
    }
    
    // Use this version if you want to WAIT for the animation to finish
    public IEnumerator PlayIntroAnimationCoroutine()
    {
        yield return StartCoroutine(AnimateLetters());
    }
    
    IEnumerator AnimateLetters()
    {
        // Animate each letter one by one
        for (int i = 0; i < letters.Length; i++)
        {
            RectTransform letter = letters[i];
            
            // Pop in animation for the panel
            letter.DOScale(targetScale, popDuration).SetEase(easeType);
            
            // Also animate children (the actual letter/symbol inside)
            foreach (Transform child in letter)
            {
                child.localScale = Vector3.zero;
                child.DOScale(targetScale, popDuration).SetEase(easeType);
            }
            
            // Optional rotation
            if (enableRotation)
            {
                float randomRotation = Random.Range(minRotation, maxRotation);
                letter.DORotate(new Vector3(0, 0, randomRotation), popDuration).SetEase(easeType);
            }
            
            // Wait before next letter
            yield return new WaitForSeconds(delayBetweenLetters);
        }
        
        // Wait a bit before fading out
        yield return new WaitForSeconds(waitBeforeFadeOut);
        
        // Fade out all letters
        foreach (RectTransform letter in letters)
        {
            Image img = letter.GetComponent<Image>();
            if (img != null)
            {
                img.DOFade(0, fadeOutDuration);
            }
            
            // Also fade text if it has TextMeshPro
            TMPro.TextMeshProUGUI text = letter.GetComponent<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.DOFade(0, fadeOutDuration);
            }
            
            // Fade children too
            foreach (Transform child in letter)
            {
                Image childImg = child.GetComponent<Image>();
                if (childImg != null)
                {
                    childImg.DOFade(0, fadeOutDuration);
                }
                
                TMPro.TextMeshProUGUI childText = child.GetComponent<TMPro.TextMeshProUGUI>();
                if (childText != null)
                {
                    childText.DOFade(0, fadeOutDuration);
                }
            }
        }
        
        // Wait for fade to complete
        yield return new WaitForSeconds(fadeOutDuration);
        
        // Trigger event (like showing the quiz)
        onAnimationComplete?.Invoke();
    }
    
    // Call this to play the animation (useful for buttons or other scripts)
    public void TriggerAnimation()
    {
        PlayIntroAnimation();
    }
}