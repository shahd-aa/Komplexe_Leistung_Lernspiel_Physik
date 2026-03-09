using UnityEngine;
using TMPro;

public class BikeForceVisualizer : MonoBehaviour
{
    [Header("References")]
    public RectTransform forceArrow;      // the UI arrow - this will be scaled on x for length
    public TextMeshProUGUI forceText;     // text showing the force (optional)

    [Header("Input Settings")]
    public Vector3 currentVelocityInput;
    public float bikeMass = 20f;
    public float forceDeadzone = 0.1f; // values below this are treated as zero

    [Header("Visualization Settings")]
    public float forceScale = 0.1f;
    public float minArrowLength = 0.5f;
    public float maxArrowLength = 5f;

    [Header("Smoothing")]
    public float scaleLerpSpeed = 5f;    // how fast arrow length lerps
    public float rotLerpSpeed = 10f;     // how fast arrow rotates
    public float textFadeWithScale = 1f; // (optional) multiply alpha by this factor

    [Header("Debug Info")]
    public bool showDebugInfo = true;

    private Vector3 previousVelocity;
    private Vector3 internalVelocity;

    // persistent target length computed when velocity changes
    private float currentTargetLength = 0f;
    // last force vector (used to compute desired rotation)
    private Vector3 lastForce = Vector3.zero;

    void Start()
    {
        internalVelocity = currentVelocityInput;
        previousVelocity = internalVelocity;

        // set initial scale to something safe
        if (forceArrow != null)
        {
            Vector3 s = forceArrow.localScale;
            forceArrow.localScale = new Vector3(Mathf.Max(s.x, 0.0001f), s.y, s.z);
        }
    }

    void FixedUpdate()
    {
        // detect inspector or animation changes of velocity
        if (internalVelocity != currentVelocityInput)
            SetVelocity(currentVelocityInput);

        // continuously lerp arrow scale & rotation toward targets so shrink/grow is smooth
        ApplyScaleAndRotation();
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        internalVelocity = newVelocity;
        OnVelocityChanged();
    }

    void OnVelocityChanged()
    {
        // compute acceleration and force using fixedDeltaTime (physics-consistent)
        Vector3 acceleration = (internalVelocity - previousVelocity) / Time.fixedDeltaTime;
        Vector3 totalForce = acceleration * bikeMass;

        previousVelocity = internalVelocity;

        // compute signed/absolute value along x (you can change axis as needed)
        float signedForce = totalForce.x;
        float absForce = Mathf.Abs(signedForce);

        // apply deadzone: if under deadzone, target length -> 0 (arrow will shrink smoothly)
        if (absForce < forceDeadzone)
        {
            currentTargetLength = 0f;
        }
        else
        {
            // compute desired length from absolute force
            float computed = absForce * forceScale;
            currentTargetLength = Mathf.Clamp(computed, minArrowLength, maxArrowLength);
        }

        // store last force so rotation can be derived each frame
        lastForce = totalForce;

        // update force text (shows signed x force so you can see braking negative values)
        if (forceText != null)
            forceText.text = signedForce.ToString("F2") + " N";
    }

    void ApplyScaleAndRotation()
    {
        if (forceArrow == null) return;

        // current scale.x
        float currentScaleX = forceArrow.localScale.x;
        // lerp toward target length
        float newScaleX = Mathf.Lerp(currentScaleX, currentTargetLength, Time.deltaTime * scaleLerpSpeed);

        // ensure scale is non-negative (we avoid negative scale to prevent text flipping)
        newScaleX = Mathf.Max(newScaleX, 0f);

        // apply scale (only x axis)
        forceArrow.localScale = new Vector3(newScaleX, forceArrow.localScale.y, forceArrow.localScale.z);

        // rotation: compute desired angle from lastForce vector (2D UI uses z rotation)
        Quaternion targetRot = forceArrow.localRotation;
        if (lastForce.sqrMagnitude > 0.000001f)
        {
            // use atan2(y, x) so arrow points along the force direction in UI local space
            float angle = Mathf.Atan2(lastForce.y, lastForce.x) * Mathf.Rad2Deg;
            targetRot = Quaternion.Euler(0f, 0f, angle);
        }

        // smoothly rotate toward target
        forceArrow.localRotation = Quaternion.Lerp(forceArrow.localRotation, targetRot, Time.deltaTime * rotLerpSpeed);

        // keep text readable and unstretched:
        if (forceText != null)
        {
            forceText.rectTransform.rotation = Quaternion.identity;

            // counter-scale the text horizontally so it doesn't appear stretched when arrow scales
            // avoid division by zero
            float inv = (newScaleX > 0.0001f) ? (1f / newScaleX) : 1f;
            // clamp inv so text doesn't explode visually
            inv = Mathf.Clamp(inv, 0.2f, 10f);
            forceText.rectTransform.localScale = new Vector3(inv, 1f, 1f);

            // optionally fade text alpha based on arrow length (so it vanishes gently)
            Color c = forceText.color;
            float alpha = Mathf.Clamp01(newScaleX / Mathf.Max(maxArrowLength, 0.0001f)) * textFadeWithScale;
            c.a = alpha;
            forceText.color = c;
        }
    }
}
