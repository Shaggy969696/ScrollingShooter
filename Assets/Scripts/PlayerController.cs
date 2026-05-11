using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 50f;
    public float drag = 5f;
    public float maxSpeed = 20f;
    
    [Header("Dash")]
    public float dashSpeed = 50f;
    public float dashDuration = 0.2f;
    public float doubleTapTime = 0.3f;

    private Vector2 moveInput;
    private Vector3 velocity;
    private Camera mainCamera;

    // Dash tracking
    private float dashTimer;
    private bool isDashing;
    private Vector3 dashDirection;
    
    // Double tap tracking
    private float lastTapTimeW, lastTapTimeA, lastTapTimeS, lastTapTimeD;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("Main Camera not found!");
    }

    public void OnMove(InputValue value)
    {
        Vector2 prevInput = moveInput;
        moveInput = value.Get<Vector2>();

        // Detect tap (transition from 0 to direction)
        CheckForDash(prevInput, moveInput);
    }

    private void CheckForDash(Vector2 prev, Vector2 current)
    {
        if (isDashing) return;

        // Check each direction for a fresh press
        if (current.y > 0 && prev.y <= 0) TryDash(ref lastTapTimeW, Vector3.forward);
        if (current.y < 0 && prev.y >= 0) TryDash(ref lastTapTimeS, Vector3.back);
        if (current.x < 0 && prev.x >= 0) TryDash(ref lastTapTimeA, Vector3.left);
        if (current.x > 0 && prev.x <= 0) TryDash(ref lastTapTimeD, Vector3.right);
    }

    private void TryDash(ref float lastTapTime, Vector3 direction)
    {
        if (Time.time - lastTapTime < doubleTapTime)
        {
            StartDash(direction);
        }
        lastTapTime = Time.time;
    }

    private void StartDash(Vector3 direction)
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = direction;
        velocity = direction * dashSpeed; // Instant velocity boost
        Debug.Log($"Dash! Direction: {direction}");
    }

    private void Update()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }
        else
        {
            // Apply acceleration
            Vector3 targetAccel = new Vector3(moveInput.x, 0, moveInput.y) * acceleration;
            velocity += targetAccel * Time.deltaTime;

            // Apply drag
            velocity -= velocity * drag * Time.deltaTime;

            // Clamp speed
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }
        }

        // Apply movement
        transform.position += velocity * Time.deltaTime;

        // Clamping to camera view
        if (mainCamera != null)
        {
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            viewportPos.x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
            viewportPos.y = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);
            
            Vector3 clampedWorldPos = mainCamera.ViewportToWorldPoint(viewportPos);
            clampedWorldPos.y = 5f; 
            
            // If hitting the edge, kill velocity in that direction
            if (viewportPos.x <= 0.051f || viewportPos.x >= 0.949f) velocity.x = 0;
            if (viewportPos.y <= 0.051f || viewportPos.y >= 0.949f) velocity.z = 0;

            transform.position = clampedWorldPos;
        }
    }
}
