using UnityEngine;
using System.Collections;

public class WalkingScript : MonoBehaviour
{
    public Animator walkingAnim;
    
    void Start()
    {
        StartCoroutine(WalkToKitchen());
    }

    // Fixed: Changed return type from void to string
    string GetClipName(Animator animator)
    {
        // Fetch the current Animation clip information for the base layer
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        
        if (clipInfo != null && clipInfo.Length > 0)
        {
            string clipName = clipInfo[0].clip.name;
            Debug.Log("Clip name: " + clipName);
            return clipName; // Now this works because return type is string!
        }
        
        Debug.LogWarning("No clip info found!");
        return ""; // Return empty string if nothing found
    }

    float GetClipLength(Animator animator)
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        
        if (clipInfo != null && clipInfo.Length > 0)
        {
            float clipLength = clipInfo[0].clip.length;
            Debug.Log("Clip length: " + clipLength);
            return clipLength;
        }
        
        Debug.LogWarning("No clip info found!");
        return 0f;
    }

    IEnumerator WalkToKitchen()
    {
        // Get the walking animation clip name
        string walkingClipName = GetClipName(walkingAnim);
        
        // Play walk animation
        walkingAnim.Play(walkingClipName);
        
        // Optional: wait one frame for animation to start
        yield return null;
        
        // Move forward for X seconds
        float walkDuration = 2f;
        float elapsed = 0f;

        while (elapsed < walkDuration)
        {
            transform.Translate(Vector3.forward * 1.5f * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("Finished walking, now making coffee!");
    }
}