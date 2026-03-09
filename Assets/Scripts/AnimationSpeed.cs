// 05.11.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class AdjustAnimationSpeed : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component attached to the GameObject
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            // Set the animation speed to half the normal speed
            animator.speed = 0.5f;
        }
        else
        {
            Debug.LogError("Animator component not found on the GameObject.");
        }
    }
}