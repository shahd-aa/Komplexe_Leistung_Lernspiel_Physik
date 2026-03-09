using UnityEngine;

public class MovementScript : MonoBehaviour
{
    public float speed = 5f;
    public Rigidbody rb;

    void FixedUpdate()
    {
        // Move in world-space forward (0,0,1)
        Vector3 movement = transform.forward * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

}
