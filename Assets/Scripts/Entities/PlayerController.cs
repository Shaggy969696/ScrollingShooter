// ============================================================
// PlayerController.cs
// FIX: Se reemplaza OnAttack(InputValue) por suscripción directa
// a los eventos performed/canceled de la acción "Attack".
// Con SendMessages, la fase canceled no se enruta de forma fiable
// a través de InputValue, por lo que isFiring quedaba atascado en true.
// ============================================================

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

    private WeaponController weaponController;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private Vector3 velocity;
    private Camera mainCamera;

    private float dashTimer;
    private bool isDashing;
    private Vector3 dashDirection;

    private float lastTapTimeW, lastTapTimeA, lastTapTimeS, lastTapTimeD;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null) Debug.LogError("PlayerInput no encontrado en el Player.");
    }

    private void OnEnable()
    {
        // Suscripción directa a performed (press) y canceled (release)
        // Esto garantiza que ambas fases se reciban independientemente
        // del Behavior configurado en el componente PlayerInput.
        if (playerInput == null) return;
        playerInput.actions["Attack"].performed += OnAttackPerformed;
        playerInput.actions["Attack"].canceled  += OnAttackCanceled;
    }

    private void OnDisable()
    {
        if (playerInput == null) return;
        playerInput.actions["Attack"].performed -= OnAttackPerformed;
        playerInput.actions["Attack"].canceled  -= OnAttackCanceled;
    }

    // Fase performed: Space presionado → comienza el disparo
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
        => weaponController?.OnFirePressed();

    // Fase canceled: Space suelto → detiene el disparo
    private void OnAttackCanceled(InputAction.CallbackContext ctx)
        => weaponController?.OnFireReleased();

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("Main Camera no encontrada!");

        weaponController = GetComponent<WeaponController>();
        if (weaponController == null) Debug.LogWarning("WeaponController no encontrado en el Player.");
    }

    // ─── Input Callbacks (SendMessages — se mantienen para Move, Next, Previous) ──

    public void OnMove(InputValue value)
    {
        Vector2 prevInput = moveInput;
        moveInput = value.Get<Vector2>();
        CheckForDash(prevInput, moveInput);
    }

    public void OnNext(InputValue value)
    {
        if (value.isPressed)
            weaponController?.SwitchNext();
    }

    public void OnPrevious(InputValue value)
    {
        if (value.isPressed)
            weaponController?.SwitchPrevious();
    }

    // ─── Dash ─────────────────────────────────────────────────────────────────

    private void CheckForDash(Vector2 prev, Vector2 current)
    {
        if (isDashing) return;

        if (current.y > 0 && prev.y <= 0) TryDash(ref lastTapTimeW, Vector3.forward);
        if (current.y < 0 && prev.y >= 0) TryDash(ref lastTapTimeS, Vector3.back);
        if (current.x < 0 && prev.x >= 0) TryDash(ref lastTapTimeA, Vector3.left);
        if (current.x > 0 && prev.x <= 0) TryDash(ref lastTapTimeD, Vector3.right);
    }

    private void TryDash(ref float lastTapTime, Vector3 direction)
    {
        if (Time.time - lastTapTime < doubleTapTime)
            StartDash(direction);
        lastTapTime = Time.time;
    }

    private void StartDash(Vector3 direction)
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = direction;
        velocity = direction * dashSpeed;
        Debug.Log($"Dash! Direction: {direction}");
    }

    // ─── Movement & Camera Clamp ──────────────────────────────────────────────

    private void Update()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) isDashing = false;
        }
        else
        {
            Vector3 targetAccel = new Vector3(moveInput.x, 0, moveInput.y) * acceleration;
            velocity += targetAccel * Time.deltaTime;
            velocity -= velocity * drag * Time.deltaTime;

            if (velocity.magnitude > maxSpeed)
                velocity = velocity.normalized * maxSpeed;
        }

        transform.position += velocity * Time.deltaTime;

        if (mainCamera != null)
        {
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            viewportPos.x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
            viewportPos.y = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);

            Vector3 clampedWorldPos = mainCamera.ViewportToWorldPoint(viewportPos);
            clampedWorldPos.y = 5f;

            if (viewportPos.x <= 0.051f || viewportPos.x >= 0.949f) velocity.x = 0;
            if (viewportPos.y <= 0.051f || viewportPos.y >= 0.949f) velocity.z = 0;

            transform.position = clampedWorldPos;
        }
    }
}