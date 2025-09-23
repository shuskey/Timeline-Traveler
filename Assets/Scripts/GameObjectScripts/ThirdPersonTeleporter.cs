using UnityEngine;
using StarterAssets;
using System.Collections;
using UnityEngine.Events;
using Assets.Scripts.DataObjects;

public class ThirdPersonTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("How far down must I fall to die and regenerate")]
    public float depthOfFallWhenIDie = -30.0f;
    
    [Tooltip("Whether to show the teleport effect")]
    public bool showTeleportEffect = true;
    
    [Header("Teleport Effect")]
    [Tooltip("Particle system for teleport effect")]
    public ParticleSystem teleportEffect;
    
    [Header("Events")]
    public UnityEvent OnBeforeTeleport;
    public UnityEvent OnAfterTeleport;
    public UnityEvent<Transform> onTeleportationComplete;
    
    private ThirdPersonController _controller;
    private CharacterController _characterController;
    private Vector3 _lastSafePosition;
    private bool _hasLastSafePosition = false;
    
    void Start()
    {
        _controller = GetComponent<ThirdPersonController>();
        _characterController = GetComponent<CharacterController>();
        
        // Set initial safe position
        if (transform.position.y > depthOfFallWhenIDie)
        {
            _lastSafePosition = transform.position;
            _hasLastSafePosition = true;
        }
    }
    
    void Update()
    {
        float currentY = transform.position.y;
        
        // Check if player fell too far
        if (currentY < depthOfFallWhenIDie)
        {
            TeleportToSafePosition();
        }
        else if (currentY > depthOfFallWhenIDie + 10f && 
                 _controller != null && _controller.Grounded && 
                 transform.parent != null && 
                 _characterController != null && _characterController.velocity.y > -2f) // Only update safe position when truly safe
        {
            if (!_hasLastSafePosition || Vector3.Distance(_lastSafePosition, transform.position) > 1f)
            {
                _lastSafePosition = transform.position;
                _hasLastSafePosition = true;
            }
        }
    }
    
    public void TeleportToPosition(Vector3 targetPosition)
    {
        StartCoroutine(TeleportCoroutine(targetPosition, null));
    }
    
    public void TeleportTo(Transform teleportTarget, Vector3 teleportOffset, int ticksToHoldHere)
    {
        Vector3 targetPosition = teleportTarget.position + teleportOffset;
        StartCoroutine(TeleportCoroutine(targetPosition, teleportTarget, ticksToHoldHere));
    }
    
    public void TeleportToSafePosition()
    {
        if (_hasLastSafePosition)
        {
            TeleportToPosition(_lastSafePosition);
        }
        else
        {
            // Fallback to spawn position or zero
            TeleportToPosition(Vector3.zero);
        }
    }
    
    private IEnumerator TeleportCoroutine(Vector3 targetPosition, Transform teleportTarget = null, int ticksToHold = 0)
    {
        OnBeforeTeleport?.Invoke();
        
        if (showTeleportEffect && teleportEffect != null)
        {
            teleportEffect.Play();
            yield return new WaitForSeconds(0.5f);
        }
        
        // Disable character controller temporarily to prevent conflicts
        if (_characterController != null)
        {
            _characterController.enabled = false;
        }
        
        // Handle parenting for teleport targets
        if (teleportTarget != null)
        {
            transform.SetParent(teleportTarget, false);
            transform.localPosition = targetPosition - teleportTarget.position;
        }
        else
        {
            // Move the transform
            transform.position = targetPosition;
        }
        
        // Hold for specified ticks if requested
        if (ticksToHold > 0)
        {
            // Disable movement for the specified duration
            if (_controller != null)
            {
                var thirdPersonController = _controller.GetComponent<ThirdPersonController>();
                if (thirdPersonController != null)
                {
                    thirdPersonController.RestrictUpdates(ticksToHold);
                }
            }
            
            // Wait for the hold duration (approximate)
            float holdTime = ticksToHold * Time.fixedDeltaTime;
            yield return new WaitForSeconds(holdTime);
        }
        
        // Re-enable character controller
        if (_characterController != null)
        {
            yield return null; // Wait one frame
            _characterController.enabled = true;
        }
        
        OnAfterTeleport?.Invoke();
        onTeleportationComplete?.Invoke(teleportTarget);
    }
} 