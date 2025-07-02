using UnityEngine;
using StarterAssets;
using System.Collections;
using UnityEngine.Events;

public class ThirdPersonTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("How far down must I fall to die and regenerate")]
    public float depthOfFallWhenIDie = -30.0f;

    private Transform lastTeleportTransform;
    private Vector3 lastTeleportOffset;
    private ThirdPersonController _controller;
    
    [Header("Events")]
    public UnityEvent<Transform> onTeleportationComplete;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize with current position as last teleport location
        lastTeleportTransform = transform.parent;
        lastTeleportOffset = transform.localPosition;
        _controller = GetComponent<ThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if we've fallen too low
        if (transform.position.y < depthOfFallWhenIDie)
        {
            RegenAtLastTeleportLocation(10);
        }
    }

    public void TeleportTo(Transform teleportTarget, Vector3 teleportOffset, int ticksToHoldHere)
    {
        lastTeleportTransform = teleportTarget.transform;
        lastTeleportOffset = teleportOffset;
        StartCoroutine(TeleportWithCallback(teleportTarget, ticksToHoldHere));
    }

    private IEnumerator TeleportWithCallback(Transform teleportTarget, int ticksToHoldHere)
    {
        RegenAtLastTeleportLocation(ticksToHoldHere);
        
        // Wait for teleportation to complete
        yield return new WaitForSeconds(ticksToHoldHere * 0.02f); // Assuming 50fps
        
        // Notify that teleportation is complete
        onTeleportationComplete?.Invoke(teleportTarget);
    }

    public void RegenAtLastTeleportLocation(int ticksToHoldHere)
    { 
        transform.SetParent(lastTeleportTransform, false);
        transform.localPosition = lastTeleportOffset;

        // VERY IMPORTANT: This is needed to enable transport to work.
        if (_controller != null)
            _controller.RestrictUpdates(ticksToHoldHere);
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f); // This usually triggers Idle
            animator.SetFloat("MotionSpeed", 0f); // Optional, if used for idle
            // If you have a specific Idle trigger, use:
            //animator.SetTrigger("Idle");
            animator.SetBool("Jump", true);
        }
    }
}
