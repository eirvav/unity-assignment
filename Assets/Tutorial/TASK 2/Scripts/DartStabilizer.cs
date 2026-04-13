using UnityEngine;

public class DartStabilizer : MonoBehaviour
{
    public Rigidbody rb;

    public float minSpeed = 1f;
    public float turnSpeed = 10f;

    void FixedUpdate()
    {
        if (rb.linearVelocity.sqrMagnitude < minSpeed * minSpeed)
            return;

        rb.MoveRotation(Quaternion.Slerp(
            rb.rotation,
            Quaternion.LookRotation(rb.linearVelocity),
            turnSpeed * Time.fixedDeltaTime
        ));
    }
}
