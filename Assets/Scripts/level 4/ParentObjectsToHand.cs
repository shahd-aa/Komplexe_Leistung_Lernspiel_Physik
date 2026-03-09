using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class ParentObjectsToHand : MonoBehaviour
{
    [Header("Manual Setup")]
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject coffee;
    public Rigidbody rbCoffee;
    public GameObject spoon;
    public PlayableDirector director;

    private Quaternion coffeeOriginalRotation;
    private Quaternion spoonOriginalRotation;

    // Lerp settings
    [Header("Smooth Rotation Speed")]
    public float rotationLerpSpeed = 5f; // Adjust this for faster/slower rotation

    [Header("Coffee Cup Rotation")]
    public Vector3 targetCoffeeRotation = new Vector3(0, 0, 0); // Set in inspector!

    void Start()
    {
        // Store original rotations
        if (coffee != null)
            coffeeOriginalRotation = coffee.transform.rotation;
        if (spoon != null)
            spoonOriginalRotation = spoon.transform.rotation;

        if (rbCoffee != null)
            rbCoffee.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void ParentCoffeeToHand()
    {
        if (coffee != null && leftHand != null)
        {
            coffee.transform.SetParent(leftHand.transform, true);
        }
    }

    public void DetachCoffeeFromHand()
    {
        if (coffee != null)
        {
            coffee.transform.SetParent(null, true);
            Quaternion targetQuat = Quaternion.Euler(targetCoffeeRotation);
            StartCoroutine(LerpRotation(coffee.transform, targetQuat));
        }
    }

    public void ParentSpoonToHand()
    {
        if (spoon != null && rightHand != null)
        {
            spoon.transform.SetParent(rightHand.transform, true);
        }
    }

    public void DetachSpoonFromHand()
    {
        if (spoon != null)
        {
            spoon.transform.SetParent(null, true);
            StartCoroutine(LerpRotation(spoon.transform, spoonOriginalRotation));
        }
    }

    public void RotateCoffeeToTarget()
    {
        if (coffee != null)
        {
            Quaternion targetQuat = Quaternion.Euler(targetCoffeeRotation);
            StartCoroutine(LerpRotation(coffee.transform, targetQuat));
        }
    }

    // Smooth rotation lerp coroutine
    private IEnumerator LerpRotation(Transform obj, Quaternion targetRotation)
    {
        Quaternion startRotation = obj.rotation;
        float timeElapsed = 0f;

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime * rotationLerpSpeed;
            obj.rotation = Quaternion.Lerp(startRotation, targetRotation, timeElapsed);
            yield return null;
        }

        // Ensure it ends exactly at target rotation
        obj.rotation = targetRotation;
    }
}