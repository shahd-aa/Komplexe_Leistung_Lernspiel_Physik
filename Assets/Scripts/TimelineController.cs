// 06.11.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    public PlayableDirector playableDirector;

    void Start()
    {
        // Ensure the timeline is paused at the start
        playableDirector.time = 0;
        playableDirector.Pause();

        // Start the timeline after a delay
        StartCoroutine(StartTimelineAfterDelay(2f)); // Adjust the delay as needed
    }

    private System.Collections.IEnumerator StartTimelineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        playableDirector.Play();
    }
}