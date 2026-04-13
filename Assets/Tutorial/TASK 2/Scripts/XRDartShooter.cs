using UnityEngine;
using UnityEngine.InputSystem;

public class XRDartShooter : MonoBehaviour
{
    [Header("References")]
    public Transform controllerTransform;
    public Transform firePointOverride;
    public GameObject dartPrefab;
    public Transform activeDartsParent;

    [Header("Input")]
    public InputActionProperty fireAction;

    [Header("Shot Settings")]
    public float launchSpeed = 18f;
    public float fireCooldown = 0.15f;
    public float dartLifetime = 15f;

    private float _nextFireTime;
    private bool _enabledActionLocally;

    private void OnEnable()
    {
        InputAction action = fireAction.action;
        if (action != null)
        {
            if (!action.enabled)
            {
                action.Enable();
                _enabledActionLocally = true;
            }

            action.performed += OnFirePerformed;
        }
    }

    private void OnDisable()
    {
        InputAction action = fireAction.action;
        if (action != null)
        {
            action.performed -= OnFirePerformed;

            if (_enabledActionLocally && action.enabled)
                action.Disable();
        }

        _enabledActionLocally = false;
    }

    private void OnFirePerformed(InputAction.CallbackContext context)
    {
        TryShoot();
    }

    public void TryShoot()
    {
        Transform shootTransform = firePointOverride != null ? firePointOverride : controllerTransform;

        if (shootTransform == null || dartPrefab == null)
        {
            Debug.LogWarning("Missing controllerTransform/firePointOverride or dartPrefab.");
            return;
        }

        if (Time.time < _nextFireTime)
            return;

        _nextFireTime = Time.time + fireCooldown;

        GameObject dartObject = Instantiate(
            dartPrefab,
            shootTransform.position,
            shootTransform.rotation,
            activeDartsParent
        );

        IgnoreControllerCollisions(dartObject);

        DartProjectile projectile = dartObject.GetComponent<DartProjectile>();
        if (projectile != null)
        {
            projectile.Launch(shootTransform.forward * launchSpeed);
        }
        else
        {
            Rigidbody rb = dartObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.linearVelocity = shootTransform.forward * launchSpeed;
            }
        }

        Destroy(dartObject, dartLifetime);
    }

    private void IgnoreControllerCollisions(GameObject dartObject)
    {
        if (controllerTransform == null)
            return;

        Collider[] controllerColliders = controllerTransform.GetComponentsInChildren<Collider>(true);
        Collider[] dartColliders = dartObject.GetComponentsInChildren<Collider>(true);

        foreach (Collider controllerCol in controllerColliders)
        {
            if (controllerCol == null) continue;

            foreach (Collider dartCol in dartColliders)
            {
                if (dartCol == null) continue;
                Physics.IgnoreCollision(controllerCol, dartCol, true);
            }
        }
    }
}