using System.Collections.Generic;
using UnityEngine;

// ============================================================
// EnemySpawner.cs
// Gestiona el spawneo de enemigos desde sus pools.
// Colocar este script EN el objeto EnemiesSetter (X=0, Y=5, Z=0).
// Su transform es el origen del espacio de juego.
//
// API pública (para un futuro WaveManager u otros scripts):
//   SpawnShooter(xOffset)  → spawnea un ShooterEnemy en la columna X indicada
//   SpawnSwooper()         → spawnea un SwooperEnemy con dirección C aleatoria
//   RetireAllShooters()    → ordena retirada a todos los shooters activos
//
// Relaciones:
//   EnemySpawner --usa--> EnemyPool (x2, uno por tipo)
//   EnemySpawner --configura--> ShooterEnemy, SwooperEnemy
// ============================================================

public class EnemySpawner : MonoBehaviour
{
    // ── Pools ──────────────────────────────────────────────────────────────────
    [Header("Pools")]
    [Tooltip("Pool del ShooterEnemy")]
    [SerializeField] private EnemyPool shooterPool;

    [Tooltip("Pool del SwooperEnemy")]
    [SerializeField] private EnemyPool swooperPool;

    // ── Espacio de juego (relativo al transform de este objeto) ───────────────
    [Header("Área de juego")]
    [Tooltip("Mitad del ancho en X")]
    [SerializeField] private float halfWidth  = 20.5f;

    [Tooltip("Mitad del alto en Z")]
    [SerializeField] private float halfHeight = 36.5f;

    [Tooltip("Distancia extra fuera del borde desde la que spawnean los enemigos")]
    [SerializeField] private float spawnMargin = 12f;

    // ── ShooterEnemy ──────────────────────────────────────────────────────────
    [Header("Shooter Enemy")]
    [Tooltip("Máximo de ShooterEnemies activos simultáneamente")]
    [SerializeField] private int maxActiveShooters = 3;

    // Seguimiento de los shooters activos para poder ordenarles retirarse
    private readonly List<ShooterEnemy> activeShooters = new List<ShooterEnemy>();

    // Predicate cacheado para RemoveAll (evita allocations por frame)
    private readonly System.Predicate<ShooterEnemy> isInactive =
        s => s == null || !s.gameObject.activeSelf;

    // ── API pública ───────────────────────────────────────────────────────────

    // Spawnea un ShooterEnemy en la columna X (local a EnemiesSetter) indicada.
    // Ignorado si ya hay maxActiveShooters activos.
    public void SpawnShooter(float localX = 0f)
    {
        if (shooterPool == null) { Debug.LogError("[EnemySpawner] shooterPool no asignado."); return; }
        if (shooterPool.ActiveCount >= maxActiveShooters)
        {
            Debug.LogWarning("[EnemySpawner] Máximo de ShooterEnemies activos alcanzado.");
            return;
        }

        Enemy e = shooterPool.GetEnemy();
        if (e is not ShooterEnemy shooter)
        {
            Debug.LogError("[EnemySpawner] El prefab en shooterPool no tiene ShooterEnemy.");
            shooterPool.ReturnEnemy(e);
            return;
        }

        shooter.Initialize(shooterPool);

        Vector3 spawnWorld  = transform.position + new Vector3(localX, 0f, halfHeight + spawnMargin);
        float   topBorderZ  = transform.position.z + halfHeight;

        shooter.BeginEntry(spawnWorld, topBorderZ);
        activeShooters.Add(shooter);
    }

    // Spawnea un SwooperEnemy con dirección de C aleatoria.
    public void SpawnSwooper()
    {
        if (swooperPool == null) { Debug.LogError("[EnemySpawner] swooperPool no asignado."); return; }

        Enemy e = swooperPool.GetEnemy();
        if (e is not SwooperEnemy swooper)
        {
            Debug.LogError("[EnemySpawner] El prefab en swooperPool no tiene SwooperEnemy.");
            swooperPool.ReturnEnemy(e);
            return;
        }

        swooper.Initialize(swooperPool);

        bool    goRight    = Random.value > 0.5f;
        Vector3 spawnWorld = transform.position + new Vector3(0f, 0f,  halfHeight + spawnMargin);
        float   exitZ      = transform.position.z - halfHeight - spawnMargin;

        swooper.BeginSwoop(spawnWorld, exitZ, goRight);
    }

    // Ordena a todos los ShooterEnemies activos que se retiren hacia Z+.
    public void RetireAllShooters()
    {
        foreach (ShooterEnemy s in activeShooters)
            if (s != null && s.gameObject.activeSelf)
                s.Retreat();
    }

    // ── Unity ─────────────────────────────────────────────────────────────────

    // Limpia referencias a shooters que ya volvieron al pool (SetActive false)
    private void Update()
    {
        activeShooters.RemoveAll(isInactive);
    }
}
