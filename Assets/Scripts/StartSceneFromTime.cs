using UnityEngine;
using UnityEngine.Playables; 

public class StartSceneFromTime : MonoBehaviour
{
    [Header("Timeline Settings")]
    public PlayableDirector timeline;  // Drag your Timeline here
    public double startTime;      // Time in seconds to start from

    void Start()
    {
        if (timeline == null)
        {
            Debug.LogWarning("No Timeline assigned!");
            return;
        }

        // Set the timeline's time to the start time
        timeline.time = startTime;

        // Play the timeline from that time
        timeline.Play();
    }
}
