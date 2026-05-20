// ============================================================
// PlayerEntity.cs
// Gestiona la vida del Player: daño, muerte y flash blanco.
// Separado de PlayerController (que solo maneja input/movimiento)
// siguiendo el principio de responsabilidad única.
//
// Hereda de Entity → implementa IDamageable automáticamente.
// Cualquier proyectil o enemigo con acceso a IDamageable puede
// dañar al player sin conocer esta clase concretamente.
//
// Prefab setup:
//   Agregar este componente al mismo GameObject raíz del Player.
//   El Renderer se busca automáticamente en hijos.
// ============================================================

using System;
using System.Collections;
using UnityEngine;

public class PlayerEntity : Entity
{
    [Header("Hit Flash")]
    [Tooltip("Duración del flash blanco al recibir daño")]
    [SerializeField] private float flashDuration = 0.12f;

    private Renderer  playerRenderer;
    private Color     originalColor;
    private Coroutine flashCoroutine;

    // ── Evento para la UI ─────────────────────────────────────────────────────
    // Parámetros: (currentHealth, maxHealth)
    public event Action<float, float> OnHealthChanged;

    protected override void Awake()
    {
        base.Awake();
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
        else
            Debug.LogWarning($"{gameObject.name}: no se encontró Renderer para el flash del player.");
    }

    private void Start()
    {
        // Notifica la UI con los valores iniciales al arrancar
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ── Daño / Flash ──────────────────────────────────────────────────────────

    protected override void OnDamageTaken(float amount)
    {
        Debug.Log($"[Player] Recibió {amount} de daño. HP: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashWhite());
    }

    private IEnumerator FlashWhite()
    {
        if (playerRenderer == null) yield break;
        playerRenderer.material.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        playerRenderer.material.color = originalColor;
        flashCoroutine = null;
    }

    // ── Muerte ────────────────────────────────────────────────────────────────

    protected override void Die()
    {
        Debug.Log("[Player] Game Over.");
        OnHealthChanged?.Invoke(0f, maxHealth);
        // TODO: pantalla de Game Over
        gameObject.SetActive(false);
    }
}
