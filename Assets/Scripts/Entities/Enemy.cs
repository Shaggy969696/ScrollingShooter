// ============================================================
// Enemy.cs
// FIX: OnDamageTaken() ahora lanza una Coroutine que hace un flash
// blanco en el Renderer del enemigo al recibir daño.
// Si ya hay un flash activo se cancela y reinicia (sin acumulación).
// ============================================================

using System.Collections;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Hit Flash")]
    [Tooltip("Duración del flash blanco en segundos")]
    [SerializeField] private float flashDuration = 0.1f;

    // Renderer del modelo del enemigo (se busca automáticamente en Awake)
    private Renderer enemyRenderer;
    // Color original del material para restaurarlo tras el flash
    private Color originalColor;
    // Referencia a la coroutine activa para poder cancelarla si hay hits rápidos
    private Coroutine flashCoroutine;

    protected override void Awake()
    {
        base.Awake();   // inicializa currentHealth en Entity

        // Busca el Renderer en este GameObject o en sus hijos (modelo 3D)
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;
        else
            Debug.LogWarning($"{gameObject.name}: no se encontró Renderer para el flash.");
    }

    // Qué hacer cuando la salud llega a 0
    protected override void Die()
    {
        Debug.Log($"{gameObject.name} destruido!");

        // TODO: reproducir efecto de explosión, sumar score, etc.
        Destroy(gameObject);
    }

    // Feedback visual/auditivo al recibir daño (de momento solo log)
    protected override void OnDamageTaken(float amount)
    {
        Debug.Log($"{gameObject.name} recibió {amount} de daño. HP: {currentHealth}/{maxHealth}");

        // Si ya hay un flash en curso, lo cancela y reinicia
        // (evita que hits rápidos dejen el enemigo atascado en blanco)
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashWhite());
    }

    // Cambia el color a blanco, espera flashDuration, luego restaura el original
    private IEnumerator FlashWhite()
    {
        if (enemyRenderer == null) yield break;

        enemyRenderer.material.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.material.color = originalColor;

        flashCoroutine = null;
    }
}