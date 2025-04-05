using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 100f;

    public Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        // Calculate target velocity
        Vector2 targetVelocity = moveInput * moveSpeed;

        // Smoothly interpolate current velocity to target velocity
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            (moveInput.magnitude > 0 ? acceleration : deceleration) * Time.fixedDeltaTime
        );

        // Apply movement
        rb.linearVelocity = currentVelocity;
    }
}
