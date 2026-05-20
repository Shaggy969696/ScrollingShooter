// ============================================================
// Enemy.cs
// MODIFICADO: Die() suma el puntaje al ScoreManager antes de
// volver al pool. scoreValue se define por inspector en cada
// prefab (ShooterEnemy y SwooperEnemy tienen valores distintos).
// ============================================================

using System.Collections;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Hit Flash")]
    [Tooltip("Duración del flash blanco en segundos")]
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Puntaje")]
    [Tooltip("Puntos que otorga este enemigo al ser destruido por el player")]
    [SerializeField] private int scoreValue = 100;

    private Renderer  enemyRenderer;
    private Color     originalColor;
    private Coroutine flashCoroutine;
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

    public void Initialize(EnemyPool pool)
    {
        originPool    = pool;
        currentHealth = maxHealth;
        OnSpawn();
    }

    protected virtual void OnSpawn() { }

    protected override void Die()
    {
        Debug.Log($"{gameObject.name} destruido! +{scoreValue} puntos.");

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.Add(scoreValue);
        else
            Debug.LogWarning("[Enemy] ScoreManager no encontrado en la escena.");

        ReturnToPool();
    }

    protected void ReturnToPool()
    {
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
            Destroy(gameObject);
    }

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