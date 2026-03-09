using UnityEngine;
using System.Collections;

public class RandomAppleAnimation : MonoBehaviour
{
    public Animator animator;

    [Header("Random Timing (seconds)")]
    public float minDelay = 5f;
    public float maxDelay = 15f;

    private Coroutine routine;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        routine = StartCoroutine(RandomAppleRoutine());
    }

    private IEnumerator RandomAppleRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            animator.SetTrigger("AppleFall");
        }
    }
}
