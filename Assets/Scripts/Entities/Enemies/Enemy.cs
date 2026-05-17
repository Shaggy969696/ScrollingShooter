// ============================================================
// Enemy.cs
// MODIFICADO: soporte para Object Pool.
//   - Initialize(pool) : restablece vida y llama al hook OnSpawn()
//   - ReturnToPool()   : limpia flash y devuelve al pool (Destroy si no hay pool)
//   - Die()            : ya no llama Destroy(), delega en ReturnToPool()
//   - OnSpawn()        : hook virtual para que las subclases reinicien estado
// ============================================================

using System.Collections;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Hit Flash")]
    [Tooltip("Duración del flash blanco en segundos")]
    [SerializeField] private float flashDuration = 0.1f;

    private Renderer   enemyRenderer;
    private Color      originalColor;
    private Coroutine  flashCoroutine;

    // Pool que gestiona este enemigo (asignado por EnemySpawner al activarlo)
    private EnemyPool originPool;

    protected override void Awake()
    {
        base.Awake();
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;
        else
            Debug.LogWarning($"{gameObject.name}: no se encontró Renderer para el flash.");
    }

    // ── API pública ───────────────────────────────────────────────────────────

    // Llamado por EnemySpawner tras sacar este enemigo del pool.
    // Restablece la vida y notifica a la subclase para que reinicie su estado.
    public void Initialize(EnemyPool pool)
    {
        originPool    = pool;
        currentHealth = maxHealth;
        OnSpawn();
    }

    // ── Hooks ─────────────────────────────────────────────────────────────────

    // Las subclases reinician sus variables de estado aquí.
    // Se llama desde Initialize(), ANTES de que el spawner fije posición/ruta.
    protected virtual void OnSpawn() { }

    protected override void Die()
    {
        Debug.Log($"{gameObject.name} destruido!");
        // TODO: efecto de explosión, sumar score, etc.
        ReturnToPool();
    }

    // ── Pool ──────────────────────────────────────────────────────────────────

    // Cancela el flash y devuelve el enemigo al pool (o lo destruye como fallback).
    // Accesible desde subclases (ej. SwooperEnemy al impactar sin morir).
    protected void ReturnToPool()
    {
        // Limpiar flash para no dejar el material atascado en blanco
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
            if (enemyRenderer != null)
                enemyRenderer.material.color = originalColor;
        }

        if (originPool != null)
            originPool.ReturnEnemy(this);
        else
            Destroy(gameObject);  // fallback: enemigo colocado en escena sin pool
    }

    // ── Daño / Flash ──────────────────────────────────────────────────────────

    protected override void OnDamageTaken(float amount)
    {
        Debug.Log($"{gameObject.name} recibió {amount} de daño. HP: {currentHealth}/{maxHealth}");

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashWhite());
    }

    private IEnumerator FlashWhite()
    {
        if (enemyRenderer == null) yield break;

        enemyRenderer.material.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.material.color = originalColor;
        flashCoroutine = null;
    }
}