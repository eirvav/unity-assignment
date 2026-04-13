using System;
using System.Collections.Generic;
using UnityEngine;

public class DartTarget : MonoBehaviour
{
    [Header("Board Space")]
    [Tooltip("Center of the scoring face. Forward should point out of the board.")]
    public Transform boardCenter;

    [Header("Ring Radii (local-space units)")]
    public float bullseyeRadius = 0.03f;
    public float innerRingRadius = 0.08f;
    public float middleRingRadius = 0.14f;
    public float outerRingRadius = 0.22f;

    // Prevents the same dart from being registered multiple times.
    private readonly HashSet<int> _registeredDarts = new HashSet<int>();

    public enum HitZone
    {
        Miss,
        Outer,
        Middle,
        Inner,
        Bullseye
    }

    /// <summary>
    /// Students can subscribe to this from their own score manager.
    /// Example: target.DartHit += OnDartHit;
    /// </summary>
    public event Action<HitZone, Vector3> DartHit;

    private void Reset()
    {
        boardCenter = transform;
    }

    public void RegisterHit(DartProjectile dart, Vector3 hitPointWorld)
    {
        if (dart == null)
            return;

        int id = dart.GetInstanceID();
        if (_registeredDarts.Contains(id))
            return;

        _registeredDarts.Add(id);

        HitZone zone = EvaluateHit(hitPointWorld, out float radius);

        Debug.Log($"Dart hit {zone} at radius {radius:F3}", this);

        DartHit?.Invoke(zone, hitPointWorld);
    }

    public HitZone EvaluateHit(Vector3 hitPointWorld, out float radius)
    {
        Transform reference = boardCenter != null ? boardCenter : transform;

        Vector3 localPoint = reference.InverseTransformPoint(hitPointWorld);

        // We measure hit distance on the board face plane.
        radius = new Vector2(localPoint.x, localPoint.y).magnitude;

        if (radius <= bullseyeRadius)
            return HitZone.Bullseye;

        if (radius <= innerRingRadius)
            return HitZone.Inner;

        if (radius <= middleRingRadius)
            return HitZone.Middle;

        if (radius <= outerRingRadius)
            return HitZone.Outer;

        return HitZone.Miss;
    }

    public void ResetRegisteredDarts()
    {
        _registeredDarts.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform reference = boardCenter != null ? boardCenter : transform;

        DrawRing(reference, bullseyeRadius, Color.red);
        DrawRing(reference, innerRingRadius, Color.yellow);
        DrawRing(reference, middleRingRadius, Color.green);
        DrawRing(reference, outerRingRadius, Color.white);
    }

    private void DrawRing(Transform reference, float radius, Color color)
    {
        if (reference == null)
            return;

        Gizmos.color = color;

        const int steps = 64;
        Vector3 prev = reference.TransformPoint(new Vector3(radius, 0f, 0f));

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps * Mathf.PI * 2f;
            Vector3 nextLocal = new Vector3(Mathf.Cos(t) * radius, Mathf.Sin(t) * radius, 0f);
            Vector3 next = reference.TransformPoint(nextLocal);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}