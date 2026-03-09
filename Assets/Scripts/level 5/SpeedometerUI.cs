using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro

public class SpeedometerUI : MonoBehaviour
{
    [Header("References")]
    public Rigidbody trackedCrate;

    [Header("UI Elements")]
    public RectTransform needle; // The rotating needle image
    public TextMeshProUGUI speedText; // Or use Text if not using TMP
    public TextMeshProUGUI accelerationText;

    [Header("Settings")]
    public float maxSpeed = 100; // Maximum speed on gauge (m/s)
    public float minAngle = 49.18f; // Needle angle at 0 speed
    public float maxAngle = -225.6f;  // Needle angle at max speed

    private float currentForce = 0f;

    private float speed;
    private float acceleration;

    void Update()
    {
        speed = trackedCrate.linearVelocity.magnitude;

        acceleration = currentForce / trackedCrate.mass;

        // Update needle rotation
        UpdateNeedle(speed);

        // Update text displays
        speedText.text = $"{speed:F1} m/s";
        accelerationText.text = $"Beschleunigung: {acceleration:F2} m/s²";
    }

    void UpdateNeedle(float speed)
    {
        // Clamp speed to max
        speed = Mathf.Clamp(speed, 0f, maxSpeed);

        // Calculate percentage (0 to 1)
        float percentage = speed / maxSpeed;

        // Calculate angle
        float angle = Mathf.Lerp(minAngle, maxAngle, percentage);

        // Rotate needle
        needle.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetCurrentForce(float force)
    {
        currentForce = force;
    }
}