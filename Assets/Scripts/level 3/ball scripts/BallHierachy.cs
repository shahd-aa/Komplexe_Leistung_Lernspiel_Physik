using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class BallHierachy : MonoBehaviour
{
    public GameObject ball;
    public GameObject parent;
    public Vector3 force;
    private Rigidbody rb;

    public PlayableDirector director;

    // timeline seconds instead of realtime seconds
    public double timelineParentTime = 7.35; // seconds on the timeline when the ball should parent
    public double timelineDetachTime = 8.95; // seconds on the timeline when the ball should be thrown
    public double timelineChangeMassTime = 10; // seconds on timeline when the ball should stop rolling

    private bool applyImpulse = false;

    void Start()
    {
        rb = ball.GetComponent<Rigidbody>();
        ball.transform.SetParent(null);
        rb.isKinematic = true;

        // Make physics stable
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        StartCoroutine(ParentObject());
    }

    IEnumerator ParentObject()
    {
        while (director == null)
            yield return null;

        // wait until timeline reaches the parent time
        yield return new WaitUntil(() => director.time >= timelineParentTime);

        // grab the ball
        ball.transform.SetParent(parent.transform);

        // wait until timeline reaches unparent time
        yield return new WaitUntil(() => director.time >= timelineDetachTime);

        // release ball (throw)
        ball.transform.SetParent(null);
        rb.isKinematic = false;
        rb.WakeUp();

        // reset physics BEFORE impulse
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // request throw on next physics step
        applyImpulse = true;

        // time until the ball stops rolling
        yield return new WaitUntil(() => director.time >= timelineChangeMassTime);

        rb.linearDamping = 2f;
        rb.angularDamping = 2f;
    }

    void FixedUpdate()
    {
        // apply the impulse EXACTLY once in the physics step
        if (applyImpulse)
        {
            rb.AddForce(force, ForceMode.Impulse);
            applyImpulse = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            Debug.Log("ball reached the ground");
        }
    }
}
