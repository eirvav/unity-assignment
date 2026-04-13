using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DartProjectile : MonoBehaviour
{
    [Header("References")]
    public Transform tip;

    [Header("Flight")]
    public float minAlignSpeed = 0.05f;

    [Header("Stick")]
    public float stickDepth = 0.01f;

    private Rigidbody _rb;
    private bool _isStuck;

    public bool IsStuck => _isStuck;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Launch(Vector3 velocity)
    {
        _isStuck = false;

        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.linearVelocity = velocity;
        _rb.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (_isStuck)
            return;

        Vector3 velocity = _rb.linearVelocity;
        if (velocity.sqrMagnitude < minAlignSpeed * minAlignSpeed)
            return;

        transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isStuck)
            return;

        DartTarget target = collision.collider.GetComponentInParent<DartTarget>();
        if (target == null)
            return;

        ContactPoint hit = collision.GetContact(0);
        StickIntoTarget(collision, hit, target);
    }

    private void StickIntoTarget(Collision collision, ContactPoint hit, DartTarget target)
    {
        _isStuck = true;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.isKinematic = true;

        Transform boardRef = target.boardCenter != null ? target.boardCenter : target.transform;

        // Use the board's face direction, not the collider normal.
        Vector3 boardForward = boardRef.forward;

        // Dart should point into the board.
        Vector3 desiredForward = -boardForward;

        Vector3 upHint = boardRef.up;
        if (Vector3.Cross(desiredForward, upHint).sqrMagnitude < 0.0001f)
            upHint = Vector3.up;

        transform.rotation = Quaternion.LookRotation(desiredForward, upHint);

        // Push the tip slightly into the board face.
        Vector3 desiredTipPosition = hit.point + desiredForward * stickDepth;

        if (tip != null)
        {
            Vector3 rootOffsetFromTip = transform.position - tip.position;
            transform.position = desiredTipPosition + rootOffsetFromTip;
        }
        else
        {
            transform.position = desiredTipPosition - transform.forward * 0.08f;
        }

        transform.SetParent(collision.collider.transform, true);

        target.RegisterHit(this, hit.point);
    }
}