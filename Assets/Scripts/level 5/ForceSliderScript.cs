using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForceSliderScript : MonoBehaviour
{
    [Header("UI Settings")]
    public SpeedometerUI speedometerUI;
    public Slider slider;
    public TextMeshProUGUI sliderText;
    public Button confirmBtn;

    [Header("Physics Settings")]
    public Rigidbody rbBox;
    public float maxForce = 20f;
    public float forceMultiplier;
    public float confirmedForce = 0f; // Force that was confirmed

    [Header("Animator Settings")]
    public Animator animator;
    public Transform character;

    // private
    private float currentForce;
    private bool isApplyingForce = false; // Whether to apply force

    void Start()
    {
        slider.onValueChanged.AddListener((v) =>
        {
            currentForce = v;
            sliderText.text = (v * 10).ToString("0") + " N";
        });

        confirmBtn.onClick.AddListener(ConfirmForce);
    }

    void FixedUpdate()
    {
        if (isApplyingForce)
        {
            // applies force on rigidbodies
            ApplyForce(confirmedForce);
        }
    }

    void ConfirmForce()
    {
        confirmedForce = currentForce;
        isApplyingForce = true;

        if (speedometerUI != null)
        {
            // updates force for the speedometer ui
            speedometerUI.SetCurrentForce(confirmedForce);
        }
    }

    void ApplyForce(float sliderForce)
    {
        float normalizedForce = sliderForce / maxForce;
        animator.SetFloat("InputMagnitude", normalizedForce);

        if (rbBox != null)
        {
            Vector3 pushDirection = -rbBox.transform.forward;
            rbBox.AddForce(pushDirection * (sliderForce * forceMultiplier), ForceMode.Force);
        }
    }

    public void ResetForce()
    {
        confirmedForce = 0f;
        isApplyingForce = false;
    }
}