using UnityEngine;

public class AppleAnimationController : MonoBehaviour
{
    [Header("Apple Setup")]
    public GameObject applePrefab;
    public Transform appleSpawnPoint;   // Where the apple appears initially
    public Transform headTarget;         // Character's head transform

    [Header("Apple Physics")]
    public float fallForce = 5f;
    public float bounceForce = 3f;

    private GameObject currentApple;
    private Rigidbody appleRb;

    // Animation Event 1 — called at animation start
    public void SpawnApple()
    {
        if (currentApple != null) return;

        currentApple = Instantiate(applePrefab, appleSpawnPoint.position, Quaternion.identity);
        appleRb = currentApple.GetComponent<Rigidbody>();

        appleRb.useGravity = false;
        appleRb.linearVelocity = Vector3.zero;
    }

    // Animation Event 2 — called when character touches her head
    public void DropApple()
    {
        if (appleRb == null) return;

        appleRb.useGravity = true;

        Vector3 direction = (headTarget.position - currentApple.transform.position).normalized;
        appleRb.AddForce(direction * fallForce, ForceMode.Impulse);
    }

    // Animation Event 3 — called right after impact
    public void BounceApple()
    {
        if (appleRb == null) return;

        Vector3 bounceDirection = (currentApple.transform.position - headTarget.position).normalized;
        appleRb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);

        Invoke(nameof(DestroyApple), 5f);
    }

    private void DestroyApple()
    {
        if (currentApple != null)
        {
            Destroy(currentApple);
            currentApple = null;
            appleRb = null;
        }
    }
}