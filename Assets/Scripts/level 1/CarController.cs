using UnityEngine;

public class CarController : MonoBehaviour, IPausable
{
    [Header("Start")]
    public float startDelay = 2f;   // seconds to wait before moving

    [Header("Speed")]
    public float acceleration = 1.5f;
    public float maxSpeed = 6f;

    [Header("Turn")]
    public float timeBeforeTurn = 5f;    // seconds of straight driving
    public float turnDegrees = 90f;   // how many degrees to turn
    public float turnDuration = 1.8f;  // seconds to complete the turn

    [Header("Post-Turn")]
    public float driveAfterTurn = 4f;    // keep going after turn for clarity

    // ── private state ──────────────────────────────────────────────
    private Rigidbody rb;
    private float speed = 0f;
    private float elapsed = 0f;
    private float turnProgress = 0f;
    private bool turning = false;
    private bool done = false;

    private Quaternion rotAtTurnStart;
    private Quaternion rotAtTurnEnd;

    // to be able to be paused when timeline pauses
    private bool isPaused = false;

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        if (isPaused) return;

        float dt = Time.fixedDeltaTime;
        elapsed += dt;

        if (elapsed < startDelay) return;   // ← sit still during delay

        float driveElapsed = elapsed - startDelay;   // time since we actually started

        // ── Phase 1 : accelerate straight ──────────────────────────
        if (!turning && !done)
        {
            speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * dt);
            rb.MovePosition(rb.position + transform.forward * speed * dt);

            if (driveElapsed >= timeBeforeTurn)   // ← use offset time here
            {
                turning = true;
                rotAtTurnStart = transform.rotation;
                rotAtTurnEnd = Quaternion.Euler(0f, turnDegrees, 0f) * rotAtTurnStart;
            }
        }

        // ── Phase 2 : turn (car rotates, keeps moving forward) ─────
        else if (turning)
        {
            turnProgress += dt / turnDuration;
            turnProgress = Mathf.Clamp01(turnProgress);

            // Smooth eased rotation (ease-in-out feel)
            float t = Mathf.SmoothStep(0f, 1f, turnProgress);
            rb.MoveRotation(Quaternion.Slerp(rotAtTurnStart, rotAtTurnEnd, t));

            // Keep driving in the car's current (rotating) forward direction
            rb.MovePosition(rb.position + transform.forward * speed * dt);

            if (turnProgress >= 1f)
            {
                turning = true;  // repurpose flag to keep driving
                done = true;
            }
        }

        // ── Phase 3 : drive away after turn ────────────────────────
        else if (done)
        {
            rb.MovePosition(rb.position + transform.forward * speed * dt);
        }
    }
}